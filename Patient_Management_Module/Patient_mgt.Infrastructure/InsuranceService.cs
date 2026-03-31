using AutoMapper;
using Patient_mgt.Application;
using Patient_mgt.Domain;
using Patient_mgt.DTOs;

namespace Patient_mgt.Infrastructure
{
    public class InsuranceService : IInsuranceService
    {
        private readonly IInsurance _insuranceRepository;
        private readonly IMapper _mapper;

        public InsuranceService(IInsurance insuranceRepository, IMapper mapper)
        {
            _insuranceRepository = insuranceRepository;
            _mapper = mapper;
        }

        public async Task<GetInsuranceDTO> CreateInsurance(CreateInsuranceDTO createInsuranceDTO)
        {
            var insurance = _mapper.Map<Insurance>(createInsuranceDTO);
            var createdInsurance = await _insuranceRepository.CreateInsurance(insurance);
            return _mapper.Map<GetInsuranceDTO>(createdInsurance);
        }

        public async Task<GetInsuranceDTO?> GetInsuranceById(int insuranceId)
        {
            var insurance = await _insuranceRepository.GetInsuranceById(insuranceId);
            return insurance == null ? null : _mapper.Map<GetInsuranceDTO>(insurance);
        }

        public async Task<List<GetInsuranceDTO>> GetInsurancesByPatientId(int patientId)
        {
            var insurances = await _insuranceRepository.GetInsurancesByPatientId(patientId);
            return _mapper.Map<List<GetInsuranceDTO>>(insurances);
        }

        public async Task<List<GetInsuranceDTO>> GetAllInsurances()
        {
            var insurances = await _insuranceRepository.GetAllInsurances();
            return _mapper.Map<List<GetInsuranceDTO>>(insurances);
        }

        public async Task<GetInsuranceDTO> UpdateInsurance(int insuranceId, UpdateInsuranceDTO updateInsuranceDTO)
        {
            var existingInsurance = await _insuranceRepository.GetInsuranceById(insuranceId);
            if (existingInsurance == null)
                throw new Exception("Insurance not found");

            _mapper.Map(updateInsuranceDTO, existingInsurance);
            var updatedInsurance = await _insuranceRepository.UpdateInsurance(existingInsurance);
            return _mapper.Map<GetInsuranceDTO>(updatedInsurance);
        }

        public async Task<bool> DeleteInsurance(int insuranceId)
        {
            return await _insuranceRepository.DeleteInsurance(insuranceId);
        }
    }
}
