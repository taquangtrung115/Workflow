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
    /// Service quản lý FileType
    /// </summary>
    public interface IFileTypeService
    {
        Task<FileTypeResponse> CreateAsync(CreateFileTypeRequest request);
        Task<FileTypeResponse?> GetByIdAsync(Guid id);
        Task<List<FileTypeResponse>> GetAllAsync();
        Task<bool> DeleteAsync(Guid id);
        Task<FileType?> FindByMimeOrExtensionAsync(string mimeType, string extension);
    }

    public class FileTypeService : IFileTypeService
    {
        private readonly WorkflowDbContext _context;

        public FileTypeService(WorkflowDbContext context)
        {
            _context = context;
        }

        public async Task<FileTypeResponse> CreateAsync(CreateFileTypeRequest request)
        {
            var fileType = new FileType
            {
                Name = request.Name,
                Mime = request.Mime,
                ExtensionsJson = JsonSerializer.Serialize(request.Extensions)
            };

            _context.FileTypes.Add(fileType);
            await _context.SaveChangesAsync();

            return MapToResponse(fileType);
        }

        public async Task<FileTypeResponse?> GetByIdAsync(Guid id)
        {
            var fileType = await _context.FileTypes.FindAsync(id);
            return fileType != null ? MapToResponse(fileType) : null;
        }

        public async Task<List<FileTypeResponse>> GetAllAsync()
        {
            var fileTypes = await _context.FileTypes.ToListAsync();
            return fileTypes.Select(MapToResponse).ToList();
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var fileType = await _context.FileTypes.FindAsync(id);
            if (fileType == null)
                return false;

            _context.FileTypes.Remove(fileType);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<FileType?> FindByMimeOrExtensionAsync(string mimeType, string extension)
        {
            var fileTypes = await _context.FileTypes.ToListAsync();

            foreach (var ft in fileTypes)
            {
                // Kiểm tra mime type
                if (!string.IsNullOrEmpty(mimeType) && ft.Mime.Equals(mimeType, StringComparison.OrdinalIgnoreCase))
                    return ft;

                // Kiểm tra extension
                if (!string.IsNullOrEmpty(extension))
                {
                    var extensions = JsonSerializer.Deserialize<List<string>>(ft.ExtensionsJson) ?? new List<string>();
                    if (extensions.Any(ext => ext.Equals(extension, StringComparison.OrdinalIgnoreCase)))
                        return ft;
                }
            }

            return null;
        }

        private static FileTypeResponse MapToResponse(FileType fileType)
        {
            var extensions = JsonSerializer.Deserialize<List<string>>(fileType.ExtensionsJson) ?? new List<string>();

            return new FileTypeResponse
            {
                Id = fileType.Id,
                Name = fileType.Name,
                Mime = fileType.Mime,
                Extensions = extensions,
                CreatedAt = fileType.CreatedAt
            };
        }
    }
}
