[![](https://img.shields.io/nuget/v/soenneker.serilog.sinks.cache.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.serilog.sinks.cache/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.serilog.sinks.cache/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.serilog.sinks.cache/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.serilog.sinks.cache.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.serilog.sinks.cache/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.serilog.sinks.cache/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.serilog.sinks.cache/actions/workflows/codeql.yml)

# Soenneker.Serilog.Sinks.Cache

A Serilog sink cache that allows for storing, retrieving, and removing log messages. Queue-backed in-memory log cache for Serilog with optional capacity and byte budget limits.

## Install

```bash
dotnet add package Soenneker.Serilog.Sinks.Cache
```

## Quick start

```csharp
using Soenneker.Serilog.Sinks.Cache.Registrars;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
var result = services.AddSerilogCacheSink();
```

Registers the Serilog cache sink in the dependency injection container. This allows you to inject ISerilogCacheSink to control the sink (enable/disable, get values, etc.).

## What you get

- `ISerilogCacheSink` — A Serilog sink cache that allows for storing, retrieving, and removing log messages. Queue-backed in-memory log cache for Serilog with optional capacity and byte budget limits.
- `SerilogCacheSinkRegistrar` — A Serilog sink cache that allows for storing, retrieving, and removing log messages.

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `ISerilogCacheSink.Capacity` | Gets the optional capacity limit for the cache. Returns null if unbounded. | Gets the optional capacity limit for the cache. Returns null if unbounded. |
| `ISerilogCacheSink.ByteBudget` | Gets the optional byte budget limit for the cache. Returns null if no byte limit. | Gets the optional byte budget limit for the cache. Returns null if no byte limit. |
| `ISerilogCacheSink.IsEnabled` | Gets whether the sink is currently enabled and will accept log events. | Gets whether the sink is currently enabled and will accept log events. |
| `ISerilogCacheSink.Snapshot()` | Gets a snapshot of all cached log entries without removing them from the cache. | A point-in-time collection of cached entries; the cache is left unchanged. |
| `ISerilogCacheSink.Drain()` | Drains all cached log entries, removing them from the cache and returning them. | All removed entries; the cache is empty after the drain completes. |
| `ISerilogCacheSink.Clear()` | Clears all cached log entries from the cache. | A task that completes when the Serilog Cache Sink has been cleared. |
| `ISerilogCacheSink.Enable()` | Enables the sink to accept log events. | A task that completes when the enable operation is complete. |
| `ISerilogCacheSink.Disable()` | Disables the sink from accepting log events. | A task that completes when the disable operation is complete. |
| `SerilogCacheSinkRegistrar.LogCache(writeTo, serviceProvider, restrictedToMinimumLevel)` | Adds a queue-backed in-memory log cache using an existing instance from DI. Use this when you've already registered the sink with AddSerilogCacheSink(). | The logger configuration. |
| `SerilogCacheSinkRegistrar.AddSerilogCacheSink(services, capacity, byteBudget, outputTemplate, formatProvider, serviceLifetime)` | Registers the Serilog cache sink in the dependency injection container. This allows you to inject ISerilogCacheSink to control the sink (enable/disable, get values, etc.). | The service collection for chaining. |

## Practical notes

- Calls that return a cached or singleton value reuse the same instance until the owning service is disposed.
- Dispose instances you own when their scope ends so held resources can be released.
