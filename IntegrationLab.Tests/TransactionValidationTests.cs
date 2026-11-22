using IntegrationLab.Api.Validation;
using IntegrationLab.Api;
using Xunit;

namespace IntegrationLab.Tests;

public class TransactionValidationTests
{
    [Fact]
    public void PositiveAmount_ShouldBeValid()
    {
        decimal amount = 25000;

        bool result = TransactionValidator.IsValidAmount(amount);

        Assert.True(result);
    }
    [Fact]
    public void ZeroAmount_ShouldBeInvalid()
    {
        decimal amount = 0;

        bool result = TransactionValidator.IsValidAmount(amount);

        Assert.False(result);
    }
    [Fact]
    public void NegativeAmount_ShouldBeInvalid()
    {
        decimal amount = -25000;

        bool result = TransactionValidator.IsValidAmount(amount);

        Assert.False(result);
    }
}
