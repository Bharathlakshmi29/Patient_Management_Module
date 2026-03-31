using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Patient_mgt.DTOs;
using Patient_mgt.Infrastructure;

namespace Patient_Management_Module.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InsuranceController : ControllerBase
    {
        private readonly IInsuranceService _insuranceService;

        public InsuranceController(IInsuranceService insuranceService)
        {
            _insuranceService = insuranceService;
        }

        [HttpPost]
        public async Task<ActionResult<GetInsuranceDTO>> CreateInsurance([FromBody] CreateInsuranceDTO createInsuranceDTO)
        {
            try
            {
                var insurance = await _insuranceService.CreateInsurance(createInsuranceDTO);
                return CreatedAtAction(nameof(GetInsuranceById), new { id = insurance.InsuranceId }, insurance);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetInsuranceDTO>> GetInsuranceById(int id)
        {
            var insurance = await _insuranceService.GetInsuranceById(id);
            if (insurance == null)
                return NotFound(new { message = "Insurance not found" });

            return Ok(insurance);
        }

        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<List<GetInsuranceDTO>>> GetInsurancesByPatientId(int patientId)
        {
            var insurances = await _insuranceService.GetInsurancesByPatientId(patientId);
            return Ok(insurances);
        }

        [HttpGet]
        public async Task<ActionResult<List<GetInsuranceDTO>>> GetAllInsurances()
        {
            var insurances = await _insuranceService.GetAllInsurances();
            return Ok(insurances);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<GetInsuranceDTO>> UpdateInsurance(int id, [FromBody] UpdateInsuranceDTO updateInsuranceDTO)
        {
            try
            {
                var insurance = await _insuranceService.UpdateInsurance(id, updateInsuranceDTO);
                return Ok(insurance);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteInsurance(int id)
        {
            var result = await _insuranceService.DeleteInsurance(id);
            if (!result)
                return NotFound(new { message = "Insurance not found" });

            return NoContent();
        }
    }
}
