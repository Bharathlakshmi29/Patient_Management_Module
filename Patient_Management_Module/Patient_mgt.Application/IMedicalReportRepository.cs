using Patient_mgt.Domain;

namespace Patient_mgt.Application
{
    public interface IMedicalReportRepository
    {
        Task<IEnumerable<MedicalReport>> GetReportsByPatientId(int patientId);
        Task<MedicalReport?> GetReportById(int reportId);
        Task<MedicalReport> CreateReport(MedicalReport report);
        Task UpdateReport(int reportId, MedicalReport report);
        Task DeleteReport(int reportId);
        Task UpdateAnalysisResult(int reportId, string analysisResult);
    }
}