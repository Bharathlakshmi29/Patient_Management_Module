using Microsoft.EntityFrameworkCore;
using Patient_mgt.Application;
using Patient_mgt.Data;
using Patient_mgt.Domain;
using System.Numerics;

namespace Patient_mgt.Infrastructure
{
    public class PatientRepository : IPatient
    {
        private readonly PatientContext _context;

        public PatientRepository(PatientContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Patient>> GetAllPatients()
        {
            return await _context.patients.ToListAsync();
        }

        public async Task<Patient> GetPatientById(int id)
        {
            return await _context.patients.FirstOrDefaultAsync(d => d.PatientId == id) ?? throw new KeyNotFoundException();
        }

       
        public async Task<Patient> AddPatient(Patient patient)
        {
            await _context.patients.AddAsync(patient);
            await _context.SaveChangesAsync();
            return patient;
        }

        public async Task<Patient> UpdatePatient(int id, Patient patient)
        {
            if(id!= patient.PatientId) throw new KeyNotFoundException();
            _context.patients.Update(patient);
            await _context.SaveChangesAsync();
            return patient;

        }

        public async Task<bool> DeletePatient(int id)
        {
            var patient = await _context.patients.FindAsync(id);

            if (patient == null)
                throw new KeyNotFoundException();

            _context.patients.Remove(patient);
            await _context.SaveChangesAsync();

            return true;
        }

      
    }
}
