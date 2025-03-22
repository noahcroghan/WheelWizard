using WheelWizard.Shared;

namespace WheelWizard.Test;

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

    [Fact(DisplayName = "Implicit result from true, should have correct state")]
    public void ImplicitResultFromTrue_ShouldHaveCorrectState()
    {
        // Act
        OperationResult operationResult = true;

        // Assert
        Assert.Null(operationResult.Error);
        Assert.False(operationResult.IsFailure);
        Assert.True(operationResult.IsSuccess);
    }

    [Fact(DisplayName = "Implicit result from false, should throw exception")]
    public void ImplicitResultFromFalse_ShouldThrowException()
    {
        // Assert
        Assert.Throws<InvalidOperationException>(() =>
        {
            // Act
            OperationResult _ = false;
        });
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
}
