using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Workflow.DTOs;
using Workflow.Services;

namespace Workflow.Controllers
{
    /// <summary>
    /// API quản lý workflow templates và instances
    /// </summary>
    [ApiController]
    [Route("api/workflow")]
    public class WorkflowController : ControllerBase
    {
        private readonly ITemplateService _templateService;
        private readonly IWorkflowService _workflowService;

        public WorkflowController(ITemplateService templateService, IWorkflowService workflowService)
        {
            _templateService = templateService;
            _workflowService = workflowService;
        }

        #region Templates

        /// <summary>
        /// Lấy tất cả templates đang active
        /// GET /api/workflow/templates
        /// </summary>
        [HttpGet("templates")]
        public async Task<ActionResult<List<TemplateResponse>>> GetTemplates()
        {
            var templates = await _templateService.GetAllActiveAsync();
            return Ok(templates);
        }

        /// <summary>
        /// Lấy template theo ID
        /// GET /api/workflow/templates/{id}
        /// </summary>
        [HttpGet("templates/{id}")]
        public async Task<ActionResult<TemplateResponse>> GetTemplate(Guid id)
        {
            var template = await _templateService.GetByIdAsync(id);
            if (template == null)
                return NotFound();

            return Ok(template);
        }

        /// <summary>
        /// Tạo template mới
        /// POST /api/workflow/templates
        /// Body: { "name": "...", "description": "...", "createdBy": "guid", "levels": [...] }
        /// </summary>
        [HttpPost("templates")]
        public async Task<ActionResult<TemplateResponse>> CreateTemplate([FromBody] CreateTemplateRequest request)
        {
            try
            {
                var template = await _templateService.CreateAsync(request);
                return CreatedAtAction(nameof(GetTemplate), new { id = template.Id }, template);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Deactivate template
        /// DELETE /api/workflow/templates/{id}
        /// </summary>
        [HttpDelete("templates/{id}")]
        public async Task<IActionResult> DeactivateTemplate(Guid id)
        {
            var result = await _templateService.DeactivateAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }

        #endregion

        #region Workflow Instances

        /// <summary>
        /// Start workflow mới
        /// POST /api/workflow/{templateId}/start?documentId={documentId}
        /// </summary>
        [HttpPost("{templateId}/start")]
        public async Task<ActionResult<WorkflowInstanceResponse>> StartWorkflow(
            Guid templateId,
            [FromQuery] Guid documentId,
            [FromQuery] Guid requestedBy)
        {
            try
            {
                var request = new StartWorkflowRequest { DocumentId = documentId };
                var instance = await _workflowService.StartWorkflowAsync(templateId, request, requestedBy);
                return Ok(instance);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy workflow instance theo ID
        /// GET /api/workflow/instances/{instanceId}
        /// </summary>
        [HttpGet("instances/{instanceId}")]
        public async Task<ActionResult<WorkflowInstanceResponse>> GetInstance(Guid instanceId)
        {
            var instance = await _workflowService.GetInstanceAsync(instanceId);
            if (instance == null)
                return NotFound();

            return Ok(instance);
        }

        /// <summary>
        /// Lấy danh sách workflows chờ user approve
        /// GET /api/workflow/pending-approvals?userId={userId}
        /// </summary>
        [HttpGet("pending-approvals")]
        public async Task<ActionResult<List<WorkflowInstanceResponse>>> GetPendingApprovals([FromQuery] Guid userId)
        {
            var instances = await _workflowService.GetUserPendingApprovalsAsync(userId);
            return Ok(instances);
        }

        /// <summary>
        /// Approve workflow
        /// POST /api/workflow/{instanceId}/approve?approverId={approverId}
        /// Body: { "signatureBase64": "...", "comment": "..." }
        /// </summary>
        [HttpPost("{instanceId}/approve")]
        public async Task<ActionResult<WorkflowInstanceResponse>> Approve(
            Guid instanceId,
            [FromQuery] Guid approverId,
            [FromBody] ApproveRequest request)
        {
            try
            {
                var instance = await _workflowService.ApproveAsync(instanceId, request, approverId);
                return Ok(instance);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Reject workflow
        /// POST /api/workflow/{instanceId}/reject?approverId={approverId}
        /// Body: { "comment": "..." }
        /// </summary>
        [HttpPost("{instanceId}/reject")]
        public async Task<ActionResult<WorkflowInstanceResponse>> Reject(
            Guid instanceId,
            [FromQuery] Guid approverId,
            [FromBody] RejectRequest request)
        {
            try
            {
                var instance = await _workflowService.RejectAsync(instanceId, request, approverId);
                return Ok(instance);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion
    }
}
