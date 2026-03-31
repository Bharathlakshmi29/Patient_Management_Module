using Patient_mgt.Domain;

namespace Patient_mgt.Application
{
    public interface IInsurance
    {
        Task<Insurance> CreateInsurance(Insurance insurance);
        Task<Insurance?> GetInsuranceById(int insuranceId);
        Task<List<Insurance>> GetInsurancesByPatientId(int patientId);
        Task<List<Insurance>> GetAllInsurances();
        Task<Insurance> UpdateInsurance(Insurance insurance);
        Task<bool> DeleteInsurance(int insuranceId);
    }
}
