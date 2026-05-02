using System.Text.Json.Serialization;

namespace FinTrack.Application.Common.Models;

public sealed record DashboardDto(
    [property: JsonPropertyName("currentBalance")] decimal CurrentBalance,
    [property: JsonPropertyName("totalIncomeMonth")] decimal TotalIncomeMonth,
    [property: JsonPropertyName("totalExpenseMonth")] decimal TotalExpenseMonth,
    [property: JsonPropertyName("cardColor")] string CardColor);
