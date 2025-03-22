namespace WheelWizard.Shared;

public class OperationError
{
    public required string Message { get; init; }

    public Exception? Exception { get; init; }
    
    public static implicit operator OperationError(string message) => new() { Message = message };
}
