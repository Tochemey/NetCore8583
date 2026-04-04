// MIT License
//
// Copyright (c) 2020 - 2026 Arsene Tochemey Gandote
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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