using FinTrack.Application.Common.Models;
using FinTrack.Backend.IntegrationTests.Support;
using FinTrack.Domain.Enums;
using Reqnroll;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace FinTrack.Backend.IntegrationTests.StepDefinitions;

[Binding]
public class ValidacoesSteps
{
    private readonly TestContext _context;

    public ValidacoesSteps(TestContext context)
    {
        _context = context;
    }

    [When(@"eu tento criar uma transação com tipo inválido ""(.*)""")]
    public async Task QuandoEuTentoCriarUmaTransacaoComTipoInvalido(string tipoInvalido)
    {
        var payload = new
        {
            categoryId = TestData.BillsCategoryId,
            amount = 100,
            transactionDateUtc = DateTime.UtcNow,
            type = int.TryParse(tipoInvalido, out var typeValue) ? typeValue : 999,
            description = "Teste de validação"
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _context.LastResponse = await _context.ApiClient.PostAsync("/api/transactions", content);
    }

    [When(@"eu tento criar uma transação com valor (.*)")]
    public async Task QuandoEuTentoCriarUmaTransacaoComValor(decimal valor)
    {
        var request = new TransactionMutationRequest(
            CategoryId: Guid.Parse(TestData.BillsCategoryId),
            Amount: valor,
            TransactionDateUtc: DateTime.UtcNow,
            Type: TransactionType.Expense,
            Description: "Teste de validação"
        );

        _context.LastResponse = await _context.ApiClient.PostAsJsonAsync("/api/transactions", request);
    }

    [When(@"eu tento criar uma transação com descrição vazia")]
    public async Task QuandoEuTentoCriarUmaTransacaoComDescricaoVazia()
    {
        var request = new TransactionMutationRequest(
            CategoryId: Guid.Parse(TestData.BillsCategoryId),
            Amount: 100m,
            TransactionDateUtc: DateTime.UtcNow,
            Type: TransactionType.Expense,
            Description: string.Empty
        );

        _context.LastResponse = await _context.ApiClient.PostAsJsonAsync("/api/transactions", request);
    }

    [When(@"eu tento criar uma transação com descrição de (.*) caracteres")]
    public async Task QuandoEuTentoCriarUmaTransacaoComDescricaoDe(int tamanho)
    {
        var descricao = new string('A', tamanho);

        var request = new TransactionMutationRequest(
            CategoryId: Guid.Parse(TestData.BillsCategoryId),
            Amount: 100m,
            TransactionDateUtc: DateTime.UtcNow,
            Type: TransactionType.Expense,
            Description: descricao
        );

        _context.LastResponse = await _context.ApiClient.PostAsJsonAsync("/api/transactions", request);
    }

    [When(@"eu tento criar uma transação com categoria ""(.*)""")]
    public async Task QuandoEuTentoCriarUmaTransacaoComCategoria(string categoriaId)
    {
        var request = new TransactionMutationRequest(
            CategoryId: Guid.Parse(categoriaId),
            Amount: 100m,
            TransactionDateUtc: DateTime.UtcNow,
            Type: TransactionType.Expense,
            Description: "Teste de validação"
        );

        _context.LastResponse = await _context.ApiClient.PostAsJsonAsync("/api/transactions", request);
    }

    [When(@"eu tento criar uma transação com data padrão")]
    public async Task QuandoEuTentoCriarUmaTransacaoComDataPadrao()
    {
        var request = new TransactionMutationRequest(
            CategoryId: Guid.Parse(TestData.BillsCategoryId),
            Amount: 100m,
            TransactionDateUtc: default,
            Type: TransactionType.Expense,
            Description: "Teste de validação"
        );

        _context.LastResponse = await _context.ApiClient.PostAsJsonAsync("/api/transactions", request);
    }
}
