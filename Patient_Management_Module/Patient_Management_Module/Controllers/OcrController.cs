using Microsoft.AspNetCore.Mvc;
using Patient_mgt.Infrastructure;

namespace Patient_Management_Module.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OcrController : ControllerBase
    {
        private readonly IOcrService _ocrService;

        public OcrController(IOcrService ocrService)
        {
            _ocrService = ocrService;
        }

        [HttpPost("extract-text")]
        public async Task<IActionResult> ExtractText(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            try
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var imageData = memoryStream.ToArray();

                var extractedText = await _ocrService.ExtractTextFromImageAsync(imageData);

                return Ok(new { text = extractedText });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error processing image: {ex.Message}");
            }
        }
    }
}