using Microsoft.EntityFrameworkCore;
using Patient_mgt.Application;
using Patient_mgt.Domain;

namespace Patient_mgt.Data
{
    public class EMRRepository : IEMR
    {
        private readonly PatientContext _context;

        public EMRRepository(PatientContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EMR>> GetAllEMRs()
        {
            return await _context.EMRs
                .Include(e => e.Patient)
                .Include(e => e.Doctor)
                .ThenInclude(d => d.User)
                .Include(e => e.PrescribedMedicines)
                .ToListAsync();
        }

        public async Task<EMR?> GetEMRById(int id)
        {
            return await _context.EMRs
                .Include(e => e.Patient)
                .Include(e => e.Doctor)
                .ThenInclude(d => d.User)
                .Include(e => e.PrescribedMedicines)
                .FirstOrDefaultAsync(e => e.EMRId == id);
        }

        public async Task<IEnumerable<EMR>> GetEMRsByPatientId(int patientId)
        {
            return await _context.EMRs
                .Include(e => e.Patient)
                .Include(e => e.Doctor)
                .ThenInclude(d => d.User)
                .Include(e => e.PrescribedMedicines)
                .Where(e => e.PatientId == patientId)
                .OrderByDescending(e => e.VisitDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<EMR>> GetEMRsByDoctorId(int doctorId)
        {
            return await _context.EMRs
                .Include(e => e.Patient)
                .Include(e => e.Doctor)
                .ThenInclude(d => d.User)
                .Include(e => e.PrescribedMedicines)
                .Where(e => e.DoctorId == doctorId)
                .OrderByDescending(e => e.VisitDate)
                .ToListAsync();
        }

        public async Task<EMR> AddEMR(EMR emr)
        {
            _context.EMRs.Add(emr);
            await _context.SaveChangesAsync();
            return emr;
        }

        public async Task UpdateEMR(int id, EMR emr)
        {
            var existingEMR = await _context.EMRs
                .Include(e => e.PrescribedMedicines)
                .FirstOrDefaultAsync(e => e.EMRId == id);

            if (existingEMR != null)
            {
                existingEMR.Diagnosis = emr.Diagnosis;
                existingEMR.ICDCode = emr.ICDCode;
                existingEMR.Notes = emr.Notes;
                existingEMR.VisitDate = emr.VisitDate;

                // Update prescribed medicines
                _context.PrescribedMedicines.RemoveRange(existingEMR.PrescribedMedicines);
                existingEMR.PrescribedMedicines = emr.PrescribedMedicines;

                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteEMR(int id)
        {
            var emr = await _context.EMRs.FindAsync(id);
            if (emr != null)
            {
                _context.EMRs.Remove(emr);
                await _context.SaveChangesAsync();
            }
        }
    }
}