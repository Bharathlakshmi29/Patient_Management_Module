using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Patient_mgt.DTOs;
using Patient_mgt.Infrastructure;

namespace Patient_Management_Module.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _service;

        public PatientController(IPatientService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = "Doctor,Staff,Admin")]
        public async Task<ActionResult<IEnumerable<GetPatientDTO>>> GetAllPatients()
        {
            var patients = await _service.GetAllPatients();
            return Ok(patients);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Doctor,Staff,Admin")]
        public async Task<ActionResult<GetPatientDTO>> GetPatientById(int id)
        {
            var patient = await _service.GetPatientById(id);

            if (patient == null)
                return NotFound($"Patient with ID {id} not found.");

            return Ok(patient);
        }

        [HttpGet("mrn/{mrn}")]
        [Authorize(Roles = "Doctor,Staff,Admin")]
        public async Task<ActionResult<GetPatientDTO>> GetPatientByMRN(string mrn)
        {
            var patient = await _service.GetPatientByMRN(mrn);

            if (patient == null)
                return NotFound($"Patient with MRN {mrn} not found.");

            return Ok(patient);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<ActionResult<PatientDTO>> CreatePatient(
            [FromForm] CreatePatientDTO dto)
        {
            try
            {
                var created = await _service.CreatePatient(dto);
                return CreatedAtAction(
                    nameof(GetPatientById),
                    new { id = created.PatientId },
                    created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> UpdatePatient(
            int id,
            [FromForm] CreatePatientDTO dto)
        {
            try
            {
                await _service.UpdatePatient(id, dto);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            var result = await _service.DeletePatient(id);

            if (!result)
                return NotFound($"Patient with ID {id} not found.");

            return NoContent();
        }

        [HttpGet("export")]
        [Authorize(Roles = "Staff,Admin,Doctor")]
        public async Task<IActionResult> ExportPatients()
        {
            var excelData = await _service.ExportPatientsToExcel();
            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Patients.xlsx");
        }

    }
}
