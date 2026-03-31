using Patient_mgt.DTOs;

namespace Patient_mgt.Infrastructure
{
    public interface IInsuranceService
    {
        Task<GetInsuranceDTO> CreateInsurance(CreateInsuranceDTO createInsuranceDTO);
        Task<GetInsuranceDTO?> GetInsuranceById(int insuranceId);
        Task<List<GetInsuranceDTO>> GetInsurancesByPatientId(int patientId);
        Task<List<GetInsuranceDTO>> GetAllInsurances();
        Task<GetInsuranceDTO> UpdateInsurance(int insuranceId, UpdateInsuranceDTO updateInsuranceDTO);
        Task<bool> DeleteInsurance(int insuranceId);
    }
}
