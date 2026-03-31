using AutoMapper;
using Patient_mgt.Application;
using Patient_mgt.Domain;
using Patient_mgt.DTOs;

namespace Patient_mgt.Infrastructure
{
    public class EMRService : IEMRService
    {
        private readonly IEMR _repo;
        private readonly IMapper _mapper;

        public EMRService(IEMR repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<GetEMRDTO>> GetAllEMRs()
        {
            var emrs = await _repo.GetAllEMRs();
            return emrs.Select(MapToGetEMRDTO);
        }

        public async Task<GetEMRDTO?> GetEMRById(int id)
        {
            var emr = await _repo.GetEMRById(id);
            return emr != null ? MapToGetEMRDTO(emr) : null;
        }

        public async Task<IEnumerable<GetEMRDTO>> GetEMRsByPatientId(int patientId)
        {
            var emrs = await _repo.GetEMRsByPatientId(patientId);
            return emrs.Select(MapToGetEMRDTO);
        }

        public async Task<IEnumerable<GetEMRDTO>> GetEMRsByDoctorId(int doctorId)
        {
            var emrs = await _repo.GetEMRsByDoctorId(doctorId);
            return emrs.Select(MapToGetEMRDTO);
        }

        public async Task<EMRDTO> CreateEMR(CreateEMRDTO dto)
        {
            try
            {
                Console.WriteLine($"CreateEMR - ExistingConditions received: '{dto.ExistingConditions}'");
                
                var emr = new EMR
                {
                    PatientId = dto.PatientId,
                    DoctorId = dto.DoctorId,
                    VisitDate = dto.VisitDate,
                    Diagnosis = dto.Diagnosis,
                    ICDCode = dto.ICDCode,
                    Notes = dto.Notes,
                    ExistingConditions = dto.ExistingConditions,
                    PrescribedMedicines = dto.PrescribedMedicines.Select(pm => new PrescribedMedicine
                    {
                        MedicineName = pm.MedicineName,
                        Dosage = pm.Dosage,
                        Frequency = pm.Frequency,
                        Instructions = pm.Instructions
                    }).ToList()
                };

                Console.WriteLine($"CreateEMR - EMR ExistingConditions before save: '{emr.ExistingConditions}'");
                
                var created = await _repo.AddEMR(emr);
                
                Console.WriteLine($"CreateEMR - EMR ExistingConditions after save: '{created.ExistingConditions}'");
                
                return _mapper.Map<EMRDTO>(created);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating EMR: {ex.Message}", ex);
            }
        }

        public async Task UpdateEMR(int id, CreateEMRDTO dto)
        {
            var emr = new EMR
            {
                EMRId = id,
                PatientId = dto.PatientId,
                DoctorId = dto.DoctorId,
                VisitDate = dto.VisitDate,
                Diagnosis = dto.Diagnosis,
                ICDCode = dto.ICDCode,
                Notes = dto.Notes,
                ExistingConditions = dto.ExistingConditions,
                PrescribedMedicines = dto.PrescribedMedicines.Select(pm => new PrescribedMedicine
                {
                    MedicineName = pm.MedicineName,
                    Dosage = pm.Dosage,
                    Frequency = pm.Frequency,
                    Instructions = pm.Instructions
                }).ToList()
            };

            await _repo.UpdateEMR(id, emr);
        }

        public async Task<bool> DeleteEMR(int id)
        {
            try
            {
                await _repo.DeleteEMR(id);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private GetEMRDTO MapToGetEMRDTO(EMR emr)
        {
            return new GetEMRDTO
            {
                EMRId = emr.EMRId,
                PatientId = emr.PatientId,
                PatientName = $"{emr.Patient.FirstName} {emr.Patient.LastName}",
                DoctorId = emr.DoctorId,
                DoctorName = emr.Doctor.User.Name,
                Department = emr.Doctor.Department.ToString(),
                VisitDate = emr.VisitDate,
                Diagnosis = emr.Diagnosis,
                ICDCode = emr.ICDCode,
                Notes = emr.Notes,
                ExistingConditions = emr.ExistingConditions,
                CreatedAt = emr.CreatedAt,
                PrescribedMedicines = emr.PrescribedMedicines.Select(pm => new GetPrescribedMedicineDTO
                {
                    PrescribedMedicineId = pm.PrescribedMedicineId,
                    MedicineName = pm.MedicineName,
                    Dosage = pm.Dosage,
                    Frequency = pm.Frequency,
                    Instructions = pm.Instructions
                }).ToList()
            };
        }
    }
}