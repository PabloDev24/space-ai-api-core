using SmartSpaces.Application.Common.Options;
using SmartSpaces.Application.Features.Cart.Commands.VoiceInteract;
using Xunit;

namespace SmartSpaces.UnitTests;

public class VoiceCartCommandHandlerTests
{
    private static List<CartRouteDefinition> SampleDestinations() =>
    [
        new CartRouteDefinition
        {
            DestinationCode = "EDIFICIO_F",
            DisplayName = "Edificio F",
            Aliases = ["edificio f", "edificio efe"],
            Steps =
            [
                new CartMovementStep { Command = "adelante", DurationSeconds = 5 },
                new CartMovementStep { Command = "izquierda", DurationSeconds = 2 },
                new CartMovementStep { Command = "adelante", DurationSeconds = 5 },
                new CartMovementStep { Command = "stop", DurationSeconds = 0 },
            ],
        },
        new CartRouteDefinition
        {
            DestinationCode = "BIBLIOTECA",
            DisplayName = "Biblioteca",
            Aliases = ["biblioteca"],
            Steps = [new CartMovementStep { Command = "stop", DurationSeconds = 0 }],
        },
    ];

    [Theory]
    [InlineData("guíame al edificio F")]
    [InlineData("llévame al edificio f")]
    [InlineData("cómo llego al edificio efe")]
    [InlineData("EDIFICIO F por favor")]
    public void FindDestination_MatchesAlias_RegardlessOfLeadingPhrase(string transcript)
    {
        var match = VoiceCartCommandHandler.FindDestination(transcript, SampleDestinations());

        Assert.NotNull(match);
        Assert.Equal("EDIFICIO_F", match!.DestinationCode);
    }

    [Fact]
    public void FindDestination_MatchesDifferentDestination_ByItsOwnAlias()
    {
        var match = VoiceCartCommandHandler.FindDestination("llévame a la biblioteca", SampleDestinations());

        Assert.NotNull(match);
        Assert.Equal("BIBLIOTECA", match!.DestinationCode);
    }

    [Theory]
    [InlineData("¿dónde está la cafetería?")]
    [InlineData("")]
    [InlineData("   ")]
    public void FindDestination_ReturnsNull_WhenNoAliasMatches(string transcript)
    {
        var match = VoiceCartCommandHandler.FindDestination(transcript, SampleDestinations());

        Assert.Null(match);
    }
}
