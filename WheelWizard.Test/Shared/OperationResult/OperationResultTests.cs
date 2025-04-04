using WheelWizard.Shared;

namespace WheelWizard.Test.Shared.OperationResultTests;

public class OperationResultTests
{
    [Fact(DisplayName = "Create success result, should have correct state")]
    public void CreateSuccessResult_ShouldHaveCorrectState()
    {
        // Act
        var operationResult = OperationResult.Ok();

        // Assert
        Assert.Null(operationResult.Error);
        Assert.False(operationResult.IsFailure);
        Assert.True(operationResult.IsSuccess);
    }

    [Fact(DisplayName = "New success result, should have correct state")]
    public void NewSuccessResult_ShouldHaveCorrectState()
    {
        // Act
        var operationResult = new OperationResult();

        // Assert
        Assert.Null(operationResult.Error);
        Assert.False(operationResult.IsFailure);
        Assert.True(operationResult.IsSuccess);
    }

    [Fact(DisplayName = "New failure result, should have correct state")]
    public void NewFailureResult_ShouldHaveCorrectState()
    {
        // Arrange
        var error = new OperationError { Message = "Error message" };

        // Act
        var operationResult = new OperationResult(error);

        // Assert
        Assert.Equal(operationResult.Error, error);
        Assert.True(operationResult.IsFailure);
        Assert.False(operationResult.IsSuccess);
    }

    [Fact(DisplayName = "Create failure result, should have correct state")]
    public void CreateFailureResult_ShouldHaveCorrectState()
    {
        // Arrange
        var error = new OperationError { Message = "Error message" };

        // Act
        var operationResult = OperationResult.Fail(error);

        // Assert
        Assert.Equal(error, operationResult.Error);
        Assert.True(operationResult.IsFailure);
        Assert.False(operationResult.IsSuccess);
    }

    [Fact(DisplayName = "Implicit result from error, should have correct state")]
    public void ImplicitResultFromError_ShouldHaveCorrectState()
    {
        // Arrange
        var error = new OperationError { Message = "Error message" };

        // Act
        OperationResult operationResult = error;

        // Assert
        Assert.Equal(error, operationResult.Error);
        Assert.True(operationResult.IsFailure);
        Assert.False(operationResult.IsSuccess);
    }

    [Fact(DisplayName = "New success generic result, should have correct state")]
    public void NewSuccessGenericResult_ShouldHaveCorrectState()
    {
        // Arrange
        var value = new object();

        // Act
        var operationResult = new OperationResult<object>(value);

        // Assert
        Assert.Null(operationResult.Error);
        Assert.False(operationResult.IsFailure);
        Assert.True(operationResult.IsSuccess);
        Assert.Equal(value, operationResult.Value);
    }

    [Fact(DisplayName = "Create success generic result, should have correct state")]
    public void CreateSuccessGenericResult_ShouldHaveCorrectState()
    {
        // Arrange
        var value = new object();

        // Act
        var operationResult = OperationResult.Ok(value);

        // Assert
        Assert.Null(operationResult.Error);
        Assert.False(operationResult.IsFailure);
        Assert.True(operationResult.IsSuccess);
        Assert.Equal(value, operationResult.Value);
    }

    [Fact(DisplayName = "New failure generic result, should have correct state")]
    public void NewFailureGenericResult_ShouldHaveCorrectState()
    {
        // Arrange
        var error = new OperationError { Message = "Error message" };

        // Act
        var operationResult = new OperationResult<object>(error);

        // Assert
        Assert.Equal(error, operationResult.Error);
        Assert.True(operationResult.IsFailure);
        Assert.False(operationResult.IsSuccess);
    }

    [Fact(DisplayName = "Create failure generic result, should have correct state")]
    public void CreateFailureGenericResult_ShouldHaveCorrectState()
    {
        // Arrange
        var error = new OperationError { Message = "Error message" };

        // Act
        var operationResult = OperationResult.Fail(error);

        // Assert
        Assert.Equal(error, operationResult.Error);
        Assert.True(operationResult.IsFailure);
        Assert.False(operationResult.IsSuccess);
    }

    [Fact(DisplayName = "Implicit generic result from error, should have correct state")]
    public void ImplicitGenericResultFromError_ShouldHaveCorrectState()
    {
        // Arrange
        var error = new OperationError { Message = "Error message" };

        // Act
        OperationResult<object> operationResult = error;

        // Assert
        Assert.Equal(error, operationResult.Error);
        Assert.True(operationResult.IsFailure);
        Assert.False(operationResult.IsSuccess);
    }

    [Fact(DisplayName = "Implicit generic result from value, should have correct state")]
    public void ImplicitGenericResultFromValue_ShouldHaveCorrectState()
    {
        // Arrange
        var value = new object();

        // Act
        OperationResult<object> operationResult = value;

        // Assert
        Assert.Null(operationResult.Error);
        Assert.False(operationResult.IsFailure);
        Assert.True(operationResult.IsSuccess);
        Assert.Equal(value, operationResult.Value);
    }
    
    [Fact(DisplayName = "Implicit result from string, should have failed state")]
    public void ImplicitResultFromString_ShouldHaveFailedState()
    {
        // Arrange
        const string errorMessage = "Error message";

        // Act
        OperationResult operationResult = errorMessage;

        // Assert
        Assert.NotNull(operationResult.Error);
        Assert.True(operationResult.IsFailure);
        Assert.False(operationResult.IsSuccess);
        Assert.Equal(errorMessage, operationResult.Error?.Message);
    }
    
    [Fact(DisplayName = "Implicit result from exception, should have failed state")]
    public void ImplicitResultFromException_ShouldHaveFailedState()
    {
        // Arrange
        var exception = new Exception("Error message");

        // Act
        OperationResult operationResult = exception;

        // Assert
        Assert.NotNull(operationResult.Error);
        Assert.True(operationResult.IsFailure);
        Assert.False(operationResult.IsSuccess);
        Assert.Equal(exception.Message, operationResult.Error?.Message);
    }

    [Fact(DisplayName = "Implicit generic result from string, should have correct failed state")]
    public void ImplicitGenericResultFromString_ShouldHaveCorrectFailedState()
    {
        // Arrange
        const string errorMessage = "Error message";

        // Act
        OperationResult<object> operationResult = errorMessage;

        // Assert
        Assert.NotNull(operationResult.Error);
        Assert.True(operationResult.IsFailure);
        Assert.False(operationResult.IsSuccess);
        Assert.Equal(errorMessage, operationResult.Error?.Message);
    }

    [Fact(DisplayName = "Implicit generic result from exception, should have correct failed state")]
    public void ImplicitGenericResultFromException_ShouldHaveCorrectFailedState()
    {
        // Arrange
        var exception = new Exception("Error message");

        // Act
        OperationResult<object> operationResult = exception;

        // Assert
        Assert.NotNull(operationResult.Error);
        Assert.True(operationResult.IsFailure);
        Assert.False(operationResult.IsSuccess);
        Assert.Equal(exception.Message, operationResult.Error?.Message);
    }

    [Fact(DisplayName = "Safe execute without exception, should have correct success state")]
    public void SafeExecuteWithoutException_ShouldHaveCorrectSuccessState()
    {
        // Arrange
        int Func() => 42;

        // Act
        var result = OperationResult.SafeExecute(Func);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact(DisplayName = "Safe execute with exception, should have failed state")]
    public void SafeExecuteWithException_ShouldHaveFailedState()
    {
        // Arrange
        var exception = new Exception("Error message");

        int Func() => throw exception;

        // Act
        var result = OperationResult.SafeExecute(Func);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(exception.Message, result.Error?.Message);
        Assert.Equal(exception, result.Error?.Exception);
    }

    [Fact(DisplayName = "Safe execute with exception with override, should have failed state with message")]
    public void SafeExecuteWithExceptionWithOverride_ShouldHaveFailedStateWithMessage()
    {
        // Arrange
        var exception = new Exception("Error message");
        const string errorMessage = "Custom error message";

        int Func() => throw exception;

        // Act
        var result = OperationResult.SafeExecute(Func, errorMessage);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(errorMessage, result.Error?.Message);
        Assert.Equal(exception, result.Error?.Exception);
    }
}
