namespace Patient_mgt.DTOs
{
    public class IcdCodeDTO
    {
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DisplayText => $"{Code} - {Description}";
    }
}