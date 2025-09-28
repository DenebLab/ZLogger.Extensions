using Microsoft.Extensions.Logging;
using System;
using System.Runtime.CompilerServices;
using ZLogger;
#pragma warning disable IDE0130

namespace Deneblab.ZLoggerExtensions;

public static class LoggerExtensions
{
    // Trace extensions
    public static void Trace(
        this ILogger logger,
        [InterpolatedStringHandlerArgument("logger")] ref ZLoggerTraceInterpolatedStringHandler message,
        object? context = null,
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        logger.ZLogTrace(ref message, context, memberName, filePath, lineNumber);
    }

    public static void Trace(
        this ILogger logger,
        EventId eventId,
        [InterpolatedStringHandlerArgument("logger")] ref ZLoggerTraceInterpolatedStringHandler message,
        object? context = null,
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        logger.ZLogTrace(eventId, ref message, context, memberName, filePath, lineNumber);
    }

    public static void Trace(
        this ILogger logger,
        Exception? exception,
        [InterpolatedStringHandlerArgument("logger")] ref ZLoggerTraceInterpolatedStringHandler message,
        object? context = null,
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        logger.ZLogTrace(exception, ref message, context, memberName, filePath, lineNumber);
    }

    public static void Trace(
        this ILogger logger,
        EventId eventId,
        Exception? exception,
        [InterpolatedStringHandlerArgument("logger")] ref ZLoggerTraceInterpolatedStringHandler message,
        object? context = null,
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        logger.ZLogTrace(eventId, exception, ref message, context, memberName, filePath, lineNumber);
    }

    // Debug extensions
    public static void Debug(
        this ILogger logger,
        [InterpolatedStringHandlerArgument("logger")] ref ZLoggerDebugInterpolatedStringHandler message,
        object? context = null,
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        logger.ZLogDebug(ref message, context, memberName, filePath, lineNumber);
    }

    public static void Debug(
        this ILogger logger,
        EventId eventId,
        [InterpolatedStringHandlerArgument("logger")] ref ZLoggerDebugInterpolatedStringHandler message,
        object? context = null,
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        logger.ZLogDebug(eventId, ref message, context, memberName, filePath, lineNumber);
    }

    public static void Debug(
        this ILogger logger,
        Exception? exception,
        [InterpolatedStringHandlerArgument("logger")] ref ZLoggerDebugInterpolatedStringHandler message,
        object? context = null,
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        logger.ZLogDebug(exception, ref message, context, memberName, filePath, lineNumber);
    }

    public static void Debug(
        this ILogger logger,
        EventId eventId,
        Exception? exception,
        [InterpolatedStringHandlerArgument("logger")] ref ZLoggerDebugInterpolatedStringHandler message,
        object? context = null,
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        logger.ZLogDebug(eventId, exception, ref message, context, memberName, filePath, lineNumber);
    }

    // Info extensions
    public static void Info(
        this ILogger logger,
        [InterpolatedStringHandlerArgument("logger")] ref ZLoggerInformationInterpolatedStringHandler message,
        object? context = null,
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        logger.ZLogInformation(ref message, context, memberName, filePath, lineNumber);
    }

    public static void Info(
        this ILogger logger,
        EventId eventId,
        [InterpolatedStringHandlerArgument("logger")] ref ZLoggerInformationInterpolatedStringHandler message,
        object? context = null,
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        logger.ZLogInformation(eventId, ref message, context, memberName, filePath, lineNumber);
    }

    public static void Info(
        this ILogger logger,
        Exception? exception,
        [InterpolatedStringHandlerArgument("logger")] ref ZLoggerInformationInterpolatedStringHandler message,
        object? context = null,
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        logger.ZLogInformation(exception, ref message, context, memberName, filePath, lineNumber);
    }

    public static void Info(
        this ILogger logger,
        EventId eventId,
        Exception? exception,
        [InterpolatedStringHandlerArgument("logger")] ref ZLoggerInformationInterpolatedStringHandler message,
        object? context = null,
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        logger.ZLogInformation(eventId, exception, ref message, context, memberName, filePath, lineNumber);
    }

    // Warn extensions
    public static void Warn(
        this ILogger logger,
        [InterpolatedStringHandlerArgument("logger")] ref ZLoggerWarningInterpolatedStringHandler message,
        object? context = null,
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        logger.ZLogWarning(ref message, context, memberName, filePath, lineNumber);
    }

    public static void Warn(
        this ILogger logger,
        EventId eventId,
        [InterpolatedStringHandlerArgument("logger")] ref ZLoggerWarningInterpolatedStringHandler message,
        object? context = null,
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        logger.ZLogWarning(eventId, ref message, context, memberName, filePath, lineNumber);
    }

    public static void Warn(
        this ILogger logger,
        Exception? exception,
        [InterpolatedStringHandlerArgument("logger")] ref ZLoggerWarningInterpolatedStringHandler message,
        object? context = null,
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        logger.ZLogWarning(exception, ref message, context, memberName, filePath, lineNumber);
    }

    public static void Warn(
        this ILogger logger,
        EventId eventId,
        Exception? exception,
        [InterpolatedStringHandlerArgument("logger")] ref ZLoggerWarningInterpolatedStringHandler message,
        object? context = null,
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        logger.ZLogWarning(eventId, exception, ref message, context, memberName, filePath, lineNumber);
    }

    // Error extensions
    public static void Error(
        this ILogger logger,
        [InterpolatedStringHandlerArgument("logger")] ref ZLoggerErrorInterpolatedStringHandler message,
        object? context = null,
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        logger.ZLogError(ref message, context, memberName, filePath, lineNumber);
    }

    public static void Error(
        this ILogger logger,
        EventId eventId,
        [InterpolatedStringHandlerArgument("logger")] ref ZLoggerErrorInterpolatedStringHandler message,
        object? context = null,
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        logger.ZLogError(eventId, ref message, context, memberName, filePath, lineNumber);
    }

    public static void Error(
        this ILogger logger,
        Exception? exception,
        [InterpolatedStringHandlerArgument("logger")] ref ZLoggerErrorInterpolatedStringHandler message,
        object? context = null,
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        logger.ZLogError(exception, ref message, context, memberName, filePath, lineNumber);
    }

    public static void Error(
        this ILogger logger,
        EventId eventId,
        Exception? exception,
        [InterpolatedStringHandlerArgument("logger")] ref ZLoggerErrorInterpolatedStringHandler message,
        object? context = null,
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        logger.ZLogError(eventId, exception, ref message, context, memberName, filePath, lineNumber);
    }

    // Critical extensions
    public static void Critical(
        this ILogger logger,
        [InterpolatedStringHandlerArgument("logger")] ref ZLoggerCriticalInterpolatedStringHandler message,
        object? context = null,
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        logger.ZLogCritical(ref message, context, memberName, filePath, lineNumber);
    }

    public static void Critical(
        this ILogger logger,
        EventId eventId,
        [InterpolatedStringHandlerArgument("logger")] ref ZLoggerCriticalInterpolatedStringHandler message,
        object? context = null,
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        logger.ZLogCritical(eventId, ref message, context, memberName, filePath, lineNumber);
    }

    public static void Critical(
        this ILogger logger,
        Exception? exception,
        [InterpolatedStringHandlerArgument("logger")] ref ZLoggerCriticalInterpolatedStringHandler message,
        object? context = null,
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        logger.ZLogCritical(exception, ref message, context, memberName, filePath, lineNumber);
    }

    public static void Critical(
        this ILogger logger,
        EventId eventId,
        Exception? exception,
        [InterpolatedStringHandlerArgument("logger")] ref ZLoggerCriticalInterpolatedStringHandler message,
        object? context = null,
        [CallerMemberName] string? memberName = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        logger.ZLogCritical(eventId, exception, ref message, context, memberName, filePath, lineNumber);
    }
}