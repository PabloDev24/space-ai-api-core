namespace SmartSpaces.Application.Common.Interfaces;

public record SpeechSynthesisResult(byte[]? AudioBytes, string ContentType, bool Success);

// No debe lanzar: si Azure Speech no responde o no está configurado, Success=false
// y el llamador (frontend) cae a la síntesis de voz nativa del navegador.
public interface ISpeechSynthesisService
{
    Task<SpeechSynthesisResult> SynthesizeAsync(string text, CancellationToken cancellationToken);
}
