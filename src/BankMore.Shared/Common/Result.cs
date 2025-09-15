namespace BankMore.Shared.Common;

public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T Value { get; }
    public string Error { get; }
    public string ErrorType { get; }

    private Result(bool isSuccess, T value, string error, string errorType)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        ErrorType = errorType;
    }

    public static Result<T> Success(T value) => new(true, value, string.Empty, string.Empty);
    public static Result<T> Failure(string error, string errorType) => new(false, default(T), error, errorType);
}

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }
    public string ErrorType { get; }

    private Result(bool isSuccess, string error, string errorType)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorType = errorType;
    }

    public static Result Success() => new(true, string.Empty, string.Empty);
    public static Result Failure(string error, string errorType) => new(false, error, errorType);
}
