using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Orbitask.Models;

namespace Orbitask.Database
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Workbench> Workbenches { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<Column> Columns { get; set; }
        public DbSet<TaskItem> TaskItems { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TaskTag> TaskTags { get; set; }
        public DbSet<WorkbenchMember> WorkbenchMembers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<WorkbenchMember>()
                .HasKey(wm => new { wm.WorkbenchId, wm.UserId });
            modelBuilder.Entity<WorkbenchMember>()
                .Property(wm => wm.Role)
                .HasConversion<int>();

            modelBuilder.Entity<WorkbenchMember>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(wm => wm.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<WorkbenchMember>()
                .HasOne<Workbench>()
                .WithMany()
                .HasForeignKey(wm => wm.WorkbenchId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<WorkbenchMember>()
                .HasIndex(wm => wm.WorkbenchId)
                .IsUnique()
                .HasFilter("[Role] = 0");  

            // ============================================
            // TASK TAG (TaskItem <-> Tag)
            // ============================================

            modelBuilder.Entity<TaskTag>()
                .HasKey(tt => new { tt.TaskItemId, tt.TagId });

            modelBuilder.Entity<TaskTag>()
                .HasOne<TaskItem>()
                .WithMany()
                .HasForeignKey(tt => tt.TaskItemId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TaskTag>()
                .HasOne<Tag>()
                .WithMany()
                .HasForeignKey(tt => tt.TagId)
                .OnDelete(DeleteBehavior.NoAction);

            // ============================================
            // BOARD -> WORKBENCH
            // ============================================

            modelBuilder.Entity<Board>()
                .HasOne<Workbench>()
                .WithMany()
                .HasForeignKey(b => b.WorkbenchId)
                .OnDelete(DeleteBehavior.NoAction);

            // ============================================
            // COLUMN -> BOARD (Only direct parent)
            // ============================================

            modelBuilder.Entity<Column>()
                .HasOne<Board>()
                .WithMany()
                .HasForeignKey(c => c.BoardId)
                .OnDelete(DeleteBehavior.NoAction);

            // ============================================
            // TAG -> BOARD (Only direct parent)
            // ============================================

            modelBuilder.Entity<Tag>()
                .HasOne<Board>()
                .WithMany()
                .HasForeignKey(t => t.BoardId)
                .OnDelete(DeleteBehavior.NoAction);

            // ============================================
            // TASKITEM -> COLUMN (Only direct parent)
            // ============================================

            modelBuilder.Entity<TaskItem>()
                .HasOne<Column>()
                .WithMany()
                .HasForeignKey(t => t.ColumnId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}