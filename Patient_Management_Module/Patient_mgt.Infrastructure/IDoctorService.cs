using Patient_mgt.DTOs;

namespace Patient_mgt.Infrastructure
{
    public interface IDoctorService
    {
        Task<IEnumerable<GetDoctorDTO>> GetAllDoctors();
        Task<GetDoctorDTO> GetDoctorById(int id);
        Task<GetDoctorDTO> GetDoctorByUserId(Guid userId);
        Task<DoctorDTO> CreateDoctor(CreateDoctorDTO doctor);
        Task UpdateDoctor(int id, CreateDoctorDTO doctor);
        Task<bool> DeleteDoctor(int id);
        Task<byte[]> ExportDoctorsToExcel();
    }
}