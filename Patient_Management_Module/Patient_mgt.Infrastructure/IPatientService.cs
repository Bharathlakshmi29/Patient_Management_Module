using Patient_mgt.Domain;
using Patient_mgt.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patient_mgt.Infrastructure
{
    public interface IPatientService
    {
        Task<IEnumerable<GetPatientDTO>> GetAllPatients();
        Task<GetPatientDTO> GetPatientById(int id);
        Task<GetPatientDTO> GetPatientByMRN(string mrn);
        Task<PatientDTO> CreatePatient(CreatePatientDTO patient);
        Task UpdatePatient(int id, CreatePatientDTO patient);
        Task<bool> DeletePatient(int id);
        Task<byte[]> ExportPatientsToExcel();
        string GenerateMRN(int patientId);
    }
}
