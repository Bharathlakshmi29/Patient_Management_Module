using Patient_mgt.Domain;

namespace Patient_mgt.Application
{
    public interface IMedicalReport
    {
        Task<MedicalReport> CreateMedicalReport(MedicalReport medicalReport);
        Task<MedicalReport?> GetMedicalReportById(int reportId);
        Task<List<MedicalReport>> GetMedicalReportsByPatientId(int patientId);
        Task<List<MedicalReport>> GetAllMedicalReports();
        Task<MedicalReport> UpdateMedicalReport(MedicalReport medicalReport);
        Task<bool> DeleteMedicalReport(int reportId);
        Task<byte[]?> GetReportFile(int reportId);
    }
}