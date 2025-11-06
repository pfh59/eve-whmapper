namespace WHMapper.Models.DTO
{
    /// <summary>
    /// Represents the result of an operation that returns data of type T
    /// </summary>
    /// <typeparam name="T">The type of data returned on success</typeparam>
    public class Result<T>
    {
        public bool IsSuccess { get; private set; }
        public T? Data { get; private set; }
        public string? ErrorMessage { get; private set; }
        public int? StatusCode { get; private set; }
        public Exception? Exception { get; private set; }

        public TimeSpan? RetryAfter { get; private set; }

        private Result(bool isSuccess, T? data, string? errorMessage, int? statusCode, Exception? exception, TimeSpan? retryAfter)
        {
            IsSuccess = isSuccess;
            Data = data;
            ErrorMessage = errorMessage;
            StatusCode = statusCode;
            Exception = exception;
            RetryAfter = retryAfter;
        }

        /// <summary>
        /// Creates a successful result with data
        /// </summary>
        public static Result<T> Success(T data) => new(true, data, null, null, null, null);

        /// <summary>
        /// Creates a failed result with error message and optional status code
        /// </summary>
        public static Result<T> Failure(string errorMessage, int? statusCode = null, Exception? exception = null, TimeSpan? retryAfter = null) 
            => new(false, default, errorMessage, statusCode, exception, retryAfter);

        /// <summary>
        /// Creates a failed result from an exception
        /// </summary>
        public static Result<T> Failure(Exception exception, int? statusCode = null, TimeSpan? retryAfter = null) 
            => new(false, default, exception.Message, statusCode, exception, retryAfter);

        /// <summary>
        /// Implicitly converts a successful value to a Result
        /// </summary>
        public static implicit operator Result<T>(T value) => Success(value);
    }

    /// <summary>
    /// Represents the result of an operation that doesn't return data
    /// </summary>
    public class Result
    {
        public bool IsSuccess { get; private set; }
        public string? ErrorMessage { get; private set; }
        public int? StatusCode { get; private set; }
        public Exception? Exception { get; private set; }
        public TimeSpan? RetryAfter { get; private set; }

        private Result(bool isSuccess, string? errorMessage, int? statusCode, Exception? exception, TimeSpan? retryAfter)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            StatusCode = statusCode;
            Exception = exception;
            RetryAfter = retryAfter;
        }

        /// <summary>
        /// Creates a successful result
        /// </summary>
        public static Result Success() => new(true, null, null, null, null);
        
        /// <summary>
        /// Creates a failed result with error message and optional status code
        /// </summary>
        public static Result Failure(string errorMessage, int? statusCode = null, Exception? exception = null, TimeSpan? retryAfter = null) 
            => new(false, errorMessage, statusCode, exception, retryAfter);

        /// <summary>
        /// Creates a failed result from an exception
        /// </summary>
        public static Result Failure(Exception exception, int? statusCode = null, TimeSpan? retryAfter = null) 
            => new(false, exception.Message, statusCode, exception, retryAfter);
    }
}