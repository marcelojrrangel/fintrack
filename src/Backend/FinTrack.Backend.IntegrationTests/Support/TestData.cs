using FinTrack.Application.Common.Models;
using FinTrack.Domain.Enums;

namespace FinTrack.Backend.IntegrationTests.Support;

public static class TestData
{
    public const string DefaultUserId = "11111111-1111-1111-1111-111111111111";
    public const string SalaryCategoryId = "22222222-2222-2222-2222-222222222221";
    public const string BillsCategoryId = "22222222-2222-2222-2222-222222222222";
    public const string SavingsCategoryId = "22222222-2222-2222-2222-222222222223";

    public static TransactionMutationRequest CreateIncomeRequest(
        decimal amount = 1000m,
        string description = "Receita de Teste",
        string? categoryId = null)
    {
        return new TransactionMutationRequest(
            CategoryId: Guid.Parse(categoryId ?? SalaryCategoryId),
            Amount: amount,
            TransactionDateUtc: DateTime.UtcNow,
            Type: TransactionType.Income,
            Description: description
        );
    }

    public static TransactionMutationRequest CreateExpenseRequest(
        decimal amount = 500m,
        string description = "Despesa de Teste",
        string? categoryId = null)
    {
        return new TransactionMutationRequest(
            CategoryId: Guid.Parse(categoryId ?? BillsCategoryId),
            Amount: amount,
            TransactionDateUtc: DateTime.UtcNow,
            Type: TransactionType.Expense,
            Description: description
        );
    }
}
