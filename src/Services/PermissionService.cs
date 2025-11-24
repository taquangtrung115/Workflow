using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Workflow.Data;
using Workflow.DTOs;
using Workflow.Models;

namespace Workflow.Services
{
    /// <summary>
    /// Service quản lý user permissions cho file types
    /// </summary>
    public interface IPermissionService
    {
        Task<UserFileTypePermissionResponse> GrantPermissionAsync(GrantFileTypePermissionRequest request);
        Task<bool> RevokePermissionAsync(Guid userId, Guid fileTypeId);
        Task<bool> HasPermissionAsync(Guid userId, Guid fileTypeId);
        Task<List<UserFileTypePermissionResponse>> GetUserPermissionsAsync(Guid userId);
    }

    public class PermissionService : IPermissionService
    {
        private readonly WorkflowDbContext _context;

        public PermissionService(WorkflowDbContext context)
        {
            _context = context;
        }

        public async Task<UserFileTypePermissionResponse> GrantPermissionAsync(GrantFileTypePermissionRequest request)
        {
            // Kiểm tra xem permission đã tồn tại chưa
            var existing = await _context.UserFileTypePermissions
                .FirstOrDefaultAsync(p => p.UserId == request.UserId && p.FileTypeId == request.FileTypeId);

            if (existing != null)
            {
                // Đã tồn tại, trả về
                return await MapToResponseAsync(existing);
            }

            var permission = new UserFileTypePermission
            {
                UserId = request.UserId,
                FileTypeId = request.FileTypeId
            };

            _context.UserFileTypePermissions.Add(permission);
            await _context.SaveChangesAsync();

            return await MapToResponseAsync(permission);
        }

        public async Task<bool> RevokePermissionAsync(Guid userId, Guid fileTypeId)
        {
            var permission = await _context.UserFileTypePermissions
                .FirstOrDefaultAsync(p => p.UserId == userId && p.FileTypeId == fileTypeId);

            if (permission == null)
                return false;

            _context.UserFileTypePermissions.Remove(permission);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasPermissionAsync(Guid userId, Guid fileTypeId)
        {
            return await _context.UserFileTypePermissions
                .AnyAsync(p => p.UserId == userId && p.FileTypeId == fileTypeId);
        }

        public async Task<List<UserFileTypePermissionResponse>> GetUserPermissionsAsync(Guid userId)
        {
            var permissions = await _context.UserFileTypePermissions
                .Include(p => p.FileType)
                .Where(p => p.UserId == userId)
                .ToListAsync();

            var responses = new List<UserFileTypePermissionResponse>();
            foreach (var permission in permissions)
            {
                responses.Add(await MapToResponseAsync(permission));
            }

            return responses;
        }

        private async Task<UserFileTypePermissionResponse> MapToResponseAsync(UserFileTypePermission permission)
        {
            var fileType = permission.FileType ?? await _context.FileTypes.FindAsync(permission.FileTypeId);

            return new UserFileTypePermissionResponse
            {
                Id = permission.Id,
                UserId = permission.UserId,
                FileTypeId = permission.FileTypeId,
                FileTypeName = fileType?.Name ?? "Unknown",
                GrantedAt = permission.GrantedAt
            };
        }
    }
}
