using System.IdentityModel.Tokens.Jwt;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SmartSpaces.Application.Features.Cart.Commands.Navigate;
using SmartSpaces.Application.Features.Cart.Commands.Query;
using SmartSpaces.Application.Features.Cart.Commands.VoiceInteract;
using SmartSpaces.Application.Features.Cart.Queries.GetStatus;
using SmartSpaces.Application.Features.Cart.Queries.SynthesizeSpeech;

namespace SmartSpaces.API.Controllers;

[ApiController]
[Route("api/[controller]")] // Esto mapea a: /api/cart
[Authorize]
[EnableRateLimiting("cart")] // 10 req/min por IP — voice/speech son AllowAnonymous, sin esto no hay freno de abuso/costo
public class CartController : ControllerBase
{
    private readonly IMediator _mediator;

    public CartController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("status")] // GET: /api/cart/status
    public async Task<IActionResult> GetStatus()
    {
        try
        {
            var result = await _mediator.Send(new GetCartStatusQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("navigate")] // POST: /api/cart/navigate
    public async Task<IActionResult> Navigate([FromBody] NavigateCartCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("query")] // POST: /api/cart/query
    public async Task<IActionResult> Query([FromBody] CartQueryCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Navegación guiada por voz o pregunta libre (RAG). Anónimo: sesión ligera sin login
    // obligatorio (docs/00 §3.4); si viene un JWT válido se personaliza la respuesta.
    [HttpPost("voice")] // POST: /api/cart/voice
    [AllowAnonymous]
    public async Task<IActionResult> Voice([FromBody] VoiceCartRequest request)
    {
        try
        {
            Guid? userId = null;
            var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (Guid.TryParse(sub, out var parsedUserId))
            {
                userId = parsedUserId;
            }

            var command = new VoiceCartCommand(userId, request.Transcript);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // Voz IA (Azure Speech). El front solo pega aquí cuando el usuario elige manualmente
    // el modo "Azure" en la tablet — por defecto usa la síntesis nativa del navegador,
    // que no pasa por el backend ni consume la cuota de Azure.
    [HttpPost("speech")] // POST: /api/cart/speech
    [AllowAnonymous]
    public async Task<IActionResult> Speech([FromBody] SpeechRequest request)
    {
        var result = await _mediator.Send(new SynthesizeSpeechQuery(request.Text));
        if (!result.Success || result.AudioBytes == null)
        {
            return StatusCode(503, new { error = "No se pudo generar audio en este momento." });
        }

        return File(result.AudioBytes, result.ContentType);
    }
}

public record VoiceCartRequest(string Transcript);
public record SpeechRequest(string Text);
