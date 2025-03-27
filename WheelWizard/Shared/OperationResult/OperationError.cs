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

    public static implicit operator OperationError(string message) => new() { Message = message };
}
