using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZLogger;

namespace ZLoggerExtensions.AdvanceFileLogger;

/// <summary>
/// Main provider class implementing ZLogger provider pattern for AdvanceFileLogger.
/// </summary>
[ProviderAlias("AdvanceFile")]
public class AdvanceFileLoggerProvider : ILoggerProvider, IDisposable, ISupportExternalScope
{
    private readonly AdvanceFileLoggerOptions _options;
    private readonly ConcurrentDictionary<string, AdvanceFileLogger> _loggers;
    private readonly AdvanceFileWriter _fileWriter;
    private IExternalScopeProvider? _scopeProvider;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the AdvanceFileLoggerProvider class.
    /// </summary>
    /// <param name="options">The logger options.</param>
    public AdvanceFileLoggerProvider(AdvanceFileLoggerOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _options.Validate();

        _loggers = new ConcurrentDictionary<string, AdvanceFileLogger>();
        _fileWriter = new AdvanceFileWriter(_options);
    }

    /// <summary>
    /// Initializes a new instance of the AdvanceFileLoggerProvider class.
    /// </summary>
    /// <param name="optionsMonitor">The options monitor.</param>
    public AdvanceFileLoggerProvider(IOptionsMonitor<AdvanceFileLoggerOptions> optionsMonitor)
        : this(optionsMonitor?.CurrentValue ?? throw new ArgumentNullException(nameof(optionsMonitor)))
    {
    }

    /// <summary>
    /// Creates a new logger instance.
    /// </summary>
    /// <param name="categoryName">The category name for the logger.</param>
    /// <returns>A new logger instance.</returns>
    public ILogger CreateLogger(string categoryName)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(AdvanceFileLoggerProvider));

        return _loggers.GetOrAdd(categoryName, name => new AdvanceFileLogger(name, _fileWriter, _scopeProvider));
    }

    /// <summary>
    /// Sets the scope provider for external scope support.
    /// </summary>
    /// <param name="scopeProvider">The scope provider.</param>
    public void SetScopeProvider(IExternalScopeProvider scopeProvider)
    {
        _scopeProvider = scopeProvider;

        // Update existing loggers with the new scope provider
        foreach (var logger in _loggers.Values)
        {
            logger.SetScopeProvider(scopeProvider);
        }
    }

    /// <summary>
    /// Forces a file roll operation on the underlying file writer.
    /// </summary>
    public void ForceRoll()
    {
        if (!_disposed)
        {
            _fileWriter.ForceRoll();
        }
    }

    /// <summary>
    /// Flushes any buffered log data.
    /// </summary>
    public void Flush()
    {
        if (!_disposed)
        {
            _fileWriter.Flush();
        }
    }

    /// <summary>
    /// Gets the current file size in bytes.
    /// </summary>
    public long CurrentFileSize => _fileWriter.CurrentFileSize;

    /// <summary>
    /// Disposes the provider and releases all resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the provider and releases all resources.
    /// </summary>
    /// <param name="disposing">True if disposing, false if finalizing.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose all loggers
                foreach (var logger in _loggers.Values)
                {
                    logger.Dispose();
                }
                _loggers.Clear();

                // Dispose file writer
                _fileWriter?.Dispose();
            }

            _disposed = true;
        }
    }
}

/// <summary>
/// Logger implementation for AdvanceFileLogger.
/// </summary>
internal class AdvanceFileLogger : ILogger, IDisposable
{
    private readonly string _categoryName;
    private readonly AdvanceFileWriter _fileWriter;
    private IExternalScopeProvider? _scopeProvider;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the AdvanceFileLogger class.
    /// </summary>
    /// <param name="categoryName">The category name for the logger.</param>
    /// <param name="fileWriter">The file writer instance.</param>
    /// <param name="scopeProvider">The scope provider.</param>
    public AdvanceFileLogger(string categoryName, AdvanceFileWriter fileWriter, IExternalScopeProvider? scopeProvider)
    {
        _categoryName = categoryName ?? throw new ArgumentNullException(nameof(categoryName));
        _fileWriter = fileWriter ?? throw new ArgumentNullException(nameof(fileWriter));
        _scopeProvider = scopeProvider;
    }

    /// <summary>
    /// Sets the scope provider.
    /// </summary>
    /// <param name="scopeProvider">The scope provider.</param>
    public void SetScopeProvider(IExternalScopeProvider? scopeProvider)
    {
        _scopeProvider = scopeProvider;
    }

    /// <summary>
    /// Begins a logical operation scope.
    /// </summary>
    /// <typeparam name="TState">The state type.</typeparam>
    /// <param name="state">The state object.</param>
    /// <returns>A disposable scope object.</returns>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return _scopeProvider?.Push(state);
    }

    /// <summary>
    /// Checks if the specified log level is enabled.
    /// </summary>
    /// <param name="logLevel">The log level to check.</param>
    /// <returns>True if the log level is enabled, false otherwise.</returns>
    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None && !_disposed && !_fileWriter.IsDisposed;
    }

    /// <summary>
    /// Logs a message.
    /// </summary>
    /// <typeparam name="TState">The state type.</typeparam>
    /// <param name="logLevel">The log level.</param>
    /// <param name="eventId">The event identifier.</param>
    /// <param name="state">The state object.</param>
    /// <param name="exception">The exception, if any.</param>
    /// <param name="formatter">The log message formatter.</param>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel) || formatter == null)
            return;

        try
        {
            var message = FormatLogMessage(logLevel, eventId, state, exception, formatter);
            _fileWriter.Write(message);
        }
        catch (Exception)
        {
            // Swallow exceptions to prevent logging from breaking the application
        }
    }

    private string FormatLogMessage<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var level = GetLogLevelString(logLevel);
        var message = formatter(state, exception);

        var logEntry = $"{timestamp} [{level}] {_categoryName}: {message}";

        // Add exception details if present
        if (exception != null)
        {
            logEntry += Environment.NewLine + exception.ToString();
        }

        // Add scope information if available
        var scopeInfo = GetScopeInformation();
        if (!string.IsNullOrEmpty(scopeInfo))
        {
            logEntry += $" => {scopeInfo}";
        }

        return logEntry;
    }

    private string GetScopeInformation()
    {
        var scopeInfo = string.Empty;
        _scopeProvider?.ForEachScope((scope, state) =>
        {
            if (!string.IsNullOrEmpty(scopeInfo))
                scopeInfo += " => ";
            scopeInfo += scope?.ToString();
        }, (object?)null);

        return scopeInfo;
    }

    private static string GetLogLevelString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "TRCE",
            LogLevel.Debug => "DBUG",
            LogLevel.Information => "INFO",
            LogLevel.Warning => "WARN",
            LogLevel.Error => "FAIL",
            LogLevel.Critical => "CRIT",
            _ => "UNKN"
        };
    }

    /// <summary>
    /// Disposes the logger.
    /// </summary>
    public void Dispose()
    {
        _disposed = true;
    }
}