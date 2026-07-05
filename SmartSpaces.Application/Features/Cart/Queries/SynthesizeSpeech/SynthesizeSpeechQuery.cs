using MediatR;
using SmartSpaces.Application.Common.Interfaces;

namespace SmartSpaces.Application.Features.Cart.Queries.SynthesizeSpeech;

public record SynthesizeSpeechQuery(string Text) : IRequest<SpeechSynthesisResult>;

public class SynthesizeSpeechQueryHandler : IRequestHandler<SynthesizeSpeechQuery, SpeechSynthesisResult>
{
    private readonly ISpeechSynthesisService _speechService;

    public SynthesizeSpeechQueryHandler(ISpeechSynthesisService speechService)
    {
        _speechService = speechService;
    }

    public Task<SpeechSynthesisResult> Handle(SynthesizeSpeechQuery request, CancellationToken cancellationToken) =>
        _speechService.SynthesizeAsync(request.Text, cancellationToken);
}
