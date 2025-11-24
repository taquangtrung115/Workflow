using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Workflow.DTOs;
using Workflow.Services;

namespace Workflow.Controllers
{
    /// <summary>
    /// API quản lý file uploads
    /// </summary>
    [ApiController]
    [Route("api/files")]
    public class FilesController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public FilesController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        /// <summary>
        /// Upload file
        /// POST /api/files/upload?uploadedBy={userId}
        /// Form-data: file (IFormFile)
        /// </summary>
        [HttpPost("upload")]
        public async Task<ActionResult<DocumentResponse>> Upload(
            [FromForm] IFormFile file,
            [FromQuery] Guid uploadedBy)
        {
            try
            {
                var document = await _documentService.UploadAsync(file, uploadedBy);
                return Ok(document);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy document theo ID
        /// GET /api/files/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentResponse>> GetDocument(Guid id)
        {
            var document = await _documentService.GetByIdAsync(id);
            if (document == null)
                return NotFound();

            return Ok(document);
        }
    }
}
