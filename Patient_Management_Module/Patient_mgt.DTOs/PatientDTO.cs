using System.ComponentModel.DataAnnotations;

namespace Patient_mgt.DTOs
{
    public class PatientDTO
    {
        public int PatientId { get; set; }
        public string MRN { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public string? BloodGroup { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreatePatientDTO
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        public DateTime DateOfBirth { get; set; }
        
        [Required]
        public string Gender { get; set; } = string.Empty;
        
        [Required]
        public string Phone { get; set; } = string.Empty;
        
        public string? Email { get; set; }
        
        [Required]
        public string AddressLine1 { get; set; } = string.Empty;
        
        public string? AddressLine2 { get; set; }
        
        [Required]
        public string City { get; set; } = string.Empty;
        
        [Required]
        public string State { get; set; } = string.Empty;
        
        [Required]
        public string Country { get; set; } = string.Empty;
        
        [Required]
        public string Pincode { get; set; } = string.Empty;
        
        public string? BloodGroup { get; set; }
        
        public string Status { get; set; } = string.Empty;
        
        public Microsoft.AspNetCore.Http.IFormFile? Photo { get; set; }
    }

    public class GetPatientDTO
    {
        public int PatientId { get; set; }
        public string MRN { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public string? BloodGroup { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
    }
}
