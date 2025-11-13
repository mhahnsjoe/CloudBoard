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
        public DbSet<WorkItem> WorkItems => Set<WorkItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureProjectRelationships(modelBuilder);
            ConfigureBoardRelationships(modelBuilder);
            ConfigureWorkItemHierarchy(modelBuilder);
        }

        private void ConfigureProjectRelationships(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>()
                .HasMany(p => p.Boards)
                .WithOne(b => b.Project)
                .HasForeignKey(b => b.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private void ConfigureBoardRelationships(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Board>()
                .HasMany(b => b.WorkItems)
                .WithOne(t => t.Board)
                .HasForeignKey(t => t.BoardId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private void ConfigureWorkItemHierarchy(ModelBuilder modelBuilder)
        {
            // Self-referencing relationship for hierarchy
            modelBuilder.Entity<WorkItem>()
                .HasOne(t => t.Parent)
                .WithMany(t => t.Children)
                .HasForeignKey(t => t.ParentId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete cycles

            // Indexes for performance
            modelBuilder.Entity<WorkItem>()
                .HasIndex(t => t.ParentId);

            modelBuilder.Entity<WorkItem>()
                .HasIndex(t => new { t.BoardId, t.Type });

            modelBuilder.Entity<WorkItem>()
                .HasIndex(t => t.Status);
        }
    }
}