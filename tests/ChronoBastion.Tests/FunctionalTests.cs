using ChronoBastion.Core.Functional;

namespace ChronoBastion.Tests;

public sealed class FunctionalTests
{
    [Fact]
    public void A_failed_result_short_circuits_the_remaining_pipeline()
    {
        var called = false;
        var result = Result<int, string>.Fail("No gold")
            .Bind(value => { called = true; return Result<int, string>.Ok(value + 1); })
            .Map(value => value * 2);
        Assert.False(called);
        Assert.Equal("No gold", Assert.IsType<Result<int, string>.Failure>(result).Error);
    }

    [Fact]
    public void Result_map_and_bind_preserve_success_values()
    {
        var result = Result<int, string>.Ok(3).Map(value => value * 2)
            .Bind(value => Result<string, string>.Ok(value.ToString()));
        Assert.Equal("6", Assert.IsType<Result<string, string>.Success>(result).Value);
    }

    [Fact]
    public void Option_does_not_call_a_function_for_none()
    {
        var option = Option<int>.None.Map<int>(_ => throw new Exception("Must not run"));
        Assert.False(option.IsSome);
        Assert.Equal(8, Option<int>.Some(4).Map(x => x * 2).Match(x => x, () => -1));
        Assert.Equal(5, Option<int>.Some(4).Bind(x => Option<int>.Some(x + 1)).Match(x => x, () => -1));
        Assert.False(Option<int>.None.Bind(x => Option<int>.Some(x)).IsSome);
    }
}
