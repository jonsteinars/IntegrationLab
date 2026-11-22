using IntegrationLab.Api.Data;
using IntegrationLab.Api.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using IntegrationLab.Api.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContextFactory<IntegrationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Postgres")));

var app = builder.Build();

app.MapGet("/health", () =>
{
    return Results.Ok(new
    {
        Status = "Healthy",
        Service = "IntegrationLab"
    });
});

app.MapGet("/transactions/{id:int}",
    async (int id, IntegrationDbContext db) =>
{
    // Finndu færsluna hér með FindAsync(id)
    var transaction = await db.Transactions.FindAsync(id);
    // var transaction = await db.Transactions.FirstOrDefaultAsync(id);
    // Ef hún er null -> NotFound
    if (transaction == null)
    {
        return Results.NotFound(new
        {
            error = $"Transaction with ID {id} not found."
        });
    }
    // Annars -> Ok
    return Results.Ok(transaction);
});

// Dependency Injection:
app.MapPost("/transactions",
    async (
        CreateTransactionRequest request, 
        IntegrationDbContext db, 
        IDbContextFactory<IntegrationDbContext> dbFactory) =>
{
    // 1. Staðfesta gögnin
    if (string.IsNullOrWhiteSpace(request.ProviderTransactionId))
    {
        return Results.BadRequest(new
        {
            error = "ProviderTransactionId is required."
        });
    }

    if (!TransactionValidator.IsValidAmount(request.Amount))
    {
        return Results.BadRequest(new
        {
            error = "Amount must be greater than zero."
        });
    }

    // 2. Leita fyrst að fyrirliggjandi færslu
    var existing = await db.Transactions
        .AsNoTracking()
        .FirstOrDefaultAsync(t =>
            t.ProviderTransactionId == request.ProviderTransactionId);

    if (existing is not null)
    {
        return CompareTransactions(existing, request);
    }

    // 3. Búa til nýja færslu
    var transaction = new Transaction
    {
        ProviderTransactionId = request.ProviderTransactionId,
        Amount = request.Amount,
        Currency = request.Currency,
        PaymentStatus = request.PaymentStatus
    };

    db.Transactions.Add(transaction);

    try
    {
        await db.SaveChangesAsync();
    }

    catch (DbUpdateException ex)
        when (ex.InnerException is PostgresException
        { 
            SqlState: PostgresErrorCodes.UniqueViolation, 
            ConstraintName: "IX_Transactions_ProviderTransactionId" 
        })
    {
            // 4. Önnur beiðni gæti hafa vistað færsluna
            // eftir að við leituðum að henni í skrefi 2.
            await using var readDb = 
                await dbFactory.CreateDbContextAsync();

            var saved = await readDb.Transactions
                .AsNoTracking()
                    .FirstOrDefaultAsync(t => 
                        t.ProviderTransactionId == request.ProviderTransactionId);

            if (saved is null)
            {
                return Results.Problem(
                    "Could not verify the existing transaction.");
            }

            return CompareTransactions(saved, request);
    }

    // 5. Ný færsla var vistuð
    return Results.Created(
        $"/transactions/{transaction.Id}",
        transaction);
});

app.Run();

static IResult CompareTransactions(
    Transaction existing,
    CreateTransactionRequest request)
{
    if (existing.Amount == request.Amount
        && existing.Currency == request.Currency
        && existing.PaymentStatus == request.PaymentStatus)
    {
        return Results.Ok(existing);
    }

    return Results.Conflict(new
    {
        error = $"Transaction '{request.ProviderTransactionId}' " +
                "already exists with different data."
    });
}

public record CreateTransactionRequest(
    string ProviderTransactionId,
    decimal Amount,
    string Currency,
    string PaymentStatus
);