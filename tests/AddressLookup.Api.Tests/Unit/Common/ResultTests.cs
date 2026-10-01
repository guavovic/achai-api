using AddressLookup.Api.Common.Results;

namespace AddressLookup.Api.Tests.Unit.Common;

public class ResultTests
{
    private static readonly Error AnyError = Error.NotFound("Teste.NaoEncontrado", "Não encontrado.");

    [Fact]
    public void Success_HoldsTheValue()
    {
        Result<int> result = 42;

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(42);
        result.Error.ShouldBeNull();
    }

    [Fact]
    public void Failure_HoldsTheError()
    {
        Result<int> result = AnyError;

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(AnyError);
    }

    [Fact]
    public void Value_WhenFailed_Throws()
    {
        Result<int> result = AnyError;

        Should.Throw<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Map_WhenSucceeded_TransformsTheValue()
    {
        Result<int> result = 21;

        var mapped = result.Map(value => value * 2);

        mapped.Value.ShouldBe(42);
    }

    [Fact]
    public void Map_WhenFailed_KeepsTheErrorWithoutCallingTheFunction()
    {
        Result<int> result = AnyError;
        var called = false;

        var mapped = result.Map(value =>
        {
            called = true;
            return value * 2;
        });

        called.ShouldBeFalse();
        mapped.Error.ShouldBe(AnyError);
    }

    [Fact]
    public void Match_CallsTheRightBranch()
    {
        Result<int> success = 1;
        Result<int> failure = AnyError;

        success.Match(_ => "success", _ => "failure").ShouldBe("success");
        failure.Match(_ => "success", error => error.Code).ShouldBe("Teste.NaoEncontrado");
    }
}
