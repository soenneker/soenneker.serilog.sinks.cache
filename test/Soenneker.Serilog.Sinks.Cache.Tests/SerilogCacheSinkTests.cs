using Soenneker.Tests.FixturedUnit;
using Xunit;

namespace Soenneker.Serilog.Sinks.Cache.Tests;

[Collection("Collection")]
public sealed class SerilogCacheSinkTests : FixturedUnitTest
{
    public SerilogCacheSinkTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
    }

    [Fact]
    public void Default()
    {

    }
}
