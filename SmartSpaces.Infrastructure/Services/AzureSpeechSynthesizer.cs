using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Infrastructure.Services;

// Llama al REST API de Azure Speech (texto-a-voz neural) directo por HttpClient,
// sin el SDK de Cognitive Services (evita una dependencia pesada para un solo endpoint).
// El front solo usa esto cuando el usuario elige manualmente el modo "Azure" — el modo
// "nativo" (por defecto) nunca llega aquí, así se evita gastar la cuota gratis sin querer.
public class AzureSpeechSynthesizer : ISpeechSynthesisService
{
    private const string ContentType = "audio/mpeg";

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AzureSpeechSynthesizer> _logger;

    public AzureSpeechSynthesizer(HttpClient httpClient, IConfiguration configuration, ILogger<AzureSpeechSynthesizer> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<SpeechSynthesisResult> SynthesizeAsync(string text, CancellationToken cancellationToken)
    {
        var key = _configuration["AZURE_SPEECH_KEY"];
        if (string.IsNullOrWhiteSpace(key) || _httpClient.BaseAddress == null)
        {
            _logger.LogWarning("Azure Speech no configurado (falta AZURE_SPEECH_KEY o AZURE_SPEECH_REGION).");
            return new SpeechSynthesisResult(null, ContentType, false);
        }

        var voice = _configuration["AZURE_SPEECH_VOICE"] ?? "es-MX-DaliaNeural";
        var ssml = BuildSsml(text, voice);

        using var request = new HttpRequestMessage(HttpMethod.Post, "cognitiveservices/v1")
        {
            Content = new StringContent(ssml, Encoding.UTF8, "application/ssml+xml"),
        };
        request.Headers.Add("Ocp-Apim-Subscription-Key", key);
        request.Headers.Add("X-Microsoft-OutputFormat", "audio-24khz-48kbitrate-mono-mp3");
        request.Headers.UserAgent.ParseAdd("SpaceIA-CartTablet");

        try
        {
            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Azure Speech respondió {Status}.", response.StatusCode);
                return new SpeechSynthesisResult(null, ContentType, false);
            }

            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            return new SpeechSynthesisResult(bytes, ContentType, true);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(ex, "No se pudo sintetizar voz con Azure Speech.");
            return new SpeechSynthesisResult(null, ContentType, false);
        }
    }

    private static string BuildSsml(string text, string voice)
    {
        var escaped = text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");

        return $"<speak version='1.0' xml:lang='es-MX'>" +
               $"<voice xml:lang='es-MX' name='{voice}'>{escaped}</voice></speak>";
    }
}
