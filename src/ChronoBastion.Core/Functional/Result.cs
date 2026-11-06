namespace ChronoBastion.Core.Functional;

public abstract record Result<T, TError>
{
    private Result() { }

    public sealed record Success(T Value) : Result<T, TError>;
    public sealed record Failure(TError Error) : Result<T, TError>;

    public bool IsSuccess => this is Success;
    public static Result<T, TError> Ok(T value) => new Success(value);
    public static Result<T, TError> Fail(TError error) => new Failure(error);

    public TResult Match<TResult>(Func<T, TResult> success, Func<TError, TResult> failure) =>
        this switch
        {
            Success result => success(result.Value),
            Failure result => failure(result.Error),
            _ => throw new InvalidOperationException("Unknown result variant.")
        };

    public Result<TResult, TError> Map<TResult>(Func<T, TResult> map) =>
        Match(value => Result<TResult, TError>.Ok(map(value)), Result<TResult, TError>.Fail);

    public Result<TResult, TError> Bind<TResult>(Func<T, Result<TResult, TError>> bind) =>
        Match(bind, Result<TResult, TError>.Fail);
}
