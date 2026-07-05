namespace SmartSpaces.Application.Common.Interfaces;

public record RagUserContext(string UserId, string Name, string Career);
public record RagSource(string Title, int? Page, string ChunkId);
public record RagAskResult(string Answer, double Confidence, IReadOnlyList<RagSource> Sources);

public interface IRagService
{
    Task<RagAskResult> AskAsync(string question, RagUserContext userContext, string source, CancellationToken cancellationToken);
}
