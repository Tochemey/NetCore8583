using System;
using System.Diagnostics;

namespace NetCore8583;

/// <summary>
/// ILogger is an interface that will be implemented
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Info log info message
    /// </summary>
    /// <param name="messageTemplate">the message template</param>
    void Info(string messageTemplate);
    
    /// <summary>
    /// Debug log in debug mode
    /// </summary>
    /// <param name="messageTemplate">the message template</param>
    void Debug(string messageTemplate); 
    
    /// <summary>
    /// Warning logs message in warning mode
    /// </summary>
    /// <param name="messageTemplate">the message template</param>
    void Warning(string messageTemplate);
    
    /// <summary>
    /// Error log error message
    /// </summary>
    /// <param name="messageTemplate">the message template</param>
    void Error(string messageTemplate);
    
    /// <summary>
    /// Error log an Exception with a custom error message
    /// </summary>
    /// <param name="exception">the exception</param>
    /// <param name="messageTemplate">the message template</param>
    void Error(Exception exception, string messageTemplate);
}