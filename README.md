# ZLogger Extensions

[![CI/CD](https://github.com/DenebLab/ZLogger.Extensions/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/DenebLab/ZLogger.Extensions/actions/workflows/ci-cd.yml)
[![NuGet Version](https://img.shields.io/nuget/v/Deneblab.ZLoggerExtensions)](https://www.nuget.org/packages/Deneblab.ZLoggerExtensions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Extensions and advanced features for [ZLogger](https://github.com/Cysharp/ZLogger), including advanced file logging with archiving, rotation, and enhanced console logging capabilities.

## Features Overview

- **🚀 Advanced File Logger**: Enterprise-grade file logging with automatic rolling, archiving, and retention policies
- **📋 AppName Configuration**: Customizable application names in log files for easy identification
- **🎨 Console Logger with Colors**: ANSI color-coded console output with multiple formatting modes
- **⚡ Shorter Extension Methods**: Convenient `.Info()`, `.Warn()`, `.Error()` methods with ZLogger performance
- **🔄 ZLogger RollingFile Style**: Date-based file naming with index rolling (`app.2025-09-29_0.log`)
- **📦 Easy Integration**: Seamless integration with .NET's `ILoggingBuilder` infrastructure
- **⚙️ Flexible Configuration**: Support for both code-based and `appsettings.json` configuration
- **🔒 Thread-Safe**: All components are thread-safe for multi-threaded applications
- **🔧 External Process Support**: Files can be accessed by external tools during active logging
- **🌐 Cross-Platform**: Works on Windows, Linux, and macOS

## Available Loggers

This library provides the following logger implementations:

### 🚀 Advanced File Logger (`AdvanceFileLogger`)

Enterprise-grade file logging with advanced features:

**Key Features:**
- **Size-based Rolling**: Automatic file rotation based on configurable size limits
- **AppName Configuration**: Customizable application names in log file names
- **ZLogger RollingFile Style**: Date-based naming pattern (`{AppName}.{yyyy-MM-dd}_{index}.log`)
- **Automatic Archiving**: Move rolled files to organized archive directories
- **Retention Policies**: Configurable cleanup of old archived files
- **External Process Support**: Files can be read/deleted by external processes during logging
- **Thread-Safe Operations**: Safe for concurrent use in multi-threaded applications
- **Cross-Platform**: Works on Windows, Linux, and macOS

**Usage:**
```csharp
// Basic usage - creates files like "app.2025-09-29_0.log"
services.AddLogging(builder =>
{
    builder.AddAdvanceFileLogger("logs/app.log");
});

// With AppName - creates files like "MyAPI.2025-09-29_0.log"
services.AddLogging(builder =>
{
    builder.AddZLoggerRollingFile("MyAPI", 1024 * 1024); // 1MB files
});

// Advanced configuration with AppName
services.AddLogging(builder =>
{
    builder.AddAdvanceFileLogger(options =>
    {
        options.AppName = "OrderService";
        options.MaxBytes = 100 * 1024 * 1024; // 100MB
        options.MaxArchivedFiles = 10;
        options.ArchiveDirectory = "archive";
        options.AllowExternalAccess = true;
    });
    // Creates files like: OrderService.2025-09-29_0.log
});
```

### 🎨 Console Logger with Colors (`ConsoleWithColorsLogger`)

Enhanced console logging with color-coded log levels and multiple formatting options:

**Key Features:**
- **Color-Coded Log Levels**: Visual distinction for different log severities
- **Multiple Verbosity Modes**: Choose from 3 different formatting styles
- **ANSI Escape Code Support**: Full terminal color support
- **UTF-8 Output**: Proper encoding for international characters
- **Customizable Formatting**: Flexible timestamp and category display options

**Verbosity Modes:**
1. **`TimeOnlyLocalLogLevel`**: Shows local time and colored log levels
   ```
   14:30:15 [INFO] Application started
   14:30:16 [WARN] Low disk space
   14:30:17 [ERROR] Connection failed
   ```

2. **`DataTimeUtcLogLevelCategory`**: Full UTC datetime with category information
   ```
   2023-12-25T14:30:15.123Z|INFO|MyApp.Services.UserService|User login successful
   2023-12-25T14:30:16.456Z|WARN|MyApp.Data.Repository|Cache miss detected
   ```

3. **`LogLevelLineColor`**: Entire log line colored by severity level
   ```
   [INFO] Application ready to serve requests
   [WARN] Performance threshold exceeded
   [ERROR] Database connection timeout
   ```

**Usage:**
```csharp
// Basic usage with default settings
services.AddLogging(builder =>
{
    builder.AddZLoggerConsoleWithColors();
});

// Custom verbosity mode
services.AddLogging(builder =>
{
    builder.AddZLoggerConsoleWithColors(options =>
    {
        options.LogVerbosity = LogVerbosity.DataTimeUtcLogLevelCategory;
    });
});
```

**Color Scheme:**
- **Trace**: Light gray (`\u001b[38;2;200;200;200m`)
- **Debug**: Default terminal color (no coloring)
- **Information**: Cyan (`\u001B[36m`)
- **Warning**: Yellow (`\u001b[33m`)
- **Error/Critical**: Red (`\u001b[31m`)

### 🔄 ZLogger RollingFile-Style Configuration

Convenient methods that follow ZLogger's rolling file conventions with date-based naming:

**Key Features:**
- **Date-based Rolling**: Files named with date and index (`AppName.yyyy-MM-dd_index.log`)
- **AppName Integration**: Easily identify logs by application name
- **Automatic Daily Rolling**: New files created automatically each day
- **Size-based Sub-rolling**: Multiple files per day when size limits are exceeded
- **ZLogger Compatibility**: Follows ZLogger's established patterns

**Usage:**
```csharp
// Simple rolling file with custom app name
services.AddLogging(builder =>
{
    builder.AddZLoggerRollingFile("WebAPI", 10 * 1024 * 1024); // 10MB files
    // Creates: WebAPI.2025-09-29_0.log, WebAPI.2025-09-29_1.log, etc.
});

// With retention policy
services.AddLogging(builder =>
{
    builder.AddZLoggerRollingFile("OrderService", 5 * 1024 * 1024, 30); // Keep 30 days
    // Creates: OrderService.2025-09-29_0.log, archived after 30 days
});

// Custom file naming pattern with AppName
services.AddLogging(builder =>
{
    builder.AddAdvanceFileLogger(options =>
    {
        options.AppName = "PaymentAPI";
        options.FileNameProvider = (dt, index) =>
            $"logs/{options.AppName}/{dt:yyyy}/{dt:MM}/{options.AppName}.{dt:yyyy-MM-dd}_{index}.log";
        options.MaxBytes = 50 * 1024 * 1024;
    });
    // Creates: logs/PaymentAPI/2025/09/PaymentAPI.2025-09-29_0.log
});
```

### ⚡ Shorter Extension Methods

Convenient shorter method names with ZLogger's high-performance interpolated string handlers:

**Available Methods:**
- `.Trace($"message")` - Shorter alternative to `.LogTrace()`
- `.Debug($"message")` - Shorter alternative to `.LogDebug()`
- `.Info($"message")` - Shorter alternative to `.LogInformation()`
- `.Warn($"message")` - Shorter alternative to `.LogWarning()`
- `.Error($"message")` - Shorter alternative to `.LogError()`
- `.Fatal($"message")` - Shorter alternative to `.LogCritical()`

**Performance Benefits:**
- Uses ZLogger's interpolated string handlers for zero-allocation logging
- Automatic caller information (`CallerMemberName`, `CallerFilePath`, `CallerLineNumber`)
- Better performance than traditional string formatting

**Usage:**
```csharp
using Deneblab.ZLoggerExtensions; // Enable shorter methods

// Traditional way
logger.LogInformation("User {UserId} logged in at {LoginTime}", userId, DateTime.Now);
logger.LogWarning("Cache miss for key {CacheKey}", cacheKey);
logger.LogError("Failed to process order {OrderId}", orderId);

// Shorter way - same performance, cleaner syntax
logger.Info($"User {userId} logged in at {DateTime.Now}");
logger.Warn($"Cache miss for key {cacheKey}");
logger.Error($"Failed to process order {orderId}");
```

**Context Support:**
```csharp
// With context object and caller information
logger.Info($"Processing started", context: new { OrderId = 123 });
logger.Error($"Validation failed for {field}", context: validationContext);
```

### 📋 Logger Comparison

| Feature | Advanced File Logger | Console with Colors |
|---------|---------------------|-------------------|
| **Output Target** | Files on disk | Console/Terminal |
| **Rolling/Rotation** | ✅ Size-based | ❌ N/A |
| **Archiving** | ✅ Automatic | ❌ N/A |
| **Colors** | ❌ Plain text | ✅ ANSI colors |
| **Formatting Options** | ✅ Structured | ✅ Multiple modes |
| **External Access** | ✅ Configurable | ❌ N/A |
| **Performance** | ✅ High (buffered) | ✅ High (direct) |
| **Thread Safety** | ✅ Yes | ✅ Yes |
| **Cross-Platform** | ✅ Yes | ✅ Yes |

### 🔧 Combined Usage

You can use both loggers together for comprehensive logging:

```csharp
services.AddLogging(builder =>
{
    // File logging for persistence with AppName
    builder.AddZLoggerRollingFile("MyApplication", 50 * 1024 * 1024, 7);
    // Creates: MyApplication.2025-09-29_0.log, MyApplication.2025-09-29_1.log, etc.

    // Console logging for development
    builder.AddZLoggerConsoleWithColors(options =>
    {
        options.LogVerbosity = LogVerbosity.TimeOnlyLocalLogLevel;
    });
});

// Or with advanced configuration
services.AddLogging(builder =>
{
    builder.AddAdvanceFileLogger(options =>
    {
        options.AppName = "WebAPI";
        options.MaxBytes = 50 * 1024 * 1024; // 50MB
        options.MaxArchivedFiles = 7;
        options.ArchiveDirectory = "logs/archive";
    });
    // Creates: WebAPI.2025-09-29_0.log

    builder.AddZLoggerConsoleWithColors();
});
```

This setup provides:
- **Persistent logging** to files with automatic rotation and archiving
- **Real-time console feedback** with color-coded severity levels
- **Development-friendly** output for debugging
- **Production-ready** file management for monitoring

## Installation

Install the package via NuGet Package Manager:

```bash
dotnet add package Deneblab.ZLoggerExtensions
```

Or via Package Manager Console:

```powershell
Install-Package Deneblab.ZLoggerExtensions
```

## Quick Start

### Advanced File Logger with AppName

```csharp
using Microsoft.Extensions.Logging;
using Deneblab.ZLoggerExtensions.AdvanceFileLogger;

var services = new ServiceCollection();
services.AddLogging(builder =>
{
    // Creates files like: MyApp.2025-09-29_0.log
    builder.AddZLoggerRollingFile("MyApp", 10 * 1024 * 1024); // 10MB files
});

var serviceProvider = services.BuildServiceProvider();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

logger.LogInformation("Application started");
logger.LogWarning("This is a warning");
logger.LogError("This is an error");
```

### Traditional Advanced File Logger

```csharp
var services = new ServiceCollection();
services.AddLogging(builder =>
{
    // Creates files like: app.2025-09-29_0.log (default AppName)
    builder.AddAdvanceFileLogger("logs/app.log");
});
```

### Console Logger with Colors

```csharp
using Microsoft.Extensions.Logging;
using Deneblab.ZLoggerExtensions.ConsoleWithColorsLogger;

var services = new ServiceCollection();
services.AddLogging(builder =>
{
    builder.AddZLoggerConsoleWithColors();
});

var serviceProvider = services.BuildServiceProvider();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

logger.LogInformation("Colorful console logging!");
logger.LogWarning("This appears in yellow");
logger.LogError("This appears in red");
```

### Shorter Extension Methods

```csharp
using Microsoft.Extensions.Logging;
using Deneblab.ZLoggerExtensions; // Enable shorter methods

var services = new ServiceCollection();
services.AddLogging(builder =>
{
    builder.AddZLoggerRollingFile("MyApp", 10 * 1024 * 1024);
    builder.AddZLoggerConsoleWithColors();
});

var serviceProvider = services.BuildServiceProvider();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

// Use shorter method names with interpolated strings
logger.Info($"Application started at {DateTime.Now}");
logger.Warn($"Warning: Memory usage is {memoryUsage}%");
logger.Error($"Failed to process {itemId}");
```

## Quick Reference

### Logger Methods

| Logger Type | Extension Method | Primary Use Case |
|-------------|------------------|------------------|
| **Advanced File** | `AddAdvanceFileLogger()` | Production logging with file management |
| **ZLogger RollingFile** | `AddZLoggerRollingFile()` | AppName-based rolling files with date pattern |
| **Console Colors** | `AddZLoggerConsoleWithColors()` | Development and debugging with visual feedback |
| **Shorter Extensions** | `.Info()`, `.Warn()`, `.Error()` | Concise logging with ZLogger performance |

### Common Configuration Patterns

```csharp
// Production setup: File + Console with AppName
services.AddLogging(builder =>
{
    builder.AddZLoggerRollingFile("ProductionAPI", maxBytes: 100_000_000);
    // Creates: ProductionAPI.2025-09-29_0.log
    builder.AddZLoggerConsoleWithColors();
});

// Development setup: Console only with detailed output
services.AddLogging(builder =>
{
    builder.AddZLoggerConsoleWithColors(opts =>
        opts.LogVerbosity = LogVerbosity.DataTimeUtcLogLevelCategory);
});

// High-volume production: File with AppName and aggressive archiving
services.AddLogging(builder =>
{
    builder.AddAdvanceFileLogger(opts =>
    {
        opts.AppName = "HighVolumeService";
        opts.MaxBytes = 50_000_000; // 50MB
        opts.MaxArchivedFiles = 20;
        opts.AutoFlush = false; // Better performance
    });
    // Creates: HighVolumeService.2025-09-29_0.log
});

// Microservice setup: Service-specific logging
services.AddLogging(builder =>
{
    builder.AddZLoggerRollingFile("OrderService", 20_000_000, 14); // 20MB, 14 days
    builder.AddZLoggerRollingFile("PaymentService", 10_000_000, 30); // 10MB, 30 days
});
```

### Advanced Configuration

```csharp
services.AddLogging(builder =>
{
    builder.AddAdvanceFileLogger(options =>
    {
        options.AppName = "MyApplicationAPI";
        options.MaxBytes = 100 * 1024 * 1024; // 100MB
        options.MaxArchivedFiles = 10;
        options.ArchiveDirectory = "archive";
        options.AllowExternalAccess = true;
        options.AutoFlush = true;
        // Creates: MyApplicationAPI.2025-09-29_0.log
    });
});

// Or with custom FileNameProvider
services.AddLogging(builder =>
{
    builder.AddAdvanceFileLogger(options =>
    {
        options.AppName = "CustomApp";
        options.FileNameProvider = (dt, index) =>
            $"logs/{options.AppName}/{dt:yyyy}/{dt:MM}/{options.AppName}_{dt:yyyy-MM-dd}_{index:D3}.log";
        options.MaxBytes = 50 * 1024 * 1024;
        // Creates: logs/CustomApp/2025/09/CustomApp_2025-09-29_000.log
    });
});
```

### Configuration via appsettings.json

```json
{
  "Logging": {
    "AdvanceFile": {
      "AppName": "MyWebAPI",
      "MaxBytes": 52428800,
      "MaxArchivedFiles": 7,
      "ArchiveDirectory": "archive",
      "AllowExternalAccess": true,
      "AutoFlush": true
    }
  }
}
```

**Note**: With AppName configuration, files will be created as `MyWebAPI.2025-09-29_0.log`, `MyWebAPI.2025-09-29_1.log`, etc.

## Advanced File Logger

The Advanced File Logger provides enterprise-grade file logging capabilities with the following features:

### Key Features

- **Size-based Rolling**: Configurable file size limit (default: 50MB)
- **Automatic Archiving**: Move rolled files to archive subdirectory
- **Retention Policy**: Configurable number of archived files to keep (default: 7)
- **External Process Support**: Allow reading/deleting log files during logging
- **ZLogger Integration**: Full compatibility with ZLogger architecture

### Configuration Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `AppName` | string | "app" | Application name used in file names |
| `FileNameProvider` | Func<DateTime, int, string> | null | Custom file naming function (overrides AppName pattern) |
| `FilePath` | string | "logs/app.log" | Base file path (used with legacy configuration) |
| `MaxBytes` | long | 50MB | File size limit for rolling (0 disables) |
| `MaxArchivedFiles` | int | 7 | Maximum archived files to retain |
| `ArchiveDirectory` | string | "archive" | Archive directory name |
| `AllowExternalAccess` | bool | true | Allow external process file access |
| `AutoFlush` | bool | true | Flush after each write |
| `CreateDirectories` | bool | true | Create directories if missing |
| `Encoding` | Encoding | UTF8 | File encoding |
| `BufferSize` | int | 4096 | File buffer size |

### File Naming Convention

The AdvanceFileLogger uses ZLogger's rolling file naming convention with date-based patterns:

**Default AppName Pattern:**
- Current files: `{AppName}.{yyyy-MM-dd}_0.log`, `{AppName}.{yyyy-MM-dd}_1.log`
- Example: `MyApp.2025-09-29_0.log`, `MyApp.2025-09-29_1.log`
- Archived files: `archive/{AppName}.{yyyy-MM-dd}_0.log`

**Daily Rolling:**
- New day creates new files: `MyApp.2025-09-30_0.log`
- Size-based rolling creates: `MyApp.2025-09-29_0.log`, `MyApp.2025-09-29_1.log`

## Console Logger with Colors

Enhanced console logging with color support for different log levels, providing better visual distinction in console applications.

## Architecture

The library consists of several well-designed components:

### Advanced File Logger Components

- **AdvanceFileLoggerProvider**: Main provider implementing `ILoggerProvider`
- **AdvanceFileWriter**: Core file writing with rolling logic
- **FileNameProvider**: Handles ZLogger-style file naming
- **FileArchiver**: Manages archiving and retention policy
- **AdvanceFileLoggerOptions**: Configuration class

## Requirements

- **.NET 8.0** or later
- **ZLogger 2.5.10** or later

## Dependencies

- **ZLogger**: High-performance structured logging library
- **Microsoft.Extensions.Logging**: .NET logging abstractions
- **Microsoft.Extensions.DependencyInjection**: Dependency injection framework

## Performance

The library is designed for high performance:

- Configurable buffer size for optimal I/O performance
- Optional auto-flush for immediate persistence
- Efficient file size monitoring
- Minimal memory allocation during logging
- Thread-safe operations without blocking

## Error Handling

The logger is designed to be resilient:

- Failed writes attempt recovery by reopening files
- Archive failures don't stop logging
- Invalid configurations are validated at startup
- Graceful degradation when external processes interfere

## Testing

The library includes comprehensive test coverage:

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

Test coverage includes:
- Basic logging functionality
- File rolling behavior
- Archive retention policy
- Configuration validation
- Provider integration
- External file access scenarios
- Thread safety verification

## Contributing

We welcome contributions! Please see our contributing guidelines for details on how to:

- Report bugs
- Suggest enhancements
- Submit pull requests
- Follow coding standards

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [ZLogger](https://github.com/Cysharp/ZLogger) - The foundational logging library this extends
- [Microsoft.Extensions.Logging](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging) - .NET logging infrastructure

## Support

- 📖 [Documentation](https://github.com/DenebLab/ZLogger.Extensions/wiki)
- 🐛 [Issues](https://github.com/DenebLab/ZLogger.Extensions/issues)
- 💬 [Discussions](https://github.com/DenebLab/ZLogger.Extensions/discussions)

---

Made with ❤️ by [DenebLab](https://github.com/DenebLab)