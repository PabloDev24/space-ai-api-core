using SmartSpaces.Application.Common.Cart;
using SmartSpaces.Application.Common.Options;
using Xunit;

namespace SmartSpaces.UnitTests;

public class CartMovementCalculatorTests
{
    private static readonly CartCalibration Calibration = new()
    {
        InchesPerSecond = 16.9,
        TurnDegreesPerSecond = 160,
        TurnOffsetDegrees = -38,
    };

    [Fact]
    public void ResolveDuration_UsesDistance_ForForwardStep()
    {
        var step = new CartMovementStep { Command = "adelante", DistanceInches = 33.8 };

        var seconds = CartMovementCalculator.ResolveDuration(step, Calibration);

        Assert.Equal(2.0, seconds, precision: 3);
    }

    [Fact]
    public void ResolveDuration_UsesDegrees_ForTurnStep()
    {
        var step = new CartMovementStep { Command = "izquierda", Degrees = 90 };

        var seconds = CartMovementCalculator.ResolveDuration(step, Calibration);

        Assert.Equal(0.8, seconds, precision: 3);
    }

    [Fact]
    public void ResolveDuration_PrefersExplicitDuration_OverDistanceOrDegrees()
    {
        var step = new CartMovementStep { Command = "adelante", DistanceInches = 100, DurationSeconds = 1.5 };

        var seconds = CartMovementCalculator.ResolveDuration(step, Calibration);

        Assert.Equal(1.5, seconds);
    }

    [Fact]
    public void ResolveDuration_ReturnsZero_ForStopWithNoParameters()
    {
        var step = new CartMovementStep { Command = "stop" };

        var seconds = CartMovementCalculator.ResolveDuration(step, Calibration);

        Assert.Equal(0, seconds);
    }

    [Fact]
    public void ComputeReverse_InvertsOrderAndDirection_AndAppendsStop()
    {
        var forward = new List<ResolvedMovementStep>
        {
            new("adelante", 2),
            new("izquierda", 0.8),
            new("adelante", 2),
            new("stop", 0),
        };

        var reverse = CartMovementCalculator.ComputeReverse(forward);

        Assert.Equal(
        [
            new ResolvedMovementStep("atras", 2),
            new ResolvedMovementStep("derecha", 0.8),
            new ResolvedMovementStep("atras", 2),
            new ResolvedMovementStep("stop", 0),
        ], reverse);
    }

    [Fact]
    public void ComputeReverse_OfReverse_RestoresOriginalSequence()
    {
        var forward = new List<ResolvedMovementStep>
        {
            new("adelante", 2),
            new("izquierda", 0.8),
            new("adelante", 2),
            new("stop", 0),
        };

        var roundTrip = CartMovementCalculator.ComputeReverse(CartMovementCalculator.ComputeReverse(forward));

        Assert.Equal(forward, roundTrip);
    }
}
