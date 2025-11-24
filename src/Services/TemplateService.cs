using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Workflow.Data;
using Workflow.DTOs;
using Workflow.Models;

namespace Workflow.Services
{
    /// <summary>
    /// Service quản lý workflow templates
    /// </summary>
    public interface ITemplateService
    {
        Task<TemplateResponse> CreateAsync(CreateTemplateRequest request);
        Task<TemplateResponse?> GetByIdAsync(Guid id);
        Task<List<TemplateResponse>> GetAllActiveAsync();
        Task<bool> DeactivateAsync(Guid id);
    }

    public class TemplateService : ITemplateService
    {
        private readonly WorkflowDbContext _context;

        public TemplateService(WorkflowDbContext context)
        {
            _context = context;
        }

        public async Task<TemplateResponse> CreateAsync(CreateTemplateRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var template = new WorkflowTemplate
                {
                    Name = request.Name,
                    Description = request.Description,
                    CreatedBy = request.CreatedBy
                };

                _context.WorkflowTemplates.Add(template);
                await _context.SaveChangesAsync();

                // Add levels
                foreach (var levelRequest in request.Levels)
                {
                    var level = new WorkflowLevel
                    {
                        TemplateId = template.Id,
                        Order = levelRequest.Order,
                        ApproverType = levelRequest.ApproverType,
                        DepartmentId = levelRequest.DepartmentId,
                        UserIdsJson = levelRequest.UserIds != null
                            ? JsonSerializer.Serialize(levelRequest.UserIds)
                            : null,
                        RequiredApprovals = levelRequest.RequiredApprovals,
                        AllowedFileTypesJson = levelRequest.AllowedFileTypes != null
                            ? JsonSerializer.Serialize(levelRequest.AllowedFileTypes)
                            : null
                    };

                    _context.WorkflowLevels.Add(level);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetByIdAsync(template.Id) ?? throw new InvalidOperationException("Failed to retrieve created template");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<TemplateResponse?> GetByIdAsync(Guid id)
        {
            var template = await _context.WorkflowTemplates
                .Include(t => t.Levels)
                .FirstOrDefaultAsync(t => t.Id == id);

            return template != null ? MapToResponse(template) : null;
        }

        public async Task<List<TemplateResponse>> GetAllActiveAsync()
        {
            var templates = await _context.WorkflowTemplates
                .Include(t => t.Levels)
                .Where(t => t.IsActive)
                .ToListAsync();

            return templates.Select(MapToResponse).ToList();
        }

        public async Task<bool> DeactivateAsync(Guid id)
        {
            var template = await _context.WorkflowTemplates.FindAsync(id);
            if (template == null)
                return false;

            template.IsActive = false;
            template.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        private static TemplateResponse MapToResponse(WorkflowTemplate template)
        {
            return new TemplateResponse
            {
                Id = template.Id,
                Name = template.Name,
                Description = template.Description,
                CreatedBy = template.CreatedBy,
                CreatedAt = template.CreatedAt,
                IsActive = template.IsActive,
                Levels = template.Levels.OrderBy(l => l.Order).Select(l => new LevelResponse
                {
                    Id = l.Id,
                    Order = l.Order,
                    ApproverType = l.ApproverType,
                    DepartmentId = l.DepartmentId,
                    UserIds = !string.IsNullOrEmpty(l.UserIdsJson)
                        ? JsonSerializer.Deserialize<List<Guid>>(l.UserIdsJson)
                        : null,
                    RequiredApprovals = l.RequiredApprovals,
                    AllowedFileTypes = !string.IsNullOrEmpty(l.AllowedFileTypesJson)
                        ? JsonSerializer.Deserialize<List<string>>(l.AllowedFileTypesJson)
                        : null
                }).ToList()
            };
        }
    }
}
