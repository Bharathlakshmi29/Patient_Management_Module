using System.ComponentModel.DataAnnotations;

namespace Patient_mgt.DTOs
{
    public class DoctorDTO
    {
        public int DoctorId { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateDoctorDTO
    {
        [Required]
        public Guid UserId { get; set; }
        
        [Required]
        public string Specialization { get; set; } = string.Empty;
        
        [Required]
        public string Department { get; set; } = string.Empty;
        
        [Required]
        public int YearsOfExperience { get; set; }
    }

    public class GetDoctorDTO
    {
        public int DoctorId { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
    }
}