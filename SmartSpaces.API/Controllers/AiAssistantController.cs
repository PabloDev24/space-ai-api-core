using System.IdentityModel.Tokens.Jwt;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartSpaces.Application.Features.AiAssistant;
using SmartSpaces.Application.Features.AiAssistant.Commands.UploadDocument;
using SmartSpaces.Application.Features.AiAssistant.Queries.GetActivity;
using SmartSpaces.Application.Features.AiAssistant.Queries.GetDocuments;
using SmartSpaces.Application.Features.AiAssistant.Queries.GetMetrics;
using SmartSpaces.Application.Features.AiAssistant.Queries.GetPipeline;

namespace SmartSpaces.API.Controllers;

/// <summary>
/// Administración de la base de conocimiento del asistente IA (panel de Conocimiento).
/// Las preguntas al RAG siguen viviendo en /api/knowledge/ask.
/// </summary>
[ApiController]
[Route("api/ai-assistant")] // El nombre lleva guion: no se puede usar [controller]
[Authorize(Roles = "admin")]
public class AiAssistantController : ControllerBase
{
    private readonly IMediator _mediator;

    public AiAssistantController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("metrics")] // GET: /api/ai-assistant/metrics
    public async Task<IActionResult> GetMetrics()
    {
        try
        {
            var result = await _mediator.Send(new GetAiMetricsQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("documents")] // GET: /api/ai-assistant/documents
    public async Task<IActionResult> GetDocuments()
    {
        try
        {
            var result = await _mediator.Send(new GetAiDocumentsQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("documents")] // POST: /api/ai-assistant/documents (multipart/form-data, campo "file")
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(AiDocumentCatalog.MaxFileSizeBytes)]
    public async Task<IActionResult> UploadDocument(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "No se recibió ningún archivo en el campo 'file'." });
        }

        try
        {
            Guid? uploadedBy = null;
            var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (Guid.TryParse(sub, out var parsedUserId))
            {
                uploadedBy = parsedUserId;
            }

            await using var content = file.OpenReadStream();

            var command = new UploadDocumentCommand(
                file.FileName,
                file.ContentType,
                file.Length,
                content,
                uploadedBy);

            var result = await _mediator.Send(command);
            return StatusCode(201, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("pipeline")] // GET: /api/ai-assistant/pipeline
    public async Task<IActionResult> GetPipeline()
    {
        try
        {
            var result = await _mediator.Send(new GetAiPipelineQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("activity")] // GET: /api/ai-assistant/activity?limit=
    public async Task<IActionResult> GetActivity([FromQuery] int limit = 20)
    {
        try
        {
            var result = await _mediator.Send(new GetAiActivityQuery(limit));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
