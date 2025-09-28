using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZLoggerExtensions.AdvanceFileLogger;

/// <summary>
/// Core file writing implementation with size monitoring and rolling logic.
/// </summary>
public class AdvanceFileWriter : IDisposable
{
    private readonly AdvanceFileLoggerOptions _options;
    private readonly FileNameProvider _fileNameProvider;
    private readonly FileArchiver _fileArchiver;
    private readonly object _lock = new object();

    private FileStream? _fileStream;
    private StreamWriter? _streamWriter;
    private long _currentFileSize;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the AdvanceFileWriter class.
    /// </summary>
    /// <param name="options">The logger options.</param>
    public AdvanceFileWriter(AdvanceFileLoggerOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _options.Validate();

        _fileNameProvider = new FileNameProvider(_options.FilePath);
        _fileArchiver = new FileArchiver(_fileNameProvider, _options);

        Initialize();
    }

    /// <summary>
    /// Gets the current file size in bytes.
    /// </summary>
    public long CurrentFileSize => _currentFileSize;

    /// <summary>
    /// Gets whether the file writer is disposed.
    /// </summary>
    public bool IsDisposed => _disposed;

    /// <summary>
    /// Writes a message to the log file.
    /// </summary>
    /// <param name="message">The message to write.</param>
    public void Write(string message)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(AdvanceFileWriter));

        if (string.IsNullOrEmpty(message))
            return;

        lock (_lock)
        {
            try
            {
                EnsureFileIsOpen();

                // Check if we need to roll the file before writing
                if (ShouldRollFile(message))
                {
                    RollFile();
                    EnsureFileIsOpen();
                }

                // Write the message
                _streamWriter?.WriteLine(message);

                // Update current file size
                var messageBytes = _options.Encoding.GetByteCount(message + Environment.NewLine);
                _currentFileSize += messageBytes;

                // Flush if auto-flush is enabled
                if (_options.AutoFlush)
                {
                    _streamWriter?.Flush();
                }
            }
            catch (Exception)
            {
                // Try to recover by reopening the file
                CloseFile();
                EnsureFileIsOpen();

                // Retry once
                try
                {
                    _streamWriter?.WriteLine(message);
                    var messageBytes = _options.Encoding.GetByteCount(message + Environment.NewLine);
                    _currentFileSize += messageBytes;

                    if (_options.AutoFlush)
                    {
                        _streamWriter?.Flush();
                    }
                }
                catch (Exception)
                {
                    // If we still can't write, we'll lose this message but not crash
                }
            }
        }
    }

    /// <summary>
    /// Writes a message asynchronously to the log file.
    /// </summary>
    /// <param name="message">The message to write.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task WriteAsync(string message, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(AdvanceFileWriter));

        if (string.IsNullOrEmpty(message))
            return;

        await Task.Run(() => Write(message), cancellationToken);
    }

    /// <summary>
    /// Flushes any buffered data to the file.
    /// </summary>
    public void Flush()
    {
        if (_disposed)
            return;

        lock (_lock)
        {
            _streamWriter?.Flush();
            _fileStream?.Flush();
        }
    }

    /// <summary>
    /// Flushes any buffered data to the file asynchronously.
    /// </summary>
    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            return;

        await Task.Run(() => Flush(), cancellationToken);
    }

    /// <summary>
    /// Forces a file roll operation.
    /// </summary>
    public void ForceRoll()
    {
        if (_disposed)
            return;

        lock (_lock)
        {
            RollFile();
        }
    }

    private void Initialize()
    {
        // Create directory if it doesn't exist
        var logDirectory = _fileNameProvider.GetLogDirectory();
        if (_options.CreateDirectories && !string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        // Initialize current file size if file exists
        var currentFilePath = _fileNameProvider.GetCurrentFilePath();
        if (File.Exists(currentFilePath))
        {
            _currentFileSize = new FileInfo(currentFilePath).Length;
        }
    }

    private void EnsureFileIsOpen()
    {
        if (_streamWriter != null && _fileStream != null)
            return;

        CloseFile();

        var currentFilePath = _fileNameProvider.GetCurrentFilePath();

        // Open file with sharing options to allow external access
        var fileShare = _options.AllowExternalAccess
            ? FileShare.Read | FileShare.Delete
            : FileShare.Read;

        _fileStream = new FileStream(
            currentFilePath,
            FileMode.Append,
            FileAccess.Write,
            fileShare,
            _options.BufferSize);

        _streamWriter = new StreamWriter(_fileStream, _options.Encoding)
        {
            AutoFlush = _options.AutoFlush
        };

        // Update current file size
        _currentFileSize = _fileStream.Length;
    }

    private bool ShouldRollFile(string message)
    {
        // If size rolling is disabled, don't roll
        if (_options.MaxBytes <= 0)
            return false;

        // Calculate the size after writing this message
        var messageBytes = _options.Encoding.GetByteCount(message + Environment.NewLine);
        return _currentFileSize + messageBytes > _options.MaxBytes;
    }

    private void RollFile()
    {
        try
        {
            CloseFile();

            var currentFilePath = _fileNameProvider.GetCurrentFilePath();
            if (File.Exists(currentFilePath))
            {
                // Archive the current file
                _fileArchiver.ArchiveCurrentFile(currentFilePath);
            }

            // Reset file size for new file
            _currentFileSize = 0;
        }
        catch (Exception)
        {
            // If rolling fails, we'll continue with the current file
            // This ensures logging doesn't stop completely
        }
    }

    private void CloseFile()
    {
        _streamWriter?.Dispose();
        _streamWriter = null;

        _fileStream?.Dispose();
        _fileStream = null;
    }

    /// <summary>
    /// Disposes the file writer and releases all resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the file writer and releases all resources.
    /// </summary>
    /// <param name="disposing">True if disposing, false if finalizing.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                lock (_lock)
                {
                    CloseFile();
                }
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Finalizer for AdvanceFileWriter.
    /// </summary>
    ~AdvanceFileWriter()
    {
        Dispose(false);
    }
}