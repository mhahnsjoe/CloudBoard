using Microsoft.EntityFrameworkCore;
using CloudBoard.Api.Models;

namespace CloudBoard.Api.Data
{
    public class CloudBoardContext : DbContext
    {
        public CloudBoardContext(DbContextOptions<CloudBoardContext> options)
            : base(options) { }

        public DbSet<Project> Projects => Set<Project>();
        public DbSet<Board> Boards => Set<Board>();
        public DbSet<TaskItem> Tasks => Set<TaskItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Project -> Boards relationship
            modelBuilder.Entity<Project>()
                .HasMany(p => p.Boards)
                .WithOne(b => b.Project)
                .HasForeignKey(b => b.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Board -> Tasks relationship
            modelBuilder.Entity<Board>()
                .HasMany(b => b.Tasks)
                .WithOne(t => t.Board)
                .HasForeignKey(t => t.BoardId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}