namespace RetailCoreEcommerce.Contracts.Shared;
using System.Text.Json.Serialization;

public class Result
{
    public Result(bool isSuccess, Error? error)
    {
        if (isSuccess && error != null)
        {
            throw new InvalidOperationException();
        }

        if (!isSuccess && error == Error.None)
        {
            throw new InvalidOperationException();
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error? Error { get; }

    public static Result Success() => new(true, null);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, null);

    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);

    public static Result<TValue> Failure<TValue>(TValue value, Error error) => new(value, false, error);

    public static implicit operator Result(Error error) => Failure(error);

    protected static Result<TValue> Create<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    public Result(TValue? value, bool isSuccess, Error? error)
        : base(isSuccess, error) =>
        _value = value;

    [JsonIgnore]
    public TValue? Value => IsSuccess
        ? _value!
        : default;

    // For API response compatibility
    public TValue? Data => IsSuccess
        ? _value!
        : default;

    [JsonIgnore]
    public TValue? GetFailureValue => IsFailure
        ? _value!
        : default;

    public static implicit operator Result<TValue>(TValue? value) => Create(value);
    public static implicit operator Result<TValue>(Error error) => Failure<TValue>(error);

    /// <summary>
    ///     Converts a Result to a Result&lt;TValue&gt; with the same error.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public static Result<TValue> FromResult(Result result)
    {
        return Failure<TValue>(result.Error!);
    }
}