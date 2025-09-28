using System;
using System.Threading.Tasks;
using ZLogger;

namespace Deneblab.ZLoggerExtensions.AdvanceFileLogger;

/// <summary>
/// ZLogger IAsyncLogProcessor implementation for advanced file logging with rolling and archiving.
/// </summary>
public class AdvanceFileAsyncLogProcessor : IAsyncLogProcessor, IDisposable
{
    private readonly AdvanceFileLoggerOptions _options;
    private readonly AdvanceFileWriter _fileWriter;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the AdvanceFileAsyncLogProcessor class.
    /// </summary>
    /// <param name="options">The logger options.</param>
    public AdvanceFileAsyncLogProcessor(AdvanceFileLoggerOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _options.Validate();

        _fileWriter = new AdvanceFileWriter(_options);
    }

    /// <summary>
    /// Processes a log entry using ZLogger's optimized IZLoggerEntry.
    /// </summary>
    /// <param name="log">The log entry to process.</param>
    public void Post(IZLoggerEntry log)
    {
        if (log == null || _disposed)
        {
            log?.Return();
            return;
        }

        try
        {
            // Use ZLogger's optimized formatting
            var formattedMessage = log.ToString();

            // Return the log entry to the pool for memory efficiency
            log.Return();

            // Write to file using existing file writer logic
            if (!string.IsNullOrEmpty(formattedMessage))
            {
                _fileWriter.Write(formattedMessage);
            }
        }
        catch (Exception)
        {
            // Ensure log entry is returned even on error
            try
            {
                log?.Return();
            }
            catch
            {
                // Ignore Return exceptions
            }

            // Swallow exceptions to prevent logging from breaking the application
            // This matches the behavior of the original implementation
        }
    }

    /// <summary>
    /// Asynchronously disposes the processor and releases all resources.
    /// </summary>
    /// <returns>A task representing the disposal operation.</returns>
    public ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            _disposed = true;

            // Dispose the file writer synchronously as it doesn't have async disposal
            _fileWriter?.Dispose();
        }

        return ValueTask.CompletedTask;
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
    /// Asynchronously flushes any buffered log data.
    /// </summary>
    /// <returns>A task representing the flush operation.</returns>
    public async Task FlushAsync()
    {
        if (!_disposed)
        {
            await _fileWriter.FlushAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Gets the current file size in bytes.
    /// </summary>
    public long CurrentFileSize => _disposed ? 0 : _fileWriter.CurrentFileSize;

    /// <summary>
    /// Gets whether the processor is disposed.
    /// </summary>
    public bool IsDisposed => _disposed;

    /// <summary>
    /// Synchronously disposes the processor and releases all resources.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _fileWriter?.Dispose();
        }
    }
}