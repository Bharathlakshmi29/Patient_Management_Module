using Patient_mgt.Infrastructure.RAG;
using System.Text;

namespace Patient_mgt.Infrastructure
{
    public class ChatService
    {
        private readonly IRagService _ragService;
        private readonly IGeminiService _geminiService;
        private readonly IQueryRouter _queryRouter;
        private readonly IPatientDataService _patientService;

        public ChatService(
            IRagService ragService,
            IGeminiService geminiService,
            IQueryRouter queryRouter,
            IPatientDataService patientService)
        {
            _ragService = ragService;
            _geminiService = geminiService;
            _queryRouter = queryRouter;
            _patientService = patientService;
        }

        public async Task<string> AskAsync(string question)
        {
            var routing = _queryRouter.Route(question);

            string guidelineContext = "";
            string patientContext = "";

            // Fetch Patient EMR (SQL)
            if (routing.Intent == QueryIntent.PatientData ||
                routing.Intent == QueryIntent.Hybrid)
            {
                patientContext = await _patientService.GetPatientContext(
                    routing.PatientName,
                    routing.Mrn);
            }

            // If patient not found, fall back to RAG regardless of original intent
            bool patientFound = !string.IsNullOrWhiteSpace(patientContext)
                                && !patientContext.Contains("not found");

            if (routing.Intent == QueryIntent.Guideline ||
                routing.Intent == QueryIntent.Hybrid ||
                !patientFound)
            {
                var chunks = await _ragService.GetRelevantChunks(question);
                if (chunks != null && chunks.Any())
                    guidelineContext = string.Join("\n\n", chunks);
            }

            var finalPrompt = BuildPrompt(question, patientFound ? patientContext : "", guidelineContext);
            return await _geminiService.GenerateResponse(finalPrompt);
        }

        private string BuildPrompt(string question, string patientData, string guidelines)
        {
            var sb = new StringBuilder();

            sb.AppendLine("You are an AI assistant for doctors and hospital staff.");
            sb.AppendLine("Answer strictly using only the information provided below.");
            sb.AppendLine("Do NOT add information from your own training data.");
            sb.AppendLine("Do NOT mention patient data or its absence unless patient data is provided below.");
            sb.AppendLine("Focus your answer specifically on what the doctor is asking — do not summarize unrelated sections.");
            sb.AppendLine();

            if (!string.IsNullOrWhiteSpace(patientData))
            {
                sb.AppendLine("===== PATIENT DATA =====");
                sb.AppendLine(patientData);
                sb.AppendLine();
            }

            if (!string.IsNullOrWhiteSpace(guidelines))
            {
                sb.AppendLine("===== MEDICAL GUIDELINES =====");
                sb.AppendLine(guidelines);
                sb.AppendLine();
            }

            sb.AppendLine("===== DOCTOR QUESTION =====");
            sb.AppendLine(question);
            sb.AppendLine();
            sb.AppendLine("Answer based only on the above context:");

            return sb.ToString();
        }
    }
}