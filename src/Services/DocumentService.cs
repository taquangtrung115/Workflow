using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Workflow.Data;
using Workflow.DTOs;
using Workflow.Models;

namespace Workflow.Services
{
    /// <summary>
    /// Service quản lý documents/files
    /// </summary>
    public interface IDocumentService
    {
        Task<DocumentResponse> UploadAsync(IFormFile file, Guid uploadedBy);
        Task<DocumentResponse?> GetByIdAsync(Guid id);
    }

    public class DocumentService : IDocumentService
    {
        private readonly WorkflowDbContext _context;
        private readonly string _uploadPath;

        public DocumentService(WorkflowDbContext context)
        {
            _context = context;
            // Trong production, nên dùng Azure Blob Storage hoặc AWS S3
            _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            Directory.CreateDirectory(_uploadPath);
        }

        public async Task<DocumentResponse> UploadAsync(IFormFile file, Guid uploadedBy)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ");

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(_uploadPath, fileName);

            // Save file to disk (trong production nên upload lên cloud storage)
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Create document record
            var document = new Document
            {
                Filename = file.FileName,
                MimeType = file.ContentType,
                BlobUrl = $"/uploads/{fileName}", // Trong production sẽ là URL của cloud storage
                FileSize = file.Length,
                UploadedBy = uploadedBy
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            return MapToResponse(document);
        }

        public async Task<DocumentResponse?> GetByIdAsync(Guid id)
        {
            var document = await _context.Documents.FindAsync(id);
            return document != null ? MapToResponse(document) : null;
        }

        private static DocumentResponse MapToResponse(Document document)
        {
            return new DocumentResponse
            {
                Id = document.Id,
                Filename = document.Filename,
                MimeType = document.MimeType,
                BlobUrl = document.BlobUrl,
                FileSize = document.FileSize,
                UploadedBy = document.UploadedBy,
                UploadedAt = document.UploadedAt
            };
        }
    }
}
