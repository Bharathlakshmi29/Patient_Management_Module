using Patient_mgt.DTOs;

namespace Patient_mgt.Infrastructure
{
    public interface IEMRService
    {
        Task<IEnumerable<GetEMRDTO>> GetAllEMRs();
        Task<GetEMRDTO?> GetEMRById(int id);
        Task<IEnumerable<GetEMRDTO>> GetEMRsByPatientId(int patientId);
        Task<IEnumerable<GetEMRDTO>> GetEMRsByDoctorId(int doctorId);
        Task<EMRDTO> CreateEMR(CreateEMRDTO dto);
        Task UpdateEMR(int id, CreateEMRDTO dto);
        Task<bool> DeleteEMR(int id);
    }
}