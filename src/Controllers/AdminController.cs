using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Workflow.DTOs;
using Workflow.Services;

namespace Workflow.Controllers
{
    /// <summary>
    /// API cho admin quản lý permissions
    /// </summary>
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public AdminController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        /// <summary>
        /// Grant permission cho user để duyệt file type
        /// POST /api/admin/grant-filetype
        /// Body: { "userId": "guid", "fileTypeId": "guid" }
        /// </summary>
        [HttpPost("grant-filetype")]
        public async Task<ActionResult<UserFileTypePermissionResponse>> GrantFileTypePermission(
            [FromBody] GrantFileTypePermissionRequest request)
        {
            try
            {
                var permission = await _permissionService.GrantPermissionAsync(request);
                return Ok(permission);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Revoke permission từ user
        /// DELETE /api/admin/revoke-filetype?userId={guid}&fileTypeId={guid}
        /// </summary>
        [HttpDelete("revoke-filetype")]
        public async Task<IActionResult> RevokeFileTypePermission([FromQuery] Guid userId, [FromQuery] Guid fileTypeId)
        {
            var result = await _permissionService.RevokePermissionAsync(userId, fileTypeId);
            if (!result)
                return NotFound();

            return NoContent();
        }

        /// <summary>
        /// Lấy tất cả permissions của một user
        /// GET /api/admin/user-permissions/{userId}
        /// </summary>
        [HttpGet("user-permissions/{userId}")]
        public async Task<ActionResult<List<UserFileTypePermissionResponse>>> GetUserPermissions(Guid userId)
        {
            var permissions = await _permissionService.GetUserPermissionsAsync(userId);
            return Ok(permissions);
        }
    }
}
