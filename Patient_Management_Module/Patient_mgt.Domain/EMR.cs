using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Patient_mgt.Domain
{
    public class EMR
    {
        [Key]
        public int EMRId { get; set; }

        [Required]
        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

        [Required]
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;

        [Required]
        public DateTime VisitDate { get; set; }

        [Required]
        [MaxLength(200)]
        public string Diagnosis { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string ICDCode { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [MaxLength(500)]
        public string? ExistingConditions { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<PrescribedMedicine> PrescribedMedicines { get; set; } = new List<PrescribedMedicine>();
    }

    public class PrescribedMedicine
    {
        [Key]
        public int PrescribedMedicineId { get; set; }

        [Required]
        public int EMRId { get; set; }
        public EMR EMR { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string MedicineName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Dosage { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Frequency { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Instructions { get; set; }
    }
}