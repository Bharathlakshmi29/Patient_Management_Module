using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Patient_mgt.Domain
{
    public class Insurance
    {
        [Key]
        public int InsuranceId { get; set; }

        [Required]
        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;

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

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }

    public enum InsuranceProvider
    {
        UNITED_HEALTHCARE,
        AETNA,
        BLUE_CROSS_BLUE_SHIELD,
        CIGNA,
        HUMANA,
        KAISER_PERMANENTE,
        MOLINA_HEALTHCARE,
        CENTENE,
        ANTHEM,
        OTHER
    }

    public enum InsuranceType
    {
        EMPLOYER_SPONSORED,
        INDIVIDUAL_MARKETPLACE,
        MEDICARE,
        MEDICAID,
        TRICARE,
        VA_HEALTHCARE,
        SELF_PAY,
        OTHER
    }

    public enum PlanType
    {
        HMO,
        PPO,
        EPO,
        POS,
        HDHP
    }

    public enum PolicyHolderRelationship
    {
        SELF,
        SPOUSE,
        PARENT,
        GUARDIAN,
        EMPLOYER,
        OTHER
    }
}
