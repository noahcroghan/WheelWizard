using System.Diagnostics.CodeAnalysis;

namespace WheelWizard.Shared;

public class OperationResult<T> : OperationResult
{
    [MemberNotNullWhen(true, nameof(Value))]
    public override bool IsSuccess => base.IsSuccess;

    [MemberNotNullWhen(false, nameof(Value))]
    public override bool IsFailure => base.IsFailure;

    public T? Value { get; }

    public OperationResult(T value)
    {
        Value = value;
    }

    public OperationResult(OperationError error) : base(error)
    {
    }

    public static implicit operator OperationResult<T>(OperationError error) => new(error);

    public static implicit operator OperationResult<T>(T value) => new(value);
}
