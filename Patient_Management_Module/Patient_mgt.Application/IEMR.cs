using Patient_mgt.Domain;

namespace Patient_mgt.Application
{
    public interface IEMR
    {
        Task<IEnumerable<EMR>> GetAllEMRs();
        Task<EMR?> GetEMRById(int id);
        Task<IEnumerable<EMR>> GetEMRsByPatientId(int patientId);
        Task<IEnumerable<EMR>> GetEMRsByDoctorId(int doctorId);
        Task<EMR> AddEMR(EMR emr);
        Task UpdateEMR(int id, EMR emr);
        Task DeleteEMR(int id);
    }
}