using System;
using WHMapper.Models.DTO;
using Xunit;

namespace WHMapper.Tests.Models;

public class ResultTest
{
    #region Result<T> Failure Tests

    [Fact]
    public void ResultGeneric_Failure_WithErrorMessage_CreatesFailedResult()
    {
        var errorMessage = "Operation failed";
        var result = Result<string>.Failure(errorMessage);

        Assert.False(result.IsSuccess);
        Assert.Equal(errorMessage, result.ErrorMessage);
        Assert.Null(result.Data);
        Assert.Null(result.StatusCode);
        Assert.Null(result.Exception);
        Assert.Null(result.RetryAfter);
    }

    [Fact]
    public void ResultGeneric_Failure_WithErrorMessageAndStatusCode_SetsBothProperties()
    {
        var errorMessage = "Bad request";
        var statusCode = 400;
        var result = Result<int>.Failure(errorMessage, statusCode);

        Assert.False(result.IsSuccess);
        Assert.Equal(errorMessage, result.ErrorMessage);
        Assert.Equal(statusCode, result.StatusCode);
        Assert.Null(result.Exception);
    }

    [Fact]
    public void ResultGeneric_Failure_WithErrorMessageAndException_SetsException()
    {
        var errorMessage = "Custom error";
        var exception = new InvalidOperationException("Inner exception");
        var result = Result<object>.Failure(errorMessage, null, exception);

        Assert.False(result.IsSuccess);
        Assert.Equal(errorMessage, result.ErrorMessage);
        Assert.Equal(exception, result.Exception);
        Assert.Null(result.StatusCode);
    }

    [Fact]
    public void ResultGeneric_Failure_WithErrorMessageAndRetryAfter_SetsRetryAfter()
    {
        var errorMessage = "Too many requests";
        var retryAfter = TimeSpan.FromSeconds(30);
        var result = Result<string>.Failure(errorMessage, null, null, retryAfter);

        Assert.False(result.IsSuccess);
        Assert.Equal(errorMessage, result.ErrorMessage);
        Assert.Equal(retryAfter, result.RetryAfter);
    }

    [Fact]
    public void ResultGeneric_Failure_WithAllParameters_SetsAllProperties()
    {
        var errorMessage = "Server error";
        var statusCode = 500;
        var exception = new Exception("Detailed error");
        var retryAfter = TimeSpan.FromMinutes(1);
        var result = Result<bool>.Failure(errorMessage, statusCode, exception, retryAfter);

        Assert.False(result.IsSuccess);
        Assert.Equal(errorMessage, result.ErrorMessage);
        Assert.Equal(statusCode, result.StatusCode);
        Assert.Equal(exception, result.Exception);
        Assert.Equal(retryAfter, result.RetryAfter);
    }

    [Fact]
    public void ResultGeneric_Failure_WithException_CreatesFailedResultFromException()
    {
        var exception = new ArgumentNullException("paramName");
        var result = Result<string>.Failure(exception);

        Assert.False(result.IsSuccess);
        Assert.Equal(exception.Message, result.ErrorMessage);
        Assert.Equal(exception, result.Exception);
        Assert.Null(result.StatusCode);
        Assert.Null(result.RetryAfter);
    }

    [Fact]
    public void ResultGeneric_Failure_WithExceptionAndStatusCode_SetsStatusCode()
    {
        var exception = new TimeoutException("Request timeout");
        var statusCode = 408;
        var result = Result<int>.Failure(exception, statusCode);

        Assert.False(result.IsSuccess);
        Assert.Equal(exception.Message, result.ErrorMessage);
        Assert.Equal(exception, result.Exception);
        Assert.Equal(statusCode, result.StatusCode);
        Assert.Null(result.RetryAfter);
    }

    [Fact]
    public void ResultGeneric_Failure_WithExceptionAndRetryAfter_SetsRetryAfter()
    {
        var exception = new Exception("Rate limited");
        var retryAfter = TimeSpan.FromSeconds(60);
        var result = Result<object>.Failure(exception, null, retryAfter);

        Assert.False(result.IsSuccess);
        Assert.Equal(exception.Message, result.ErrorMessage);
        Assert.Equal(exception, result.Exception);
        Assert.Equal(retryAfter, result.RetryAfter);
    }

    [Fact]
    public void ResultGeneric_Failure_WithExceptionAllParameters_SetsAllProperties()
    {
        var exception = new InvalidOperationException("Operation invalid");
        var statusCode = 422;
        var retryAfter = TimeSpan.FromSeconds(45);
        var result = Result<string>.Failure(exception, statusCode, retryAfter);

        Assert.False(result.IsSuccess);
        Assert.Equal(exception.Message, result.ErrorMessage);
        Assert.Equal(exception, result.Exception);
        Assert.Equal(statusCode, result.StatusCode);
        Assert.Equal(retryAfter, result.RetryAfter);
    }

    #endregion

    #region Result Failure Tests

    [Fact]
    public void Result_Failure_WithErrorMessage_CreatesFailedResult()
    {
        var errorMessage = "Operation failed";
        var result = Result.Failure(errorMessage);

        Assert.False(result.IsSuccess);
        Assert.Equal(errorMessage, result.ErrorMessage);
        Assert.Null(result.StatusCode);
        Assert.Null(result.Exception);
        Assert.Null(result.RetryAfter);
    }

    [Fact]
    public void Result_Failure_WithErrorMessageAndStatusCode_SetsBothProperties()
    {
        var errorMessage = "Not found";
        var statusCode = 404;
        var result = Result.Failure(errorMessage, statusCode);

        Assert.False(result.IsSuccess);
        Assert.Equal(errorMessage, result.ErrorMessage);
        Assert.Equal(statusCode, result.StatusCode);
        Assert.Null(result.Exception);
    }

    [Fact]
    public void Result_Failure_WithErrorMessageAndException_SetsException()
    {
        var errorMessage = "Validation error";
        var exception = new ArgumentException("Invalid argument");
        var result = Result.Failure(errorMessage, null, exception);

        Assert.False(result.IsSuccess);
        Assert.Equal(errorMessage, result.ErrorMessage);
        Assert.Equal(exception, result.Exception);
        Assert.Null(result.StatusCode);
    }

    [Fact]
    public void Result_Failure_WithErrorMessageAndRetryAfter_SetsRetryAfter()
    {
        var errorMessage = "Service unavailable";
        var retryAfter = TimeSpan.FromSeconds(120);
        var result = Result.Failure(errorMessage, null, null, retryAfter);

        Assert.False(result.IsSuccess);
        Assert.Equal(errorMessage, result.ErrorMessage);
        Assert.Equal(retryAfter, result.RetryAfter);
    }

    [Fact]
    public void Result_Failure_WithAllParameters_SetsAllProperties()
    {
        var errorMessage = "Internal server error";
        var statusCode = 500;
        var exception = new Exception("Unexpected error");
        var retryAfter = TimeSpan.FromSeconds(300);
        var result = Result.Failure(errorMessage, statusCode, exception, retryAfter);

        Assert.False(result.IsSuccess);
        Assert.Equal(errorMessage, result.ErrorMessage);
        Assert.Equal(statusCode, result.StatusCode);
        Assert.Equal(exception, result.Exception);
        Assert.Equal(retryAfter, result.RetryAfter);
    }

    [Fact]
    public void Result_Failure_WithException_CreatesFailedResultFromException()
    {
        var exception = new InvalidOperationException("Cannot perform operation");
        var result = Result.Failure(exception);

        Assert.False(result.IsSuccess);
        Assert.Equal(exception.Message, result.ErrorMessage);
        Assert.Equal(exception, result.Exception);
        Assert.Null(result.StatusCode);
        Assert.Null(result.RetryAfter);
    }

    [Fact]
    public void Result_Failure_WithExceptionAndStatusCode_SetsStatusCode()
    {
        var exception = new TimeoutException("Connection timeout");
        var statusCode = 504;
        var result = Result.Failure(exception, statusCode);

        Assert.False(result.IsSuccess);
        Assert.Equal(exception.Message, result.ErrorMessage);
        Assert.Equal(exception, result.Exception);
        Assert.Equal(statusCode, result.StatusCode);
    }

    [Fact]
    public void Result_Failure_WithExceptionAndRetryAfter_SetsRetryAfter()
    {
        var exception = new Exception("Too many requests");
        var retryAfter = TimeSpan.FromSeconds(90);
        var result = Result.Failure(exception, null, retryAfter);

        Assert.False(result.IsSuccess);
        Assert.Equal(exception.Message, result.ErrorMessage);
        Assert.Equal(exception, result.Exception);
        Assert.Equal(retryAfter, result.RetryAfter);
    }

    [Fact]
    public void Result_Failure_WithExceptionAllParameters_SetsAllProperties()
    {
        var exception = new Exception("Resource conflict");
        var statusCode = 409;
        var retryAfter = TimeSpan.FromSeconds(30);
        var result = Result.Failure(exception, statusCode, retryAfter);

        Assert.False(result.IsSuccess);
        Assert.Equal(exception.Message, result.ErrorMessage);
        Assert.Equal(exception, result.Exception);
        Assert.Equal(statusCode, result.StatusCode);
        Assert.Equal(retryAfter, result.RetryAfter);
    }

    #endregion
}