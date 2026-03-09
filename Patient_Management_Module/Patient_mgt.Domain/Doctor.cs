using System.ComponentModel.DataAnnotations;

namespace Patient_mgt.Domain
{
    public enum Department
    {
        GeneralMedicine,
        Cardiology,
        Neurology,
        Orthopedics,
        Pediatrics,
        Nephrology,
        Dermatology,
        Psychiatry,
        Oncology,
        Gastroenterology,
        Pulmonology,
        Endocrinology,
        Radiology,
        Anesthesiology,
        Surgery
    }

    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Specialization { get; set; } = string.Empty;

        [Required]
        public Department Department { get; set; }

        [Required]
        public int YearsOfExperience { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}