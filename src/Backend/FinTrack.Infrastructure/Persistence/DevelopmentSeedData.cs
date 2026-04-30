namespace FinTrack.Infrastructure.Persistence;

internal static class DevelopmentSeedData
{
    public static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid IncomeCategoryId = Guid.Parse("22222222-2222-2222-2222-222222222221");
    public static readonly Guid ExpenseCategoryId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid SavingsCategoryId = Guid.Parse("22222222-2222-2222-2222-222222222223");
    public static readonly DateTime SeededAtUtc = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
}
