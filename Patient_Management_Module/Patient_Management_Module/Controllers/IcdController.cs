using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Patient_mgt.DTOs;
using Patient_mgt.Infrastructure;

namespace Patient_Management_Module.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class IcdController : ControllerBase
    {
        private readonly IIcdService _icdService;

        public IcdController(IIcdService icdService)
        {
            _icdService = icdService;
        }

        [HttpGet("search")]
        public async Task<ActionResult<List<IcdCodeDTO>>> SearchIcdCodes([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return Ok(new List<IcdCodeDTO>());

            var results = await _icdService.SearchIcdCodes(query);
            return Ok(results);
        }
    }
}