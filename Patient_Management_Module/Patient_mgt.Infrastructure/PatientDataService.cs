using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Text;

namespace Patient_mgt.Infrastructure
{
    public interface IPatientDataService
    {
        Task<string> GetPatientContext(string? patientName, string? mrn);
    }

    public class PatientDataService : IPatientDataService
    {
        private readonly string _connectionString;

        public PatientDataService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("dbConn")
                ?? throw new InvalidOperationException("Connection string 'dbConn' not found.");
        }

        public async Task<string> GetPatientContext(string? patientName, string? mrn)
        {
            var patient = await FindPatient(patientName, mrn);

            if (patient == null)
                return "Patient not found in EMR.";

            var emrTask = GetEMRHistory(patient.PatientId);
            var medsTask = GetPrescribedMedicines(patient.PatientId);
            var reportsTask = GetMedicalReports(patient.PatientId);

            await Task.WhenAll(emrTask, medsTask, reportsTask);

            var sb = new StringBuilder();
            sb.AppendLine($"Patient: {patient.FirstName} {patient.LastName}");
            sb.AppendLine($"MRN: {patient.MRN}");
            sb.AppendLine($"DOB: {patient.DateOfBirth:yyyy-MM-dd}");
            sb.AppendLine($"Gender: {patient.Gender}");
            sb.AppendLine($"Blood Group: {patient.BloodGroup ?? "N/A"}");
            sb.AppendLine($"Status: {(patient.Status == 1 ? "Active" : "Inactive")}");
            sb.AppendLine();

            sb.AppendLine("---- EMR / Visit History ----");
            sb.AppendLine(emrTask.Result);
            sb.AppendLine();

            sb.AppendLine("---- Prescribed Medicines ----");
            sb.AppendLine(medsTask.Result);
            sb.AppendLine();

            sb.AppendLine("---- Medical Reports ----");
            sb.AppendLine(reportsTask.Result);

            return sb.ToString();
        }

        private async Task<PatientDto?> FindPatient(string? name, string? mrn)
        {
            using var conn = new NpgsqlConnection(_connectionString);

            const string cols = @"""PatientId"", ""MRN"", ""FirstName"", ""LastName"", ""DateOfBirth"", ""Gender"", ""BloodGroup"", ""Status""";

            if (!string.IsNullOrWhiteSpace(mrn))
            {
                return await conn.QueryFirstOrDefaultAsync<PatientDto>(
                    $@"SELECT {cols} FROM patients WHERE ""MRN"" = @mrn LIMIT 1",
                    new { mrn });
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 2)
                {
                    var fullName = string.Join(" ", parts);
                    var result = await conn.QueryFirstOrDefaultAsync<PatientDto>(
                        $@"SELECT {cols} FROM patients
                           WHERE (""FirstName"" || ' ' || ""LastName"") ILIKE @fullName
                           LIMIT 1",
                        new { fullName = $"%{fullName}%" });

                    if (result != null) return result;

                    return await conn.QueryFirstOrDefaultAsync<PatientDto>(
                        $@"SELECT {cols} FROM patients
                           WHERE ""FirstName"" ILIKE @first AND ""LastName"" ILIKE @last
                           LIMIT 1",
                        new { first = $"%{parts[0]}%", last = $"%{parts[^1]}%" });
                }

                return await conn.QueryFirstOrDefaultAsync<PatientDto>(
                    $@"SELECT {cols} FROM patients
                       WHERE ""FirstName"" ILIKE @name OR ""LastName"" ILIKE @name
                       LIMIT 1",
                    new { name = $"%{name}%" });
            }

            return null;
        }

        private async Task<string> GetEMRHistory(int patientId)
        {
            using var conn = new NpgsqlConnection(_connectionString);

            var rows = await conn.QueryAsync(
                @"SELECT ""VisitDate"", ""Diagnosis"", ""ICDCode"", ""Notes"", ""ExistingConditions""
                  FROM ""EMRs""
                  WHERE ""PatientId"" = @patientId
                  ORDER BY ""VisitDate"" DESC
                  LIMIT 5",
                new { patientId });

            if (!rows.Any()) return "No EMR records found.";

            var sb = new StringBuilder();
            foreach (var r in rows)
            {
                string conditions = r.ExistingConditions ?? "None";
                string notes = r.Notes ?? "None";
                sb.AppendLine($"[{r.VisitDate:yyyy-MM-dd}] {r.Diagnosis} (ICD: {r.ICDCode}) | Conditions: {conditions} | Notes: {notes}");
            }

            return sb.ToString();
        }

        private async Task<string> GetPrescribedMedicines(int patientId)
        {
            using var conn = new NpgsqlConnection(_connectionString);

            var rows = await conn.QueryAsync(
                @"SELECT pm.""MedicineName"", pm.""Dosage"", pm.""Frequency"", pm.""Instructions""
                  FROM ""PrescribedMedicines"" pm
                  INNER JOIN ""EMRs"" e ON e.""EMRId"" = pm.""EMRId""
                  WHERE e.""PatientId"" = @patientId
                  ORDER BY e.""VisitDate"" DESC
                  LIMIT 10",
                new { patientId });

            if (!rows.Any()) return "No prescribed medicines found.";

            var sb = new StringBuilder();
            foreach (var r in rows)
            {
                string instructions = r.Instructions ?? "No instructions";
                sb.AppendLine($"{r.MedicineName} - {r.Dosage} - {r.Frequency} | {instructions}");
            }

            return sb.ToString();
        }

        private async Task<string> GetMedicalReports(int patientId)
        {
            using var conn = new NpgsqlConnection(_connectionString);

            var rows = await conn.QueryAsync(
                @"SELECT ""ReportName"", ""ReportType"", ""Description"", ""UploadedAt"", ""AnalysisResult""
                  FROM ""MedicalReports""
                  WHERE ""PatientId"" = @patientId
                  ORDER BY ""UploadedAt"" DESC
                  LIMIT 5",
                new { patientId });

            if (!rows.Any()) return "No medical reports found.";

            var sb = new StringBuilder();
            foreach (var r in rows)
            {
                string description = r.Description ?? "No description";
                string analysis = r.AnalysisResult ?? "No analysis available";
                sb.AppendLine($"[{r.UploadedAt:yyyy-MM-dd}] {r.ReportName} ({r.ReportType}) | {description}");
                sb.AppendLine($"  Analysis: {analysis}");
            }

            return sb.ToString();
        }
    }

    public class PatientDto
    {
        public int PatientId { get; set; }
        public string MRN { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string? BloodGroup { get; set; }
        public int Status { get; set; }
    }
}
