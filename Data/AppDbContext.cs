using Microsoft.EntityFrameworkCore;
using Minerva.Models;

namespace Minerva.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Existing table name mappings
            modelBuilder.Entity<University>().ToTable("UniversityTb");
            modelBuilder.Entity<Admin>().ToTable("AdminTb");
            modelBuilder.Entity<Doctor>().ToTable("DoctorTb");
            modelBuilder.Entity<Student>().ToTable("StudentTb");
            modelBuilder.Entity<Subject>().ToTable("SubjectTb");
            modelBuilder.Entity<StudentSubject>().ToTable("StudentSubject");
            modelBuilder.Entity<Assignment>().ToTable("AssignmentTb");
            modelBuilder.Entity<Attemp>().ToTable("AttempTb");

            // New table name mappings
            modelBuilder.Entity<Quiz>().ToTable("QuizTb");
            modelBuilder.Entity<QQuestionTb>().ToTable("QQuestionTb");
            modelBuilder.Entity<MCQOptionTb>().ToTable("MCQOptionTb");
            modelBuilder.Entity<QAttempTb>().ToTable("Q_AttempTb");

            // Define composite primary key
            modelBuilder.Entity<StudentSubject>()
                .HasKey(ss => new { ss.Student_id, ss.Subject_id });

            // Relationships using Fluent API (for new entities)
            modelBuilder.Entity<Quiz>()
                .HasMany(q => q.Questions)
                .WithOne(qq => qq.Quiz)
                .HasForeignKey(qq => qq.Quiz_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QQuestionTb>()
                .HasMany(qq => qq.Options)
                .WithOne(o => o.Question)
                .HasForeignKey(o => o.Question_id)
                .OnDelete(DeleteBehavior.Cascade);

            // Relationship for QAttempTb
            modelBuilder.Entity<QAttempTb>()
                .HasOne(a => a.Quiz)
                .WithMany()
                .HasForeignKey(a => a.Quiz_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QAttempTb>()
                .HasOne(a => a.Student)
                .WithMany()
                .HasForeignKey(a => a.Student_id)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }


        // Existing DbSets
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Lecture> Lectures { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<University> Universities { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<StudentSubject> StudentSubjects { get; set; }
        public DbSet<Attemp> Attemps { get; set; }

        // New DbSets for Quiz entities
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<QQuestionTb> Questions { get; set; }
        public DbSet<MCQOptionTb> MCQOptions { get; set; }
        public DbSet<QAttempTb> QAttempts { get; set; }

        // New DbSet for QAttemptAnswerTb
        public DbSet<QAttemptAnswerTb> QAttemptAnswers { get; set; }
    }
}
