using System.Diagnostics.CodeAnalysis;

namespace WheelWizard.Shared;

/// <summary>
/// Represents the result of an operation.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public class OperationResult<T> : OperationResult
{
    [MemberNotNullWhen(true, nameof(Value))]
    public override bool IsSuccess => base.IsSuccess;

    [MemberNotNullWhen(false, nameof(Value))]
    public override bool IsFailure => base.IsFailure;

    /// <summary>
    /// The value of the operation result.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OperationResult{T}"/> class.
    /// </summary>
    /// <param name="value">The value of the operation result.</param>
    public OperationResult(T value)
    {
        Value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OperationResult{T}"/> class with the specified error.
    /// </summary>
    /// <param name="error">The error that occurred during the operation.</param>
    public OperationResult(OperationError error) : base(error)
    {
    }

    public static implicit operator OperationResult<T>(OperationError error) => new(error);

    public static implicit operator OperationResult<T>(T value) => new(value);
}
