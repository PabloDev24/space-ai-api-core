using System.Net.Http.Json;
using System.Text.Json;
using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Infrastructure.Services;

public class RagHttpService : IRagService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _httpClient;

    public RagHttpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private record AskRequestBody(string Question, RagUserContext UserContext, string Source);
    private record AskResponseSource(string Title, int? Page, string ChunkId);
    private record AskResponseBody(string Answer, double Confidence, List<AskResponseSource> Sources);

    public async Task<RagAskResult> AskAsync(string question, RagUserContext userContext, string source, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "/ask",
            new AskRequestBody(question, userContext, source),
            JsonOptions,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<AskResponseBody>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Respuesta vacía del servicio RAG.");

        var sources = body.Sources
            .Select(s => new RagSource(s.Title, s.Page, s.ChunkId))
            .ToList();

        return new RagAskResult(body.Answer, body.Confidence, sources);
    }
}
