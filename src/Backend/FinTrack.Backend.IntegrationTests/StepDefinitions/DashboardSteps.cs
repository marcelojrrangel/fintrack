using FinTrack.Application.Common.Models;
using FinTrack.Backend.IntegrationTests.Support;
using FluentAssertions;
using Reqnroll;
using System.Net.Http.Json;

namespace FinTrack.Backend.IntegrationTests.StepDefinitions;

[Binding]
public class DashboardSteps
{
    private readonly TestContext _context;
    private DashboardDto? _dashboardData;

    public DashboardSteps(TestContext context)
    {
        _context = context;
    }

    [When(@"eu consulto o dashboard")]
    public async Task QuandoEuConsultoODashboard()
    {
        _context.LastResponse = await _context.ApiClient.GetAsync("/api/dashboard");

        if (_context.LastResponse.IsSuccessStatusCode)
        {
            var result = await _context.LastResponse.Content
                .ReadFromJsonAsync<ApiResponse<DashboardDto>>();
            _dashboardData = result?.Data;
        }
    }

    [Then(@"o total de receitas deve ser (.*)")]
    public void EntaoOTotalDeReceitasDeveSer(decimal valorEsperado)
    {
        _dashboardData.Should().NotBeNull("os dados do dashboard devem estar disponíveis");
        _dashboardData!.TotalIncomeMonth.Should().Be(valorEsperado,
            $"o total de receitas deve ser {valorEsperado}");
    }

    [Then(@"o total de despesas deve ser (.*)")]
    public void EntaoOTotalDeDespesasDeveSer(decimal valorEsperado)
    {
        _dashboardData.Should().NotBeNull("os dados do dashboard devem estar disponíveis");
        _dashboardData!.TotalExpenseMonth.Should().Be(valorEsperado,
            $"o total de despesas deve ser {valorEsperado}");
    }

    [Then(@"o balanço deve ser (.*)")]
    public void EntaoOBalancoDeveSer(decimal valorEsperado)
    {
        _dashboardData.Should().NotBeNull("os dados do dashboard devem estar disponíveis");
        _dashboardData!.CurrentBalance.Should().Be(valorEsperado,
            $"o balanço deve ser {valorEsperado}");
    }
}
