using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Patient_mgt.Infrastructure;

namespace Patient_Management_Module.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;

        public ChatController(ChatService chatService)
        {
            _chatService = chatService;
            //_geminiService = geminiService;   //fallback if your knowledge base has no answer
        }

        [HttpGet("ask")]
        public async Task<IActionResult> Ask(string question)
        {
            

            try
            {
                // 🛑 Validate input
                if (string.IsNullOrWhiteSpace(question))
                    return BadRequest("Question cannot be empty.");

                var answer = await _chatService.AskAsync(question);
                // var answer = await _geminiService.GenerateRagAnswerAsync(question, chunks);

                return Ok(new { answer });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Chatbot failed to process request.",
                    details = ex.Message
                });
            }
        }
    }
}
