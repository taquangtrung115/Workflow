using Microsoft.EntityFrameworkCore;
using Workflow.Models;

namespace Workflow.Data
{
    /// <summary>
    /// Database context cho Workflow system
    /// </summary>
    public class WorkflowDbContext : DbContext
    {
        public WorkflowDbContext(DbContextOptions<WorkflowDbContext> options)
            : base(options)
        {
        }

        public DbSet<FileType> FileTypes { get; set; } = null!;
        public DbSet<UserFileTypePermission> UserFileTypePermissions { get; set; } = null!;
        public DbSet<Document> Documents { get; set; } = null!;
        public DbSet<WorkflowTemplate> WorkflowTemplates { get; set; } = null!;
        public DbSet<WorkflowLevel> WorkflowLevels { get; set; } = null!;
        public DbSet<WorkflowInstance> WorkflowInstances { get; set; } = null!;
        public DbSet<ApprovalRecord> ApprovalRecords { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // FileType configuration
            modelBuilder.Entity<FileType>(entity =>
            {
                entity.HasIndex(e => e.Mime);
                entity.HasIndex(e => e.Name);
            });

            // UserFileTypePermission configuration
            modelBuilder.Entity<UserFileTypePermission>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.FileTypeId }).IsUnique();
            });

            // Document configuration
            modelBuilder.Entity<Document>(entity =>
            {
                entity.HasIndex(e => e.UploadedBy);
                entity.HasIndex(e => e.UploadedAt);
            });

            // WorkflowTemplate configuration
            modelBuilder.Entity<WorkflowTemplate>(entity =>
            {
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.CreatedBy);
            });

            // WorkflowLevel configuration
            modelBuilder.Entity<WorkflowLevel>(entity =>
            {
                entity.HasIndex(e => new { e.TemplateId, e.Order });
                entity.HasOne(e => e.Template)
                    .WithMany(t => t.Levels)
                    .HasForeignKey(e => e.TemplateId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // WorkflowInstance configuration
            modelBuilder.Entity<WorkflowInstance>(entity =>
            {
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.RequestedBy);
                entity.HasIndex(e => e.RequestedAt);
                entity.HasOne(e => e.Template)
                    .WithMany()
                    .HasForeignKey(e => e.TemplateId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Document)
                    .WithMany()
                    .HasForeignKey(e => e.DocumentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ApprovalRecord configuration
            modelBuilder.Entity<ApprovalRecord>(entity =>
            {
                entity.HasIndex(e => new { e.InstanceId, e.LevelOrder });
                entity.HasIndex(e => e.ApproverUserId);
                entity.HasOne(e => e.Instance)
                    .WithMany(i => i.ApprovalRecords)
                    .HasForeignKey(e => e.InstanceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
