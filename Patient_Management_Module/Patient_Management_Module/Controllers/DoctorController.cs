using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Patient_mgt.DTOs;
using Patient_mgt.Infrastructure;

namespace Patient_Management_Module.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GetDoctorDTO>>> GetAllDoctors()
        {
            var doctors = await _doctorService.GetAllDoctors();
            return Ok(doctors);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetDoctorDTO>> GetDoctorById(int id)
        {
            var doctor = await _doctorService.GetDoctorById(id);
            if (doctor == null)
                return NotFound();

            return Ok(doctor);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<GetDoctorDTO>> GetDoctorByUserId(Guid userId)
        {
            var doctor = await _doctorService.GetDoctorByUserId(userId);
            if (doctor == null)
                return NotFound();

            return Ok(doctor);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<DoctorDTO>> CreateDoctor([FromBody] CreateDoctorDTO createDoctorDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var doctor = await _doctorService.CreateDoctor(createDoctorDto);
            return CreatedAtAction(nameof(GetDoctorById), new { id = doctor.DoctorId }, doctor);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDoctor(int id, [FromBody] CreateDoctorDTO updateDoctorDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _doctorService.UpdateDoctor(id, updateDoctorDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var result = await _doctorService.DeleteDoctor(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportDoctorsToExcel()
        {
            var excelData = await _doctorService.ExportDoctorsToExcel();
            return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Doctors.xlsx");
        }
    }
}