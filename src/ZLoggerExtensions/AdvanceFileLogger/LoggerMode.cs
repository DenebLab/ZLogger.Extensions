namespace Deneblab.ZLoggerExtensions.AdvanceFileLogger;

/// <summary>
/// Specifies the logging mode that determines file access behavior.
/// </summary>
public enum LoggerMode
{
    /// <summary>
    /// Production mode - optimized for performance. The file handle remains open for maximum throughput.
    /// Use this mode in production environments where logging performance is critical.
    /// </summary>
    Production = 0,

    /// <summary>
    /// Development mode - optimized for file accessibility. The file is closed after each write operation,
    /// allowing external processes to read, modify, or delete log files during logging.
    /// Use this mode in development environments where you need to inspect or manipulate log files in real-time.
    /// Note: This mode has lower performance compared to Production mode due to frequent file open/close operations.
    /// </summary>
    Development = 1
}
