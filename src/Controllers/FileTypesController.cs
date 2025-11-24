using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Workflow.DTOs;
using Workflow.Services;

namespace Workflow.Controllers
{
    /// <summary>
    /// API quản lý FileTypes
    /// </summary>
    [ApiController]
    [Route("api/filetypes")]
    public class FileTypesController : ControllerBase
    {
        private readonly IFileTypeService _fileTypeService;

        public FileTypesController(IFileTypeService fileTypeService)
        {
            _fileTypeService = fileTypeService;
        }

        /// <summary>
        /// Lấy tất cả file types
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<FileTypeResponse>>> GetAll()
        {
            var fileTypes = await _fileTypeService.GetAllAsync();
            return Ok(fileTypes);
        }

        /// <summary>
        /// Lấy file type theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<FileTypeResponse>> GetById(Guid id)
        {
            var fileType = await _fileTypeService.GetByIdAsync(id);
            if (fileType == null)
                return NotFound();

            return Ok(fileType);
        }

        /// <summary>
        /// Tạo file type mới
        /// POST /api/filetypes
        /// Body: { "name": "PDF", "mime": "application/pdf", "extensions": [".pdf"] }
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<FileTypeResponse>> Create([FromBody] CreateFileTypeRequest request)
        {
            try
            {
                var fileType = await _fileTypeService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = fileType.Id }, fileType);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Xóa file type
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _fileTypeService.DeleteAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
