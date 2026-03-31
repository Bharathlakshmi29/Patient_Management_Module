using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Patient_mgt.DTOs
{
    public class MedicalReportDTO
    {
        public int ReportId { get; set; }
        public int PatientId { get; set; }
        public string ReportType { get; set; } = string.Empty;
        public string ReportName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? FileUrl { get; set; }
        public string? CloudinaryPublicId { get; set; }
        public DateTime UploadedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateMedicalReportDTO
    {
        [Required]
        public int PatientId { get; set; }
        
        [Required]
        public int ReportType { get; set; }
        
        [Required]
        public string ReportName { get; set; } = string.Empty;
        
        public string? Description { get; set; }
        
        [Required]
        public IFormFile File { get; set; } = null!;
    }

    public class UpdateMedicalReportDTO
    {
        public int ReportType { get; set; }
        public string ReportName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public IFormFile? File { get; set; }
    }
}