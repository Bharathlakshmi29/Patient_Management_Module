using AutoMapper;
using OfficeOpenXml;
using Patient_mgt.Data;
using Patient_mgt.Domain;
using Patient_mgt.DTOs;

namespace Patient_mgt.Infrastructure
{
    public class DoctorService : IDoctorService
    {
        private readonly DoctorRepository _repo;
        private readonly IMapper _mapper;

        public DoctorService(DoctorRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<GetDoctorDTO>> GetAllDoctors()
        {
            var doctors = await _repo.GetAllDoctors();
            return _mapper.Map<IEnumerable<GetDoctorDTO>>(doctors);
        }

        public async Task<GetDoctorDTO?> GetDoctorById(int id)
        {
            var doctor = await _repo.GetDoctorById(id);
            if (doctor == null) return null;

            return _mapper.Map<GetDoctorDTO>(doctor);
        }

        public async Task<GetDoctorDTO?> GetDoctorByUserId(Guid userId)
        {
            var doctor = await _repo.GetDoctorByUserId(userId);
            if (doctor == null) return null;

            return _mapper.Map<GetDoctorDTO>(doctor);
        }

        public async Task<DoctorDTO> CreateDoctor(CreateDoctorDTO dto)
        {
            var doctor = _mapper.Map<Doctor>(dto);
            var created = await _repo.AddDoctor(doctor);
            return _mapper.Map<DoctorDTO>(created);
        }

        public async Task UpdateDoctor(int id, CreateDoctorDTO dto)
        {
            var doctor = _mapper.Map<Doctor>(dto);
            doctor.DoctorId = id;
            await _repo.UpdateDoctor(id, doctor);
        }

        public async Task<bool> DeleteDoctor(int id)
        {
            try
            {
                await _repo.DeleteDoctor(id);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<byte[]> ExportDoctorsToExcel()
        {
            var doctors = await _repo.GetAllDoctors();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Doctors");

            worksheet.Cells[1, 1].Value = "Doctor ID";
            worksheet.Cells[1, 2].Value = "Full Name";
            worksheet.Cells[1, 3].Value = "Specialization";
            worksheet.Cells[1, 4].Value = "Department";
            worksheet.Cells[1, 5].Value = "Years of Experience";
            worksheet.Cells[1, 6].Value = "Email";

            using (var range = worksheet.Cells[1, 1, 1, 6])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
            }

            int row = 2;
            foreach (var doctor in doctors)
            {
                worksheet.Cells[row, 1].Value = doctor.DoctorId;
                worksheet.Cells[row, 2].Value = doctor.User.Name;
                worksheet.Cells[row, 3].Value = doctor.Specialization;
                worksheet.Cells[row, 4].Value = doctor.Department.ToString();
                worksheet.Cells[row, 5].Value = doctor.YearsOfExperience;
                worksheet.Cells[row, 6].Value = doctor.User.EmailId;
                row++;
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            return package.GetAsByteArray();
        }
    }
}