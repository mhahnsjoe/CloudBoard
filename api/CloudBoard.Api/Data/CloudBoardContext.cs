using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using CloudBoard.Api.Models;

namespace CloudBoard.Api.Data
{
    public class CloudBoardContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public CloudBoardContext(DbContextOptions<CloudBoardContext> options)
            : base(options) { }

        public DbSet<Project> Projects => Set<Project>();
        public DbSet<Board> Boards => Set<Board>();
        public DbSet<WorkItem> WorkItems => Set<WorkItem>();
        public DbSet<Sprint> Sprints => Set<Sprint>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // CRITICAL for Identity tables

            ConfigureProjectRelationships(modelBuilder);
            ConfigureBoardRelationships(modelBuilder);
            ConfigureWorkItemelationships(modelBuilder);
            ConfigureWorkItemHierarchy(modelBuilder);
            ConfigureSprintRelationships(modelBuilder);
        }

        private void ConfigureProjectRelationships(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>()
                .HasMany(p => p.Boards)
                .WithOne(b => b.Project)
                .HasForeignKey(b => b.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            //User-Project relationship
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Owner)
                .WithMany(u => u.OwnedProjects)
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
            
        }
        private void ConfigureBoardRelationships(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Board>()
                .HasMany(b => b.WorkItems)
                .WithOne(t => t.Board)
                .HasForeignKey(t => t.BoardId)
                .OnDelete(DeleteBehavior.SetNull);
        }
        private void ConfigureSprintRelationships(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Sprint>()
                .HasOne(s => s.Board)
                .WithMany(b => b.Sprints)
                .HasForeignKey(s => s.BoardId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Sprint>()
                .HasMany(s => s.WorkItems)
                .WithOne(w => w.Sprint)
                .HasForeignKey(w => w.SprintId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            modelBuilder.Entity<Sprint>().HasIndex(s => s.BoardId);
            modelBuilder.Entity<Sprint>().HasIndex(s => s.Status);
        }
        private void ConfigureWorkItemelationships(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WorkItem>()
                .HasOne(w => w.Project)
                .WithMany()
                .HasForeignKey(w => w.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private void ConfigureWorkItemHierarchy(ModelBuilder modelBuilder)
        {
            // Self-referencing hierarchy
            modelBuilder.Entity<WorkItem>()
                .HasOne(t => t.Parent)
                .WithMany(t => t.Children)
                .HasForeignKey(t => t.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // User-WorkItem relationships
            modelBuilder.Entity<WorkItem>()
                .HasOne(w => w.AssignedTo)
                .WithMany(u => u.AssignedWorkItems)
                .HasForeignKey(w => w.AssignedToId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<WorkItem>()
                .HasOne(w => w.CreatedBy)
                .WithMany(u => u.CreatedWorkItems)
                .HasForeignKey(w => w.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            modelBuilder.Entity<WorkItem>().HasIndex(t => t.ParentId);
            modelBuilder.Entity<WorkItem>().HasIndex(t => new { t.BoardId, t.Type });
            modelBuilder.Entity<WorkItem>().HasIndex(t => t.Status);
            modelBuilder.Entity<WorkItem>().HasIndex(t => t.AssignedToId);
        }
    }
}