using Achai.Api.Common;

namespace Achai.Api.Tests.Unit.Common;

public class CoordinatesTests
{
    [Fact]
    public void DistanceKmTo_BetweenSaoPauloAndRio_IsAbout360Km()
    {
        var se = new Coordinates(-23.5475, -46.63611);
        var copacabana = new Coordinates(-22.90642, -43.18223);

        se.DistanceKmTo(copacabana).ShouldBe(360, tolerance: 5);
    }

    [Fact]
    public void DistanceKmTo_ThePointItself_IsZero()
    {
        var se = new Coordinates(-23.5475, -46.63611);

        se.DistanceKmTo(se).ShouldBe(0, tolerance: 0.0001);
    }
}
