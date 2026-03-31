using Microsoft.EntityFrameworkCore;
using Patient_mgt.Application;
using Patient_mgt.Domain;

namespace Patient_mgt.Data
{
    public class InsuranceRepository : IInsurance
    {
        private readonly PatientContext _context;

        public InsuranceRepository(PatientContext context)
        {
            _context = context;
        }

        public async Task<Insurance> CreateInsurance(Insurance insurance)
        {
            _context.Insurances.Add(insurance);
            await _context.SaveChangesAsync();
            return insurance;
        }

        public async Task<Insurance?> GetInsuranceById(int insuranceId)
        {
            return await _context.Insurances
                .Include(i => i.Patient)
                .FirstOrDefaultAsync(i => i.InsuranceId == insuranceId);
        }

        public async Task<List<Insurance>> GetInsurancesByPatientId(int patientId)
        {
            return await _context.Insurances
                .Include(i => i.Patient)
                .Where(i => i.PatientId == patientId)
                .ToListAsync();
        }

        public async Task<List<Insurance>> GetAllInsurances()
        {
            return await _context.Insurances
                .Include(i => i.Patient)
                .ToListAsync();
        }

        public async Task<Insurance> UpdateInsurance(Insurance insurance)
        {
            insurance.UpdatedAt = DateTime.UtcNow;
            _context.Insurances.Update(insurance);
            await _context.SaveChangesAsync();
            return insurance;
        }

        public async Task<bool> DeleteInsurance(int insuranceId)
        {
            var insurance = await _context.Insurances.FindAsync(insuranceId);
            if (insurance == null) return false;

            _context.Insurances.Remove(insurance);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
