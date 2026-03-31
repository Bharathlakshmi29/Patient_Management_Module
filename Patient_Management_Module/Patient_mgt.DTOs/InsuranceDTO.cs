using System.ComponentModel.DataAnnotations;
using Patient_mgt.Domain;

namespace Patient_mgt.DTOs
{
    public class CreateInsuranceDTO
    {
        [Required]
        public int PatientId { get; set; }

        [Required]
        public InsuranceProvider Provider { get; set; }

        [MaxLength(200)]
        public string? OtherProvider { get; set; }

        [Required]
        public InsuranceType InsuranceType { get; set; }

        [Required]
        public PlanType PlanType { get; set; }

        [Required]
        [MaxLength(100)]
        public string PolicyNumber { get; set; } = string.Empty;

        [Required]
        public PolicyHolderRelationship PolicyHolderRelationship { get; set; }
    }

    public class UpdateInsuranceDTO
    {
        [Required]
        public InsuranceProvider Provider { get; set; }

        [MaxLength(200)]
        public string? OtherProvider { get; set; }

        [Required]
        public InsuranceType InsuranceType { get; set; }

        [Required]
        public PlanType PlanType { get; set; }

        [Required]
        [MaxLength(100)]
        public string PolicyNumber { get; set; } = string.Empty;

        [Required]
        public PolicyHolderRelationship PolicyHolderRelationship { get; set; }
    }

    public class GetInsuranceDTO
    {
        public int InsuranceId { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public InsuranceProvider Provider { get; set; }
        public string? OtherProvider { get; set; }
        public InsuranceType InsuranceType { get; set; }
        public PlanType PlanType { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public PolicyHolderRelationship PolicyHolderRelationship { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
