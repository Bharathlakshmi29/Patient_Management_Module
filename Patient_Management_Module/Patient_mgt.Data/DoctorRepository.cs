using Microsoft.EntityFrameworkCore;
using Patient_mgt.Domain;

namespace Patient_mgt.Data
{
    public class DoctorRepository
    {
        private readonly PatientContext _context;

        public DoctorRepository(PatientContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Doctor>> GetAllDoctors()
        {
            return await _context.Doctors
                .Include(d => d.User)
                .Where(d => d.IsActive && d.User.Role == UserRole.Doctor)
                .ToListAsync();
        }

        public async Task<Doctor?> GetDoctorById(int id)
        {
            return await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DoctorId == id && d.IsActive);
        }

        public async Task<Doctor?> GetDoctorByUserId(Guid userId)
        {
            return await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == userId && d.IsActive);
        }

        public async Task<Doctor> AddDoctor(Doctor doctor)
        {
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();
            return doctor;
        }

        public async Task UpdateDoctor(int id, Doctor doctor)
        {
            var existingDoctor = await _context.Doctors.FindAsync(id);
            if (existingDoctor != null)
            {
                existingDoctor.Specialization = doctor.Specialization;
                existingDoctor.Department = doctor.Department;
                existingDoctor.YearsOfExperience = doctor.YearsOfExperience;
                
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteDoctor(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor != null)
            {
                doctor.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }
    }
}