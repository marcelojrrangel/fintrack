namespace FinTrack.Application.Common.Models;

public sealed record DashboardDto(
    decimal CurrentBalance,
    decimal TotalIncomeMonth,
    decimal TotalExpenseMonth,
    string CardColor);
