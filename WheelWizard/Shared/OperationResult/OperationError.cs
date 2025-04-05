namespace WheelWizard.Shared;

/// <summary>
/// Represents an error that occurred during an operation.
/// </summary>
public class OperationError
{
    /// <summary>
    /// The error message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// The exception that occurred during the operation, if any.
    /// </summary>
    public Exception? Exception { get; init; }

    #region Implicit Operators

    public static implicit operator OperationError(string errorMessage) => new() { Message = errorMessage };

    public static implicit operator OperationError(Exception exception) => new()
    {
        Message = exception.Message,
        Exception = exception
    };

    #endregion
}
