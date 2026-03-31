using Microsoft.EntityFrameworkCore;
using Patient_mgt.Domain;

namespace Patient_mgt.Data
{
    public class PatientContext : DbContext
    {
        public DbSet<Patient> patients { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<EMR> EMRs { get; set; }
        public DbSet<PrescribedMedicine> PrescribedMedicines { get; set; }
        public DbSet<Insurance> Insurances { get; set; }
        public DbSet<MedicalReport> MedicalReports { get; set; }
      
        public PatientContext(DbContextOptions<PatientContext> options) : base(options) 
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Patient>()
                .HasIndex(p => p.Phone)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.EmailId)
                .IsUnique();

            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EMR>()
                .HasOne(e => e.Patient)
                .WithMany()
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EMR>()
                .HasOne(e => e.Doctor)
                .WithMany()
                .HasForeignKey(e => e.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PrescribedMedicine>()
                .HasOne(pm => pm.EMR)
                .WithMany(e => e.PrescribedMedicines)
                .HasForeignKey(pm => pm.EMRId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Insurance>()
                .HasOne(i => i.Patient)
                .WithMany(p => p.Insurances)
                .HasForeignKey(i => i.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MedicalReport>()
                .HasOne(mr => mr.Patient)
                .WithMany(p => p.MedicalReports)
                .HasForeignKey(mr => mr.PatientId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
    
}
