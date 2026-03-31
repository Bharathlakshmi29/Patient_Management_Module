using Patient_mgt.DTOs;

namespace Patient_mgt.Infrastructure
{
    public interface IMedicalReportService
    {
        Task<IEnumerable<MedicalReportDTO>> GetReportsByPatientId(int patientId);
        Task<MedicalReportDTO?> GetReportById(int reportId);
        Task<MedicalReportDTO> CreateReport(CreateMedicalReportDTO dto);
        Task UpdateReport(int reportId, UpdateMedicalReportDTO dto);
        Task<bool> DeleteReport(int reportId);
        Task<byte[]> DownloadReport(int reportId);
    }
}