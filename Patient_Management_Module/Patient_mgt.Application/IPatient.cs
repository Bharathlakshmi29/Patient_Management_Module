using Patient_mgt.Domain;

namespace Patient_mgt.Application
{
    public interface IPatient
    {
        Task<IEnumerable<Patient>> GetAllPatients();
        Task<Patient> GetPatientById(int id);
        Task<Patient> AddPatient(Patient patient);
        Task<Patient> UpdatePatient(int id, Patient patient);
        Task<bool> DeletePatient(int id);
    }
}
