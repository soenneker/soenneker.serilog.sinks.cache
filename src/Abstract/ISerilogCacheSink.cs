using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog.Core;

namespace Soenneker.Serilog.Sinks.Cache.Abstract;

/// <summary>
/// A Serilog sink cache that allows for storing, retrieving, and removing log messages.
/// Queue-backed in-memory log cache for Serilog with optional capacity and byte budget limits.
/// </summary>
public interface ISerilogCacheSink : ILogEventSink, IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Gets the optional capacity limit for the cache. Returns null if unbounded.
    /// </summary>
    int? Capacity { get; }

    /// <summary>
    /// Gets the optional byte budget limit for the cache. Returns null if no byte limit.
    /// </summary>
    long? ByteBudget { get; }

    /// <summary>
    /// Gets whether the sink is currently enabled and will accept log events.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Gets a snapshot of all cached log entries without removing them from the cache.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation and contains the list of cached log entries</returns>
    Task<List<string>> Snapshot();

    /// <summary>
    /// Drains all cached log entries, removing them from the cache and returning them.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation and contains the list of drained log entries</returns>
    Task<List<string>> Drain();

    /// <summary>
    /// Clears all cached log entries from the cache.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task Clear();

    /// <summary>
    /// Enables the sink to accept log events.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task Enable();

    /// <summary>
    /// Disables the sink from accepting log events.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task Disable();
}
