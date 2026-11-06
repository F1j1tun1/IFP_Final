namespace ChronoBastion.Core.Functional;

public readonly record struct Option<T> where T : notnull
{
    private readonly T? value;

    private Option(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        this.value = value;
        IsSome = true;
    }

    public bool IsSome { get; }
    public static Option<T> None => default;
    public static Option<T> Some(T value) => new(value);

    public TResult Match<TResult>(Func<T, TResult> some, Func<TResult> none) =>
        IsSome ? some(value!) : none();

    public Option<TResult> Map<TResult>(Func<T, TResult> map) where TResult : notnull =>
        IsSome ? Option<TResult>.Some(map(value!)) : Option<TResult>.None;

    public Option<TResult> Bind<TResult>(Func<T, Option<TResult>> bind) where TResult : notnull =>
        IsSome ? bind(value!) : Option<TResult>.None;
}
