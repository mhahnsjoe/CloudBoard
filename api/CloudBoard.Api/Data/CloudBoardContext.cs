using Microsoft.EntityFrameworkCore;
using CloudBoard.Api.Models;

namespace CloudBoard.Api.Data
{
    public class CloudBoardContext : DbContext
    {
        public CloudBoardContext(DbContextOptions<CloudBoardContext> options)
            : base(options) { }

        public DbSet<Project> Projects => Set<Project>();
        public DbSet<TaskItem> Tasks => Set<TaskItem>();
    }
}
