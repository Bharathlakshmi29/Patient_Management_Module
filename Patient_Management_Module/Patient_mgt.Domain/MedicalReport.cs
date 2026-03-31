using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Patient_mgt.Domain
{
    public class MedicalReport
    {
        [Key]
        public int ReportId { get; set; }

        [Required]
        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        [Required]
        public ReportType ReportType { get; set; }

        [Required]
        [MaxLength(200)]
        public string ReportName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(100)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string FileType { get; set; } = string.Empty;

        [Required]
        public long FileSize { get; set; }

        // Store file URL from Cloudinary
        [MaxLength(500)]
        public string? FileUrl { get; set; }

        // Cloudinary public ID for file management
        [MaxLength(200)]
        public string? CloudinaryPublicId { get; set; }

        [Required]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Store analysis result as JSON
        public string? AnalysisResult { get; set; }

        public DateTime? AnalyzedAt { get; set; }
    }

    public enum ReportType
    {
        LAB_REPORT,
        SCAN_REPORT,
        XRAY,
        MRI,
        CT_SCAN,
        ULTRASOUND,
        BLOOD_TEST,
        URINE_TEST,
        OTHER
    }
}