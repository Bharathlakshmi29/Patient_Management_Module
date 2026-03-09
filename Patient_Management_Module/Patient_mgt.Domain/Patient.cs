using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Patient_mgt.Domain
{
    public enum PatientStatus
    {
        Stable,
        Mild,
        Critical
    }

    public class Patient
    {
        [Key]
        public int PatientId { get; set; }   // Primary Key

        [Required]
        [MaxLength(20)]
        public string MRN { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [MaxLength(10)]
        public string Gender { get; set; } = string.Empty;

        [Required]
        [MaxLength(15)]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }

        [Required]
        [MaxLength(200)]
        public string AddressLine1 { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? AddressLine2 { get; set; }

        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string State { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Country { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Pincode { get; set; } = string.Empty;

        [MaxLength(10)]
        public string? BloodGroup { get; set; }

        [Required]
        public PatientStatus Status { get; set; } = PatientStatus.Stable;

        [Required]
        public bool IsActive { get; set; } = true;

        // Store image as binary in DB
        [Column(TypeName = "varbinary(max)")]
        public byte[]? Photo { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

  
}
