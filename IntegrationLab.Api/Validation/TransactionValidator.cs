namespace IntegrationLab.Api.Validation;

public static class TransactionValidator
{
    public static bool IsValidAmount(decimal amount)
    {
        return amount > 0;
    }
}
