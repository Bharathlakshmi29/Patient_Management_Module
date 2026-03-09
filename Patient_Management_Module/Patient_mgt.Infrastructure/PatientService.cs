using AutoMapper;
using OfficeOpenXml;
using Patient_mgt.Application;
using Patient_mgt.Domain;
using Patient_mgt.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Patient_mgt.Infrastructure
{
    public class PatientService : IPatientService
    {
        private readonly IPatient _repo;
        private readonly IMapper _mapper;

        public PatientService(IPatient repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<GetPatientDTO>> GetAllPatients()
        {
            var patients = await _repo.GetAllPatients();
            return _mapper.Map<IEnumerable<GetPatientDTO>>(patients);
        }

        public async Task<GetPatientDTO?> GetPatientById(int id)
        {
            var patient = await _repo.GetPatientById(id);
            if (patient == null) return null;

            return _mapper.Map<GetPatientDTO>(patient);
        }

        public async Task<GetPatientDTO?> GetPatientByMRN(string mrn)
        {
            var patients = await _repo.GetAllPatients();
            var patient = patients.FirstOrDefault(p => p.MRN == mrn);
            if (patient == null) return null;

            return _mapper.Map<GetPatientDTO>(patient);
        }

        public async Task<PatientDTO> CreatePatient(CreatePatientDTO dto)
        {
            // Check for duplicate phone number
            var existingPatients = await _repo.GetAllPatients();
            if (existingPatients.Any(p => p.Phone == dto.Phone))
            {
                throw new InvalidOperationException($"A patient with phone number {dto.Phone} already exists.");
            }

            var patient = _mapper.Map<Patient>(dto);

            if (dto.Photo != null)
            {
                using var ms = new MemoryStream();
                await dto.Photo.CopyToAsync(ms);
                patient.Photo = ms.ToArray();
            }

            var created = await _repo.AddPatient(patient);
            created.MRN = GenerateMRN(created.PatientId);
            await _repo.UpdatePatient(created.PatientId, created);
            return _mapper.Map<PatientDTO>(created);
        }

        public async Task UpdatePatient(int id, CreatePatientDTO dto)
        {
            var existingPatient = await _repo.GetPatientById(id);
            if (existingPatient == null)
                throw new Exception($"Patient with ID {id} not found.");

            // Check for duplicate phone number (excluding current patient)
            var allPatients = await _repo.GetAllPatients();
            if (allPatients.Any(p => p.Phone == dto.Phone && p.PatientId != id))
            {
                throw new InvalidOperationException($"A patient with phone number {dto.Phone} already exists.");
            }

            // Update only the editable fields, preserve MRN and PatientId
            existingPatient.FirstName = dto.FirstName;
            existingPatient.LastName = dto.LastName;
            existingPatient.DateOfBirth = dto.DateOfBirth;
            existingPatient.Gender = dto.Gender;
            existingPatient.Phone = dto.Phone;
            existingPatient.Email = dto.Email;
            existingPatient.AddressLine1 = dto.AddressLine1;
            existingPatient.AddressLine2 = dto.AddressLine2;
            existingPatient.Country = dto.Country;
            existingPatient.State = dto.State;
            existingPatient.City = dto.City;
            existingPatient.Pincode = dto.Pincode;
            existingPatient.BloodGroup = dto.BloodGroup;
            if (Enum.TryParse<PatientStatus>(dto.Status, out var status))
            {
                existingPatient.Status = status;
            }
            else
            {
                existingPatient.Status = PatientStatus.Stable; // Default fallback
            }

            if (dto.Photo != null)
            {
                using var ms = new MemoryStream();
                await dto.Photo.CopyToAsync(ms);
                existingPatient.Photo = ms.ToArray();
            }

            await _repo.UpdatePatient(id, existingPatient);
        }

        public async Task<bool> DeletePatient(int id)
        {
            try
            {
                await _repo.DeletePatient(id);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<byte[]> ExportPatientsToExcel()
        {
            var patients = await _repo.GetAllPatients();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Patients");

            worksheet.Cells[1, 1].Value = "Patient ID";
            worksheet.Cells[1, 2].Value = "MRN";
            worksheet.Cells[1, 3].Value = "Full Name";
            worksheet.Cells[1, 4].Value = "Date of Birth";
            worksheet.Cells[1, 5].Value = "Age";
            worksheet.Cells[1, 6].Value = "Gender";
            worksheet.Cells[1, 7].Value = "Phone";
            worksheet.Cells[1, 8].Value = "Email";
            worksheet.Cells[1, 9].Value = "Address Line 1";
            worksheet.Cells[1, 10].Value = "Address Line 2";
            worksheet.Cells[1, 11].Value = "City";
            worksheet.Cells[1, 12].Value = "State";
            worksheet.Cells[1, 13].Value = "Country";
            worksheet.Cells[1, 14].Value = "Pincode";
            worksheet.Cells[1, 15].Value = "Blood Group";
            worksheet.Cells[1, 16].Value = "Status";

            using (var range = worksheet.Cells[1, 1, 1, 16])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            }

            int row = 2;
            foreach (var patient in patients)
            {
                worksheet.Cells[row, 1].Value = patient.PatientId;
                worksheet.Cells[row, 2].Value = patient.MRN;
                worksheet.Cells[row, 3].Value = $"{patient.FirstName} {patient.LastName}";
                worksheet.Cells[row, 4].Value = patient.DateOfBirth.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 5].Value = CalculateAge(patient.DateOfBirth);
                worksheet.Cells[row, 6].Value = patient.Gender;
                worksheet.Cells[row, 7].Value = patient.Phone;
                worksheet.Cells[row, 8].Value = patient.Email;
                worksheet.Cells[row, 9].Value = patient.AddressLine1;
                worksheet.Cells[row, 10].Value = patient.AddressLine2;
                worksheet.Cells[row, 11].Value = patient.City;
                worksheet.Cells[row, 12].Value = patient.State;
                worksheet.Cells[row, 13].Value = patient.Country;
                worksheet.Cells[row, 14].Value = patient.Pincode;
                worksheet.Cells[row, 15].Value = patient.BloodGroup;
                worksheet.Cells[row, 16].Value = patient.Status.ToString();
                row++;
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            return package.GetAsByteArray();
        }

        private int CalculateAge(DateTime dob)
        {
            var today = DateTime.Today;
            var age = today.Year - dob.Year;
            if (dob.Date > today.AddYears(-age)) age--;
            return age;
        }

        public string GenerateMRN(int patientId)
        {
            return $"MRN{patientId:D6}";
        }
    }
}
