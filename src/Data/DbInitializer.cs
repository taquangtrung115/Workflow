using System;
using System.Linq;
using System.Text.Json;
using Workflow.Models;

namespace Workflow.Data
{
    /// <summary>
    /// Seed dữ liệu ban đầu cho database
    /// </summary>
    public static class DbInitializer
    {
        public static void Initialize(WorkflowDbContext context)
        {
            context.Database.EnsureCreated();

            // Kiểm tra nếu đã có dữ liệu
            if (context.FileTypes.Any())
            {
                return; // DB đã được seed
            }

            // Seed FileTypes
            var fileTypes = new[]
            {
                new FileType
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-0000000000f1"),
                    Name = "PDF",
                    Mime = "application/pdf",
                    ExtensionsJson = JsonSerializer.Serialize(new[] { ".pdf" })
                },
                new FileType
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-0000000000f2"),
                    Name = "Word Document",
                    Mime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    ExtensionsJson = JsonSerializer.Serialize(new[] { ".docx" })
                },
                new FileType
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-0000000000f3"),
                    Name = "Excel Spreadsheet",
                    Mime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    ExtensionsJson = JsonSerializer.Serialize(new[] { ".xlsx" })
                },
                new FileType
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-0000000000f4"),
                    Name = "Image JPEG",
                    Mime = "image/jpeg",
                    ExtensionsJson = JsonSerializer.Serialize(new[] { ".jpg", ".jpeg" })
                },
                new FileType
                {
                    Id = Guid.Parse("00000000-0000-0000-0000-0000000000f5"),
                    Name = "Image PNG",
                    Mime = "image/png",
                    ExtensionsJson = JsonSerializer.Serialize(new[] { ".png" })
                }
            };

            context.FileTypes.AddRange(fileTypes);
            context.SaveChanges();

            // Seed một template mẫu (giả sử có users và departments đã tạo trước)
            var sampleTemplate = new WorkflowTemplate
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Purchase Approval Workflow",
                Description = "Template cho quy trình duyệt hóa đơn mua hàng",
                CreatedBy = Guid.Parse("00000000-0000-0000-0000-000000000001")
            };

            context.WorkflowTemplates.Add(sampleTemplate);

            var level1 = new WorkflowLevel
            {
                TemplateId = sampleTemplate.Id,
                Order = 1,
                ApproverType = "Department",
                DepartmentId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                RequiredApprovals = 1,
                AllowedFileTypesJson = JsonSerializer.Serialize(new[] { "application/pdf", ".pdf" })
            };

            var level2 = new WorkflowLevel
            {
                TemplateId = sampleTemplate.Id,
                Order = 2,
                ApproverType = "Users",
                UserIdsJson = JsonSerializer.Serialize(new[]
                {
                    "33333333-3333-3333-3333-333333333333",
                    "44444444-4444-4444-4444-444444444444"
                }),
                RequiredApprovals = 2,
                AllowedFileTypesJson = JsonSerializer.Serialize(new[] { "application/pdf", ".pdf", ".docx" })
            };

            context.WorkflowLevels.AddRange(level1, level2);
            context.SaveChanges();
        }
    }
}
