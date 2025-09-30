using System;
using System.IO;

namespace Deneblab.ZLoggerExtensions.AdvanceFileLogger;

/// <summary>
/// Configuration options for AdvanceFileLogger provider.
/// </summary>
public class AdvanceFileLoggerOptions
{
    /// <summary>
    /// Gets or sets the file name provider function that generates file names (without path) based on date and index.
    /// The function should return only the filename, not the full path.
    /// If null, uses the default provider that generates files in format: {AppName}.{yyyy-MM-dd}.{index:D2}.log
    /// </summary>
    public Func<DateTime, int, string>? FileNameProvider { get; set; }
    /// <summary>
    /// Gets or sets the application name used in log file names.
    /// </summary>
    public string AppName { get; set; } = "app";

    /// <summary>
    /// Gets or sets the directory path where log files will be stored.
    /// </summary>
    public string LogDirPath { get; set; } = "logs";

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
    /// Gets the effective file name provider that returns full paths by combining LogDirPath with filename.
    /// </summary>
    /// <returns>The file name provider function that returns full paths.</returns>
    public Func<DateTime, int, string> GetFileNameProvider()
    {
        if (FileNameProvider != null)
        {
            // Wrap custom provider to combine with LogDirPath
            return (date, index) => Path.Combine(LogDirPath, FileNameProvider(date, index));
        }

        // Default provider - creates ZLogger RollingFile style names: {AppName}.{yyyy-MM-dd}.{index:D2}.log
        return (date, index) =>
            Path.Combine(LogDirPath, $"{AppName}.{date:yyyy-MM-dd}.{index:D2}.log");
    }

    /// <summary>
    /// Validates the configuration options.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when configuration is invalid.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(LogDirPath))
            throw new ArgumentException("LogDirPath cannot be null or empty.", nameof(LogDirPath));

        if (string.IsNullOrWhiteSpace(AppName))
            throw new ArgumentException("AppName cannot be null or empty.", nameof(AppName));

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