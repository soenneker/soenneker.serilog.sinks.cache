using Soenneker.Tests.HostedUnit;

namespace Soenneker.Serilog.Sinks.Cache.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public sealed class SerilogCacheSinkTests : HostedUnitTest
{
    public SerilogCacheSinkTests(Host host) : base(host)
    {
    }

    [Test]
    public void Default()
    {

    }
}
