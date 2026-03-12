using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Soenneker.Serilog.Sinks.Cache.Abstract;
using System;

namespace Soenneker.Serilog.Sinks.Cache.Registrars;

/// <summary>
/// A Serilog sink cache that allows for storing, retrieving, and removing log messages
/// </summary>
public static class SerilogCacheSinkRegistrar
{
    /// <summary>
    /// Adds a queue-backed in-memory log cache using an existing instance from DI.
    /// Use this when you've already registered the sink with AddSerilogCacheSink().
    /// </summary>
    /// <param name="writeTo">The logger sink configuration</param>
    /// <param name="serviceProvider">The service provider to get the registered cache sink</param>
    /// <param name="restrictedToMinimumLevel">Minimum log level</param>
    /// <returns>The logger configuration</returns>
    public static LoggerConfiguration LogCache(this LoggerSinkConfiguration writeTo, IServiceProvider serviceProvider, LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum)
    {
        var cache = serviceProvider.GetRequiredService<ISerilogCacheSink>();
        return writeTo.Sink(cache, restrictedToMinimumLevel);
    }

    /// <summary>
    /// Registers the Serilog cache sink in the dependency injection container.
    /// This allows you to inject ISerilogCacheSink to control the sink (enable/disable, get values, etc.).
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="capacity">Optional capacity limit for the cache</param>
    /// <param name="byteBudget">Optional byte budget limit for the cache</param>
    /// <param name="outputTemplate">Output template for log formatting</param>
    /// <param name="formatProvider">Format provider for log formatting</param>
    /// <param name="serviceLifetime">Service lifetime (defaults to Singleton)</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddSerilogCacheSink(this IServiceCollection services, int? capacity = null, long? byteBudget = null,
        string outputTemplate = "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}", IFormatProvider? formatProvider = null,
        ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
    {
        services.Add(new ServiceDescriptor(typeof(ISerilogCacheSink), provider => new SerilogCacheSink(capacity, byteBudget, outputTemplate, formatProvider), serviceLifetime));

        return services;
    }
}