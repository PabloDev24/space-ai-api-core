using MediatR;

namespace SmartSpaces.Application.Features.Knowledge.Commands.Ask;

public record AskKnowledgeCommand(Guid UserId, string Question, string Source) : IRequest<AskKnowledgeResult>;

public record KnowledgeSourceDto(string Title, int? Page);

public record AskKnowledgeResult(string Answer, double Confidence, IReadOnlyList<KnowledgeSourceDto> Sources, bool IsMock);
