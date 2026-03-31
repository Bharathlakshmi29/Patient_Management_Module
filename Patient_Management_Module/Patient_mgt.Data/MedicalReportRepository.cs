using Microsoft.EntityFrameworkCore;
using Patient_mgt.Application;
using Patient_mgt.Domain;

namespace Patient_mgt.Data
{
    public class MedicalReportRepository : IMedicalReportRepository
    {
        private readonly PatientContext _context;

        public MedicalReportRepository(PatientContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MedicalReport>> GetReportsByPatientId(int patientId)
        {
            return await _context.MedicalReports
                .Where(r => r.PatientId == patientId)
                .OrderByDescending(r => r.UploadedAt)
                .ToListAsync();
        }

        public async Task<MedicalReport?> GetReportById(int reportId)
        {
            return await _context.MedicalReports
                .FirstOrDefaultAsync(r => r.ReportId == reportId);
        }

        public async Task<MedicalReport> CreateReport(MedicalReport report)
        {
            _context.MedicalReports.Add(report);
            await _context.SaveChangesAsync();
            return report;
        }

        public async Task UpdateReport(int reportId, MedicalReport report)
        {
            var existingReport = await _context.MedicalReports.FindAsync(reportId);
            if (existingReport != null)
            {
                _context.Entry(existingReport).CurrentValues.SetValues(report);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteReport(int reportId)
        {
            var report = await _context.MedicalReports.FindAsync(reportId);
            if (report != null)
            {
                _context.MedicalReports.Remove(report);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateAnalysisResult(int reportId, string analysisResult)
        {
            var report = await _context.MedicalReports.FindAsync(reportId);
            if (report != null)
            {
                report.AnalysisResult = analysisResult;
                report.AnalyzedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}