using WheelWizard.Helpers;

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

    /// <summary>
    /// The translation applied to this error result for better visualization in the UI.
    /// </summary>
    public MessageTranslation? MessageTranslation { get; set; }

    // Note that the MessageTranslation can NOT be used to retrieve the message.
    // This is because the translation fo the MessageTranslation is localized, while the actual Message MUST be in English.

    #region Implicit Operators

    public static implicit operator OperationError(string errorMessage) => new() { Message = errorMessage };

    public static implicit operator OperationError(Exception exception) => new() { Message = exception.Message, Exception = exception };

    #endregion
}
