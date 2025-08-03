using DeferredTaskAPI.Entities;
using DeferredTaskAPI.Models;
using DeferredTaskAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace DeferredTaskAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/")]
    public class ScheduledTaskController : Controller
    {
        // TODO: Add authentication?
        private readonly ILogger<ScheduledTaskController> _logger;
        private readonly IScheduledTaskService _scheduledTaskService;

        public ScheduledTaskController(ILogger<ScheduledTaskController> logger,
            IScheduledTaskService scheduledTaskService)
        {
            _logger = logger;
            _scheduledTaskService = scheduledTaskService;
        }

        [HttpGet]
        [Authorize]
        public ActionResult<ScheduledTask> GetScheduledTasks()
        {
            try
            {
                var result = _scheduledTaskService.GetScheduledTasks();
                return ProcessResult(nameof(GetScheduledTask), result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error encountered while executing method {Method}",
                    nameof(GetScheduledTask));
                return Problem("An internal error was encountered while processing this request.");
            }
        }

        [HttpGet("id")]
        [Authorize]
        public ActionResult<ScheduledTask> GetScheduledTask(Guid id)
        {
            try
            {
                var result = _scheduledTaskService.GetScheduledTask(id);
                return ProcessResult(nameof(GetScheduledTask), result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error encountered while executing method {Method} for id {id}",
                    nameof(GetScheduledTask),
                    id);
                return Problem("An internal error was encountered while processing this request.");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ScheduledTask>> CreateScheduledTaskAsync(ScheduledTaskRequest request)
        {
            try
            {
                var result = await _scheduledTaskService.CreateScheduledTaskAsync(request);
                return ProcessResult(nameof(GetScheduledTask), result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error encountered while executing method {Method} for request {Request}",
                    nameof(CreateScheduledTaskAsync),
                    JsonSerializer.Serialize(request));
                return Problem("An internal error was encountered while processing this request.");
            }
        }

        [HttpPut("id")]
        [Authorize]
        public async Task<ActionResult<ScheduledTask>> UpdateScheduledTaskAsync(Guid id,
            ScheduledTaskRequest request)
        {
            try
            {
                var result = await _scheduledTaskService.UpdateScheduledTaskAsync(id, request);
                return ProcessResult(nameof(GetScheduledTask), result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error encountered while executing method {Method} for id {Id} with request {Request}",
                    nameof(UpdateScheduledTaskAsync),
                    id,
                    JsonSerializer.Serialize(request));
                return Problem("An internal error was encountered while processing this request.");
            }
        }

        [HttpDelete("id")]
        [Authorize]
        public async Task<ActionResult<ScheduledTask>> DeleteScheduledTaskAsync(Guid id)
        {
            try
            {
                var result = await _scheduledTaskService.DeleteScheduledTaskAsync(id);
                return ProcessResult(nameof(GetScheduledTask), result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error encountered while executing method {Method} for id {Id}",
                    nameof(DeleteScheduledTaskAsync),
                    id);
                return Problem("An internal error was encountered while processing this request.");
            }
        }

        private ActionResult ProcessResult(string methodName,
            ApiResult result,
            Guid? id = null,
            ScheduledTaskRequest? request = null)
        {
            if (result.IsFailed)
            {
                var response = HandleFailedResult(methodName, result, id, request);
                return response;
            }

            return Ok();
        }
        private ActionResult ProcessResult<T>(string methodName,
            ApiResult<T> result,
            Guid? id = null,
            ScheduledTaskRequest? request = null)
        {
            if (result.IsFailed)
            {
                var response = HandleFailedResult(methodName, result, id, request);
                return response;
            }

            return Ok(result.Value);
        }

        private ObjectResult HandleFailedResult(string methodName,
            ApiResult result,
            Guid? id,
            ScheduledTaskRequest? request)
        {
            _logger.LogWarning(
                "Failed result on {MethodName}.\n\tId: {Id}\n\tRequest: {Request}\n\tResult: {result}",
                methodName,
                id,
                JsonSerializer.Serialize(request),
                JsonSerializer.Serialize(result));
            var response = result.StatusCode switch
            {
                HttpStatusCode.NotFound => NotFound(new { result.Errors }),
                HttpStatusCode.BadRequest => BadRequest(new { result.Errors }),
                _ => Problem("An internal error was encountered while processing this request.")
            };

            return response;
        }
    }
}
