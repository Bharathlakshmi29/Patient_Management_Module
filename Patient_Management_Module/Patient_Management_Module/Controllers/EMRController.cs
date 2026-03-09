using Microsoft.AspNetCore.Mvc;
using Patient_mgt.DTOs;
using Patient_mgt.Infrastructure;

namespace Patient_Management_Module.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EMRController : ControllerBase
    {
        private readonly IEMRService _emrService;

        public EMRController(IEMRService emrService)
        {
            _emrService = emrService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetEMRDTO>>> GetAllEMRs()
        {
            try
            {
                var emrs = await _emrService.GetAllEMRs();
                return Ok(emrs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetEMRDTO>> GetEMRById(int id)
        {
            try
            {
                var emr = await _emrService.GetEMRById(id);
                if (emr == null)
                    return NotFound($"EMR with ID {id} not found");

                return Ok(emr);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<IEnumerable<GetEMRDTO>>> GetEMRsByPatientId(int patientId)
        {
            try
            {
                var emrs = await _emrService.GetEMRsByPatientId(patientId);
                return Ok(emrs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("doctor/{doctorId}")]
        public async Task<ActionResult<IEnumerable<GetEMRDTO>>> GetEMRsByDoctorId(int doctorId)
        {
            try
            {
                var emrs = await _emrService.GetEMRsByDoctorId(doctorId);
                return Ok(emrs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<EMRDTO>> CreateEMR([FromBody] CreateEMRDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var emr = await _emrService.CreateEMR(dto);
                return CreatedAtAction(nameof(GetEMRById), new { id = emr.EMRId }, emr);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEMR(int id, [FromBody] CreateEMRDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                await _emrService.UpdateEMR(id, dto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEMR(int id)
        {
            try
            {
                var result = await _emrService.DeleteEMR(id);
                if (!result)
                    return NotFound($"EMR with ID {id} not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}