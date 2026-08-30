using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog.Core;

namespace Soenneker.Serilog.Sinks.Cache.Abstract;

/// <summary>
/// A queue-backed in-memory Serilog sink with snapshot, drain, clear, and enable controls plus optional count and approximate byte limits.
/// </summary>
public interface ISerilogCacheSink : ILogEventSink, IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Gets the optional capacity limit for the cache. Returns null if unbounded.
    /// </summary>
    int? Capacity { get; }

    /// <summary>
    /// Gets the optional approximate UTF-16 message byte budget. Returns null if no byte limit was configured.
    /// </summary>
    long? ByteBudget { get; }

    /// <summary>
    /// Gets whether the sink is currently enabled and will accept log events.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Gets a FIFO snapshot of all formatted log entries without removing them from the cache.
    /// </summary>
    /// <returns>A task whose result is the collection returned by snapshot.</returns>
    Task<List<string>> Snapshot();

    /// <summary>
    /// Removes and returns all formatted log entries in FIFO order.
    /// </summary>
    /// <returns>A task whose result is the collection returned by drain.</returns>
    Task<List<string>> Drain();

    /// <summary>
    /// Clears all cached log entries from the cache.
    /// </summary>
    /// <returns>A task that completes when the Serilog Cache Sink has been cleared.</returns>
    Task Clear();

    /// <summary>
    /// Enables the sink to accept log events.
    /// </summary>
    /// <returns>A task that completes when the enable operation is complete.</returns>
    Task Enable();

    /// <summary>
    /// Disables the sink from accepting log events.
    /// </summary>
    /// <returns>A task that completes when the disable operation is complete.</returns>
    Task Disable();
}
