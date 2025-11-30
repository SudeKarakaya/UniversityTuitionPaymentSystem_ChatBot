using Microsoft.EntityFrameworkCore;
using UniversityTuitionPaymentSystem.Models;

namespace UniversityTuitionPaymentSystem.Data
{
    public class UniversityDatabase : DbContext
    {
        public UniversityDatabase(DbContextOptions<UniversityDatabase> options)
        : base(options)
        {
        }


        public DbSet<Student> Students => Set<Student>();
        public DbSet<Tuition> Tuitions => Set<Tuition>();
        public DbSet<Payment> Payments => Set<Payment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>()
                .HasIndex(s => s.StudentNo)
                .IsUnique();

            modelBuilder.Entity<Tuition>()
                .HasOne(t => t.Student)
                .WithMany(s => s.Tuitions)
                .HasForeignKey(t => t.StudentId);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Tuition)
                .WithMany(t => t.Payments)
                .HasForeignKey(p => p.TuitionId);
        }
    }
}
