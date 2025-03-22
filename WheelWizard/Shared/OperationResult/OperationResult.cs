using System.Diagnostics.CodeAnalysis;

namespace WheelWizard.Shared;

/// <summary>
/// Represents the result of an operation.
/// </summary>
public class OperationResult
{
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Error))]
    public virtual bool IsSuccess => Error is null;

    /// <summary>
    /// Indicates whether the operation failed.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Error))]
    public virtual bool IsFailure => !IsSuccess;

    /// <summary>
    /// The error that occurred during the operation, if any.
    /// </summary>
    /// <remarks>
    /// This property is <see langword="null"/> if the operation was successful.
    /// </remarks>
    public OperationError? Error { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OperationResult"/> class.
    /// </summary>
    public OperationResult()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OperationResult"/> class with the specified error.
    /// </summary>
    /// <param name="error">The error that occurred during the operation.</param>
    public OperationResult(OperationError error)
    {
        Error = error;
    }

    public static OperationResult Fail(OperationError error) => new(error);

    public static OperationResult Ok() => new();

    public static OperationResult<T> Fail<T>(OperationError error) => new(error);

    public static OperationResult<T> Ok<T>(T value) => new(value);

    public static implicit operator OperationResult(OperationError error) => new(error);
}
