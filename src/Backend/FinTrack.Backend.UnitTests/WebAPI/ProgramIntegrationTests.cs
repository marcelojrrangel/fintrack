using FinTrack.Application.Common.Models;
using FinTrack.Backend.UnitTests.Testing;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace FinTrack.Backend.UnitTests.WebAPI;

public sealed class ProgramIntegrationTests : IClassFixture<FinTrackWebApplicationFactory>
{
    private readonly FinTrackWebApplicationFactory _factory;

    public ProgramIntegrationTests(FinTrackWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Application_ShouldServeHealthEndpoint()
    {
        using var client = _factory.CreateClient(new()
        {
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.GetAsync("/api/health");
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();

        response.IsSuccessStatusCode.Should().BeTrue();
        payload!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task CreateTransaction_ShouldRejectInvalidTransactionType()
    {
        using var client = _factory.CreateClient(new()
        {
            BaseAddress = new Uri("https://localhost")
        });

        client.DefaultRequestHeaders.Add("X-User-Id", "11111111-1111-1111-1111-111111111111");

        var invalidPayload = new
        {
            categoryId = "22222222-2222-2222-2222-222222222222",
            amount = 150,
            transactionDateUtc = "2026-04-30T13:07:43.374Z",
            type = 0, // Invalid type
            description = "Test"
        };

        var content = new StringContent(JsonSerializer.Serialize(invalidPayload), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/transactions", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("Type");
    }
}

public sealed class DevelopmentProgramIntegrationTests : IClassFixture<DevelopmentFinTrackWebApplicationFactory>
{
    private readonly DevelopmentFinTrackWebApplicationFactory _factory;

    public DevelopmentProgramIntegrationTests(DevelopmentFinTrackWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Application_ShouldExposeSwaggerInDevelopment()
    {
        using var client = _factory.CreateClient(new()
        {
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();

        response.IsSuccessStatusCode.Should().BeTrue();
        content.Should().Contain("X-User-Id");
    }
}
