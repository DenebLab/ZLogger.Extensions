using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Deneblab.ZLoggerExtensions.AdvanceFileLogger;

/// <summary>
/// Extension methods for ILoggingBuilder to configure AdvanceFileLogger.
/// </summary>
public static class LoggingBuilderExtensions
{
    /// <summary>
    /// Adds the AdvanceFileLogger provider to the logging builder.
    /// </summary>
    /// <param name="builder">The logging builder.</param>
    /// <returns>The logging builder for chaining.</returns>
    public static ILoggingBuilder AddAdvanceFileLogger(this ILoggingBuilder builder)
    {
        return AddAdvanceFileLogger(builder, configure: null);
    }

    /// <summary>
    /// Adds the AdvanceFileLogger processor to the logging builder with configuration using ZLogger primitives.
    /// </summary>
    /// <param name="builder">The logging builder.</param>
    /// <param name="configure">The configuration action.</param>
    /// <returns>The logging builder for chaining.</returns>
    public static ILoggingBuilder AddAdvanceFileLogger(
        this ILoggingBuilder builder,
        Action<AdvanceFileLoggerOptions>? configure)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        // Register the provider and options
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, AdvanceFileLoggerProvider>());
        
        if (configure != null)
        {
            builder.Services.Configure(configure);
        }

        return builder;
    }

    /// <summary>
    /// Adds the AdvanceFileLogger provider to the logging builder with file path.
    /// </summary>
    /// <param name="builder">The logging builder.</param>
    /// <param name="filePath">The log file path.</param>
    /// <returns>The logging builder for chaining.</returns>
    public static ILoggingBuilder AddAdvanceFileLogger(
        this ILoggingBuilder builder,
        string filePath)
    {
        return AddAdvanceFileLogger(builder, options => options.FilePath = filePath);
    }

    /// <summary>
    /// Adds the AdvanceFileLogger provider to the logging builder with file path and size limit.
    /// </summary>
    /// <param name="builder">The logging builder.</param>
    /// <param name="filePath">The log file path.</param>
    /// <param name="maxBytes">The maximum file size in bytes before rolling.</param>
    /// <returns>The logging builder for chaining.</returns>
    public static ILoggingBuilder AddAdvanceFileLogger(
        this ILoggingBuilder builder,
        string filePath,
        long maxBytes)
    {
        return AddAdvanceFileLogger(builder, options =>
        {
            options.FilePath = filePath;
            options.MaxBytes = maxBytes;
        });
    }

    /// <summary>
    /// Adds the AdvanceFileLogger provider to the logging builder with full configuration.
    /// </summary>
    /// <param name="builder">The logging builder.</param>
    /// <param name="filePath">The log file path.</param>
    /// <param name="maxBytes">The maximum file size in bytes before rolling.</param>
    /// <param name="maxArchivedFiles">The maximum number of archived files to keep.</param>
    /// <returns>The logging builder for chaining.</returns>
    public static ILoggingBuilder AddAdvanceFileLogger(
        this ILoggingBuilder builder,
        string filePath,
        long maxBytes,
        int maxArchivedFiles)
    {
        return AddAdvanceFileLogger(builder, options =>
        {
            options.FilePath = filePath;
            options.MaxBytes = maxBytes;
            options.MaxArchivedFiles = maxArchivedFiles;
        });
    }

    /// <summary>
    /// Adds the AdvanceFileLogger provider to the logging builder with complete configuration.
    /// </summary>
    /// <param name="builder">The logging builder.</param>
    /// <param name="filePath">The log file path.</param>
    /// <param name="maxBytes">The maximum file size in bytes before rolling.</param>
    /// <param name="maxArchivedFiles">The maximum number of archived files to keep.</param>
    /// <param name="archiveDirectory">The archive directory name.</param>
    /// <returns>The logging builder for chaining.</returns>
    public static ILoggingBuilder AddAdvanceFileLogger(
        this ILoggingBuilder builder,
        string filePath,
        long maxBytes,
        int maxArchivedFiles,
        string archiveDirectory)
    {
        return AddAdvanceFileLogger(builder, options =>
        {
            options.FilePath = filePath;
            options.MaxBytes = maxBytes;
            options.MaxArchivedFiles = maxArchivedFiles;
            options.ArchiveDirectory = archiveDirectory;
        });
    }
}

/// <summary>
/// Extension methods for IServiceCollection to configure AdvanceFileLogger.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds AdvanceFileLogger services to the service collection using ZLogger primitives.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">The configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAdvanceFileLogger(
        this IServiceCollection services,
        Action<AdvanceFileLoggerOptions>? configure = null)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        // Add logging services first
        services.AddLogging(builder =>
        {
            builder.AddAdvanceFileLogger(configure);
        });

        return services;
    }

    /// <summary>
    /// Adds AdvanceFileLogger services to the service collection with file path.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="filePath">The log file path.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAdvanceFileLogger(
        this IServiceCollection services,
        string filePath)
    {
        return AddAdvanceFileLogger(services, options => options.FilePath = filePath);
    }

    /// <summary>
    /// Adds AdvanceFileLogger services to the service collection with configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="filePath">The log file path.</param>
    /// <param name="maxBytes">The maximum file size in bytes before rolling.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAdvanceFileLogger(
        this IServiceCollection services,
        string filePath,
        long maxBytes)
    {
        return AddAdvanceFileLogger(services, options =>
        {
            options.FilePath = filePath;
            options.MaxBytes = maxBytes;
        });
    }
}