using System;
using System.Runtime.Serialization;

namespace Libaec;

/// <summary>
/// Represents errors that occur during Libaec operations.
/// </summary>
[Serializable]
public class AecException : Exception
{
    /// <summary>
    /// Operation return code.
    /// </summary>
    public int ReturnCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AecException"/> class.
    /// </summary>
    public AecException() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AecException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public AecException(string message, int returnCode) : base(message)
    {
        ReturnCode = returnCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AecException"/> class with a specified error message 
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, 
    /// or a null reference if no inner exception is specified.</param>
    public AecException(string message, int returnCode, Exception innerException)
        : base(message, innerException)
    {
        ReturnCode = returnCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AecException"/> class with serialized data.
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
    /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="info"/> parameter is null.</exception>
    /// <exception cref="SerializationException">The class name is null or <see cref="Exception.HResult"/> is zero (0).</exception>
    protected AecException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}
