using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace triviaApp.Models
{
	public class AppDbContext: IdentityDbContext<User>
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Competition> Competitions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<Score> Scores { get; set; }


        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
		}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Participant>()
               .HasOne(p => p.Competition)
               .WithMany(c => c.Participants)
               .HasForeignKey(p => p.CompetitionId)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Score>()
                .HasOne(s => s.Competition)
                .WithMany(c => c.Scores)
                .HasForeignKey(s => s.CompetitionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Score>()
                .HasOne(s => s.Participant)
                .WithMany(p => p.Scores)
                .HasForeignKey(s => s.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Participant>()
               .HasIndex(p => new { p.Username, p.CompetitionId })
               .IsUnique();

            modelBuilder.Entity<CompetitionCategory>()
                .HasKey(cc => new { cc.CompetitionId, cc.CategoryId });

            modelBuilder.Entity<CompetitionCategory>()
                .HasOne(cc => cc.Competition)
                .WithMany(c => c.CompetitionCategories)
                .HasForeignKey(cc => cc.CompetitionId);

            modelBuilder.Entity<CompetitionCategory>()
                .HasOne(cc => cc.Category)
                .WithMany(c => c.CompetitionCategories)
                .HasForeignKey(cc => cc.CategoryId);

            modelBuilder.Entity<CompetitionQuestion>()
                .HasKey(cq => new { cq.CompetitionId, cq.QuestionId });

            modelBuilder.Entity<CompetitionQuestion>()
                .HasOne(cq => cq.Competition)
                .WithMany(c => c.CompetitionQuestions)
                .HasForeignKey(cq => cq.CompetitionId);

            modelBuilder.Entity<CompetitionQuestion>()
                .HasOne(cq => cq.Question)
                .WithMany(q => q.CompetitionQuestions)
                .HasForeignKey(cq => cq.QuestionId);
        }
    }
}

