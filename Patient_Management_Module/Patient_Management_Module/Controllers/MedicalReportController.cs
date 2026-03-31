using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Patient_mgt.DTOs;
using Patient_mgt.Infrastructure;

namespace Patient_Management_Module.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MedicalReportController : ControllerBase
    {
        private readonly IMedicalReportService _medicalReportService;

        public MedicalReportController(IMedicalReportService medicalReportService)
        {
            _medicalReportService = medicalReportService;
        }

        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<IEnumerable<MedicalReportDTO>>> GetReportsByPatientId(int patientId)
        {
            var reports = await _medicalReportService.GetReportsByPatientId(patientId);
            return Ok(reports);
        }

        [HttpGet("{reportId}")]
        public async Task<ActionResult<MedicalReportDTO>> GetReportById(int reportId)
        {
            var report = await _medicalReportService.GetReportById(reportId);
            if (report == null)
                return NotFound($"Medical report with ID {reportId} not found.");

            return Ok(report);
        }

        [HttpPost]
        public async Task<ActionResult<MedicalReportDTO>> CreateReport([FromForm] CreateMedicalReportDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var report = await _medicalReportService.CreateReport(dto);
            return CreatedAtAction(nameof(GetReportById), new { reportId = report.ReportId }, report);
        }

        [HttpPut("{reportId}")]
        public async Task<IActionResult> UpdateReport(int reportId, [FromForm] UpdateMedicalReportDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _medicalReportService.UpdateReport(reportId, dto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{reportId}")]
        public async Task<IActionResult> DeleteReport(int reportId)
        {
            var success = await _medicalReportService.DeleteReport(reportId);
            if (!success)
                return NotFound($"Medical report with ID {reportId} not found.");

            return NoContent();
        }

        [HttpGet("{reportId}/download")]
        public async Task<IActionResult> DownloadReport(int reportId)
        {
            try
            {
                var report = await _medicalReportService.GetReportById(reportId);
                if (report == null)
                    return NotFound($"Medical report with ID {reportId} not found.");

                var fileBytes = await _medicalReportService.DownloadReport(reportId);
                return File(fileBytes, report.FileType, report.FileName);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{reportId}/can-analyze")]
        public async Task<IActionResult> CanAnalyzeReport(int reportId)
        {
            var report = await _medicalReportService.GetReportById(reportId);
            if (report == null)
                return NotFound($"Medical report with ID {reportId} not found.");

            // Check if it's a lab report type
            bool canAnalyze = report.ReportType == "LAB_REPORT" || report.ReportType == "BLOOD_TEST" || report.ReportType == "URINE_TEST";
            
            return Ok(new { canAnalyze, reportType = report.ReportType });
        }
    }
}