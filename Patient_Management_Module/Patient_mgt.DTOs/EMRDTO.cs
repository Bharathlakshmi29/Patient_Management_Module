using System.ComponentModel.DataAnnotations;

namespace Patient_mgt.DTOs
{
    public class CreateEMRDTO
    {
        [Required]
        public int PatientId { get; set; }

        [Required]
        public int DoctorId { get; set; }

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

        public List<CreatePrescribedMedicineDTO> PrescribedMedicines { get; set; } = new List<CreatePrescribedMedicineDTO>();
    }

    public class CreatePrescribedMedicineDTO
    {
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

    public class GetEMRDTO
    {
        public int EMRId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public string ICDCode { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<GetPrescribedMedicineDTO> PrescribedMedicines { get; set; } = new List<GetPrescribedMedicineDTO>();
    }

    public class GetPrescribedMedicineDTO
    {
        public int PrescribedMedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public string? Instructions { get; set; }
    }

    public class EMRDTO
    {
        public int EMRId { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateTime VisitDate { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public string ICDCode { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}