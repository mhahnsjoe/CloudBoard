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
        public DbSet<BoardColumn> BoardColumns => Set<BoardColumn>();
        public DbSet<WorkItemHistory> WorkItemHistories => Set<WorkItemHistory>();
        public DbSet<Team> Teams => Set<Team>();
        public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
        public DbSet<TeamInvitation> TeamInvitations => Set<TeamInvitation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // CRITICAL for Identity tables

            ConfigureTeamRelationships(modelBuilder);
            ConfigureProjectRelationships(modelBuilder);
            ConfigureBoardRelationships(modelBuilder);
            ConfigureBoardColumns(modelBuilder);
            ConfigureWorkItemRelationships(modelBuilder);
            ConfigureWorkItemHierarchy(modelBuilder);
            ConfigureSprintRelationships(modelBuilder);
            ConfigureWorkItemHistoryRelationships(modelBuilder);
        }

        private void ConfigureTeamRelationships(ModelBuilder modelBuilder)
        {
            // Team configuration
            modelBuilder.Entity<Team>(entity =>
            {
                entity.HasKey(t => t.Id);

                entity.Property(t => t.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(t => t.Description)
                    .HasMaxLength(500);

                entity.HasOne(t => t.CreatedBy)
                    .WithMany(u => u.CreatedTeams)
                    .HasForeignKey(t => t.CreatedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // TeamMember configuration (composite key)
            modelBuilder.Entity<TeamMember>(entity =>
            {
                entity.HasKey(tm => new { tm.TeamId, tm.UserId });

                entity.HasOne(tm => tm.Team)
                    .WithMany(t => t.Members)
                    .HasForeignKey(tm => tm.TeamId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(tm => tm.User)
                    .WithMany(u => u.TeamMemberships)
                    .HasForeignKey(tm => tm.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(tm => tm.InvitedBy)
                    .WithMany()
                    .HasForeignKey(tm => tm.InvitedById)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.Property(tm => tm.Role)
                    .HasConversion<string>()
                    .HasMaxLength(20);
            });

            // TeamInvitation configuration
            modelBuilder.Entity<TeamInvitation>(entity =>
            {
                entity.HasKey(ti => ti.Id);

                entity.Property(ti => ti.Email)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(ti => ti.Token)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.HasIndex(ti => ti.Token)
                    .IsUnique();

                entity.HasIndex(ti => new { ti.TeamId, ti.Email });

                entity.Property(ti => ti.Role)
                    .HasConversion<string>()
                    .HasMaxLength(20);

                entity.HasOne(ti => ti.Team)
                    .WithMany(t => t.Invitations)
                    .HasForeignKey(ti => ti.TeamId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ti => ti.InvitedBy)
                    .WithMany(u => u.SentInvitations)
                    .HasForeignKey(ti => ti.InvitedById)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigureProjectRelationships(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>()
                .HasMany(p => p.Boards)
                .WithOne(b => b.Project)
                .HasForeignKey(b => b.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // User-Project relationship (creator/owner for audit)
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Owner)
                .WithMany(u => u.OwnedProjects)
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Team-Project relationship
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Team)
                .WithMany(t => t.Projects)
                .HasForeignKey(p => p.TeamId)
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

        private void ConfigureBoardColumns(ModelBuilder modelBuilder)
        {
            // One-to-many relationship: Board -> BoardColumn
            modelBuilder.Entity<Board>()
                .HasMany(b => b.Columns)
                .WithOne(c => c.Board)
                .HasForeignKey(c => c.BoardId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure column properties
            modelBuilder.Entity<BoardColumn>()
                .Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(50);

            modelBuilder.Entity<BoardColumn>()
                .Property(c => c.Category)
                .IsRequired()
                .HasMaxLength(20);

            // Index on BoardId + Order for efficient ordering queries
            modelBuilder.Entity<BoardColumn>()
                .HasIndex(c => new { c.BoardId, c.Order });

            // Unique constraint on BoardId + Name to prevent duplicate column names within a board
            modelBuilder.Entity<BoardColumn>()
                .HasIndex(c => new { c.BoardId, c.Name })
                .IsUnique();
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
        private void ConfigureWorkItemRelationships(ModelBuilder modelBuilder)
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

        private void ConfigureWorkItemHistoryRelationships(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WorkItemHistory>()
                .HasOne(h => h.WorkItem)
                .WithMany()
                .HasForeignKey(h => h.WorkItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkItemHistory>()
                .HasOne(h => h.ChangedBy)
                .WithMany()
                .HasForeignKey(h => h.ChangedById)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkItemHistory>().HasIndex(h => h.WorkItemId);
            modelBuilder.Entity<WorkItemHistory>().HasIndex(h => h.ChangedAt);
        }
    }
}