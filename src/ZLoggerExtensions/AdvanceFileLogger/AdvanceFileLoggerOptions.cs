using System;

namespace Deneblab.ZLoggerExtensions.AdvanceFileLogger;

/// <summary>
/// Configuration options for AdvanceFileLogger provider.
/// </summary>
public class AdvanceFileLoggerOptions
{
    /// <summary>
    /// Gets or sets the base file path for log files.
    /// </summary>
    public string FilePath { get; set; } = "logs/app.log";

    /// <summary>
    /// Roll when file exceeds this size (0 disables size rolling).
    /// </summary>
    public long MaxBytes { get; set; } = 50 * 1024 * 1024; // 50 MB

    /// <summary>
    /// Gets or sets the archive directory name relative to the log file directory.
    /// </summary>
    public string ArchiveDirectory { get; set; } = "archive";

    /// <summary>
    /// Gets or sets the maximum number of archived files to retain (default: 7).
    /// </summary>
    public int MaxArchivedFiles { get; set; } = 7;

    /// <summary>
    /// Gets or sets whether to create directories if they don't exist.
    /// </summary>
    public bool CreateDirectories { get; set; } = true;

    /// <summary>
    /// Gets or sets the file encoding.
    /// </summary>
    public System.Text.Encoding Encoding { get; set; } = System.Text.Encoding.UTF8;

    /// <summary>
    /// Gets or sets whether to flush after each write operation.
    /// </summary>
    public bool AutoFlush { get; set; } = true;

    /// <summary>
    /// Gets or sets the buffer size for file operations.
    /// </summary>
    public int BufferSize { get; set; } = 4096;

    /// <summary>
    /// Gets or sets whether to allow external processes to read/delete log files during logging.
    /// </summary>
    public bool AllowExternalAccess { get; set; } = true;

    /// <summary>
    /// Validates the configuration options.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when configuration is invalid.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(FilePath))
            throw new ArgumentException("FilePath cannot be null or empty.", nameof(FilePath));

        if (MaxBytes < 0)
            throw new ArgumentException("MaxBytes cannot be negative.", nameof(MaxBytes));

        if (MaxArchivedFiles < 0)
            throw new ArgumentException("MaxArchivedFiles cannot be negative.", nameof(MaxArchivedFiles));

        if (string.IsNullOrWhiteSpace(ArchiveDirectory))
            throw new ArgumentException("ArchiveDirectory cannot be null or empty.", nameof(ArchiveDirectory));

        if (BufferSize <= 0)
            throw new ArgumentException("BufferSize must be positive.", nameof(BufferSize));
    }
}