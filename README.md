[![](https://img.shields.io/nuget/v/soenneker.serilog.sinks.cache.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.serilog.sinks.cache/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.serilog.sinks.cache/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.serilog.sinks.cache/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.serilog.sinks.cache.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.serilog.sinks.cache/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.serilog.sinks.cache/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.serilog.sinks.cache/actions/workflows/codeql.yml)

# Soenneker.Serilog.Sinks.Cache

An in-memory Serilog sink for inspecting, draining, or clearing a bounded queue of formatted log messages.

## Installation

```bash
dotnet add package Soenneker.Serilog.Sinks.Cache
```

## Register and configure

Register the sink, then use the same DI-owned instance in the Serilog configuration:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Soenneker.Serilog.Sinks.Cache.Abstract;
using Soenneker.Serilog.Sinks.Cache.Registrars;

var services = new ServiceCollection();

services.AddSerilogCacheSink(
    capacity: 500,
    byteBudget: 1_000_000,
    outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}");

await using ServiceProvider provider = services.BuildServiceProvider();

Log.Logger = new LoggerConfiguration()
    .WriteTo.LogCache(provider)
    .CreateLogger();

ISerilogCacheSink cache = provider.GetRequiredService<ISerilogCacheSink>();
```

The default DI lifetime is singleton. If `serviceLifetime` is changed, the service provider passed to `LogCache` must resolve the instance whose lifetime is at least as long as the Serilog logger.

## Read and manage messages

```csharp
List<string> current = await cache.Snapshot(); // Leaves entries in the cache.
List<string> removed = await cache.Drain();    // Returns and removes every entry.

await cache.Clear();
await cache.Disable();
await cache.Enable();
```

Commands and log events share one channel and are processed by one reader. Awaiting a command means all channel items ordered before that command have been processed. Calls made concurrently by different producers may naturally interleave.

`Disable` prevents later `Emit` calls from entering the channel; it does not clear entries already cached. `Enable` starts accepting events again.

## Capacity behavior

The cache retains entries in FIFO order. When `capacity` is exceeded, the oldest entries are evicted until the count fits. When `byteBudget` is exceeded, the oldest entries are evicted until the estimated size fits. If one entry alone exceeds the byte budget, it is immediately evicted.

The byte estimate is `message.Length * 2`, representing the formatted string's UTF-16 character storage. It does not include object, queue, or runtime overhead and is not the UTF-8 payload size.

`null` or `0` disables the corresponding limit. Without either limit, the cache is unbounded and can consume memory for the lifetime of the sink.

The sink owns a background reader. Dispose the DI service provider and close the Serilog logger during application or test teardown. After disposal, read operations return an empty list and control operations complete without changing state.

Cached messages can contain credentials or personal data. Apply suitable log filtering and do not expose `Snapshot` or `Drain` results directly to untrusted callers.
