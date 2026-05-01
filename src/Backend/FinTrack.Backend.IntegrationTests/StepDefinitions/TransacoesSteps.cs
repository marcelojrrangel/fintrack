using FinTrack.Application.Common.Models;
using FinTrack.Backend.IntegrationTests.Support;
using FinTrack.Domain.Enums;
using FluentAssertions;
using Reqnroll;
using System.Net;
using System.Net.Http.Json;

namespace FinTrack.Backend.IntegrationTests.StepDefinitions;

[Binding]
public class TransacoesSteps
{
    private readonly TestContext _context;
    private TransactionDto? _lastTransaction;
    private List<TransactionDto>? _transactionList;
    private List<TransactionHistoryDto>? _historyList;

    public TransacoesSteps(TestContext context)
    {
        _context = context;
    }

    [Given(@"que existem as seguintes transações:")]
    public async Task DadoQueExistemAsSeguintesTransacoes(Table table)
    {
        foreach (var row in table.Rows)
        {
            var tipo = Enum.Parse<TransactionType>(row["Tipo"]);
            var categoriaId = table.ContainsColumn("Categoria") ? MapearCategoria(row["Categoria"]) : "22222222-2222-2222-2222-222222222221";
            var valor = decimal.Parse(row["Valor"]);
            var data = table.ContainsColumn("Data") ? DateTime.Parse(row["Data"]) : DateTime.UtcNow;
            var descricao = row["Descrição"];

            var request = new TransactionMutationRequest(
                CategoryId: Guid.Parse(categoriaId),
                Amount: valor,
                TransactionDateUtc: data,
                Type: tipo,
                Description: descricao
            );

            var response = await _context.ApiClient.PostAsJsonAsync("/api/transactions", request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<TransactionDto>>();
            _context.StoreTransactionId(descricao, result!.Data!.Id.ToString());
        }
    }

    [Given(@"que existe uma transação de despesa no valor de (.*) com descrição ""(.*)""")]
    public async Task DadoQueExisteUmaTransacaoDeDespesaNoValorComDescricao(decimal valor, string descricao)
    {
        var request = TestData.CreateExpenseRequest(valor, descricao);
        var response = await _context.ApiClient.PostAsJsonAsync("/api/transactions", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<TransactionDto>>();
        _context.StoreTransactionId(descricao, result!.Data!.Id.ToString());
        _lastTransaction = result.Data;
    }

    [Given(@"que existe uma transação com descrição ""(.*)""")]
    public async Task DadoQueExisteUmaTransacaoComDescricao(string descricao)
    {
        var request = TestData.CreateIncomeRequest(1000m, descricao);
        var response = await _context.ApiClient.PostAsJsonAsync("/api/transactions", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<TransactionDto>>();
        _context.StoreTransactionId(descricao, result!.Data!.Id.ToString());
        _lastTransaction = result.Data;
    }

    [Given(@"a transação ""(.*)"" foi excluída")]
    public async Task DadoATransacaoFoiExcluida(string descricao)
    {
        var transactionId = _context.GetTransactionId(descricao);
        var response = await _context.ApiClient.DeleteAsync($"/api/transactions/{transactionId}");
        response.EnsureSuccessStatusCode();
    }

    [Given(@"a transação foi atualizada com descrição ""(.*)""")]
    public async Task DadoATransacaoFoiAtualizadaComDescricao(string novaDescricao)
    {
        _lastTransaction.Should().NotBeNull();

        var request = new TransactionMutationRequest(
            CategoryId: _lastTransaction!.CategoryId,
            Amount: _lastTransaction.Amount,
            TransactionDateUtc: _lastTransaction.TransactionDateUtc,
            Type: _lastTransaction.Type,
            Description: novaDescricao
        );

        var response = await _context.ApiClient.PutAsJsonAsync(
            $"/api/transactions/{_lastTransaction.Id}", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<ApiResponse<TransactionDto>>();
        _lastTransaction = result!.Data;
    }

    [When(@"eu crio uma transação com os seguintes dados:")]
    public async Task QuandoEuCrioUmaTransacaoComOsSeguintesDados(Table table)
    {
        var tipo = Enum.Parse<TransactionType>(table.Rows[0]["Valor"]);
        var categoriaId = Guid.Parse(table.Rows[1]["Valor"]);
        var valor = decimal.Parse(table.Rows[2]["Valor"]);
        var data = DateTime.Parse(table.Rows[3]["Valor"]);
        var descricao = table.Rows[4]["Valor"];

        var request = new TransactionMutationRequest(
            CategoryId: categoriaId,
            Amount: valor,
            TransactionDateUtc: data,
            Type: tipo,
            Description: descricao
        );

        _context.LastResponse = await _context.ApiClient.PostAsJsonAsync("/api/transactions", request);

        if (_context.LastResponse.IsSuccessStatusCode)
        {
            var result = await _context.LastResponse.Content
                .ReadFromJsonAsync<ApiResponse<TransactionDto>>();
            _lastTransaction = result?.Data;
            if (_lastTransaction != null)
            {
                _context.StoreTransactionId(descricao, _lastTransaction.Id.ToString());
            }
        }
    }

    [When(@"eu atualizo o valor para (.*)")]
    public async Task QuandoEuAtualizoOValorPara(decimal novoValor)
    {
        _lastTransaction.Should().NotBeNull();

        var request = new TransactionMutationRequest(
            CategoryId: _lastTransaction!.CategoryId,
            Amount: novoValor,
            TransactionDateUtc: _lastTransaction.TransactionDateUtc,
            Type: _lastTransaction.Type,
            Description: _lastTransaction.Description
        );

        _context.LastResponse = await _context.ApiClient.PutAsJsonAsync(
            $"/api/transactions/{_lastTransaction.Id}", request);

        if (_context.LastResponse.IsSuccessStatusCode)
        {
            var result = await _context.LastResponse.Content
                .ReadFromJsonAsync<ApiResponse<TransactionDto>>();
            _lastTransaction = result?.Data;
        }
    }

    [When(@"eu atualizo a descrição para ""(.*)""")]
    public async Task QuandoEuAtualizoADescricaoPara(string novaDescricao)
    {
        _lastTransaction.Should().NotBeNull();

        var request = new TransactionMutationRequest(
            CategoryId: _lastTransaction!.CategoryId,
            Amount: _lastTransaction.Amount,
            TransactionDateUtc: _lastTransaction.TransactionDateUtc,
            Type: _lastTransaction.Type,
            Description: novaDescricao
        );

        _context.LastResponse = await _context.ApiClient.PutAsJsonAsync(
            $"/api/transactions/{_lastTransaction.Id}", request);

        if (_context.LastResponse.IsSuccessStatusCode)
        {
            var result = await _context.LastResponse.Content
                .ReadFromJsonAsync<ApiResponse<TransactionDto>>();
            _lastTransaction = result?.Data;
        }
    }

    [When(@"eu excluo a transação")]
    public async Task QuandoEuExcluoATransacao()
    {
        _lastTransaction.Should().NotBeNull();
        _context.LastResponse = await _context.ApiClient.DeleteAsync(
            $"/api/transactions/{_lastTransaction!.Id}");
    }

    [When(@"eu listo todas as transações")]
    public async Task QuandoEuListoTodasAsTransacoes()
    {
        _context.LastResponse = await _context.ApiClient.GetAsync("/api/transactions");

            if (_context.LastResponse.IsSuccessStatusCode)
            {
                var result = await _context.LastResponse.ReadPagedApiResponseAsync<TransactionDto>();
                _transactionList = result?.Data?.Items?.ToList() ?? new List<TransactionDto>();
            }
    }

    [When(@"eu busco a transação pelo ID")]
    public async Task QuandoEuBuscoATransacaoPeloID()
    {
        _lastTransaction.Should().NotBeNull();
        _context.LastResponse = await _context.ApiClient.GetAsync(
            $"/api/transactions/{_lastTransaction!.Id}");

        if (_context.LastResponse.IsSuccessStatusCode)
        {
            var result = await _context.LastResponse.Content
                .ReadFromJsonAsync<ApiResponse<TransactionDto>>();
            _lastTransaction = result?.Data;
        }
    }

    [When(@"eu consulto o histórico da transação")]
    public async Task QuandoEuConsultoOHistoricoDaTransacao()
    {
        _lastTransaction.Should().NotBeNull();
        _context.LastResponse = await _context.ApiClient.GetAsync(
            $"/api/transactions/{_lastTransaction!.Id}/history");

        if (_context.LastResponse.IsSuccessStatusCode)
        {
            var result = await _context.LastResponse.Content
                .ReadFromJsonAsync<ApiResponse<IReadOnlyCollection<TransactionHistoryDto>>>();
            _historyList = result?.Data?.ToList();
        }
    }

    [Then(@"a transação deve ser criada com sucesso")]
    public void EntaoATransacaoDeveSerCriadaComSucesso()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.StatusCode.Should().Be(HttpStatusCode.Created);
        _lastTransaction.Should().NotBeNull();
    }

    [Then(@"a transação deve ser atualizada com sucesso")]
    public void EntaoATransacaoDeveSerAtualizadaComSucesso()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.StatusCode.Should().Be(HttpStatusCode.OK);
        _lastTransaction.Should().NotBeNull();
    }

    [Then(@"a transação deve ser excluída com sucesso")]
    public void EntaoATransacaoDeveSerExcluidaComSucesso()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Then(@"a resposta deve conter o ID da transação")]
    public void EntaoARespostaDeveConterOIDDaTransacao()
    {
        _lastTransaction.Should().NotBeNull();
        _lastTransaction!.Id.Should().NotBeEmpty();
    }

    [Then(@"o tipo deve ser ""(.*)""")]
    public void EntaoOTipoDeveSer(string tipoEsperado)
    {
        _lastTransaction.Should().NotBeNull();
        var tipoEnum = Enum.Parse<TransactionType>(tipoEsperado);
        _lastTransaction!.Type.Should().Be(tipoEnum);
    }

    [Then(@"o valor deve ser (.*)")]
    public void EntaoOValorDeveSer(decimal valorEsperado)
    {
        _lastTransaction.Should().NotBeNull();
        _lastTransaction!.Amount.Should().Be(valorEsperado);
    }

    [Then(@"a descrição deve ser ""(.*)""")]
    public void EntaoADescricaoDeveSer(string descricaoEsperada)
    {
        _lastTransaction.Should().NotBeNull();
        _lastTransaction!.Description.Should().Be(descricaoEsperada);
    }

    [Then(@"um registro de histórico deve ser criado com ação ""(.*)""")]
    public async Task EntaoUmRegistroDeHistoricoDeveSerCriadoComAcao(string acaoEsperada)
    {
        _lastTransaction.Should().NotBeNull();

        var historyResponse = await _context.ApiClient.GetAsync(
            $"/api/transactions/{_lastTransaction!.Id}/history");
        historyResponse.IsSuccessStatusCode.Should().BeTrue();

        var historyResult = await historyResponse.Content
            .ReadFromJsonAsync<ApiResponse<IReadOnlyCollection<TransactionHistoryDto>>>();

        historyResult.Should().NotBeNull();
        historyResult!.Data.Should().NotBeNull();

        var expectedAction = Enum.Parse<FinTrack.Domain.Enums.HistoryActionType>(acaoEsperada);
        historyResult.Data.Should().Contain(h => h.Action == expectedAction);
    }

    [Then(@"a transação não deve aparecer na listagem de transações")]
    public async Task EntaoATransacaoNaoDeveAparecerNaListagemDeTransacoes()
    {
        _lastTransaction.Should().NotBeNull();

            var listResponse = await _context.ApiClient.GetAsync("/api/transactions");
            listResponse.IsSuccessStatusCode.Should().BeTrue();

            var result = await listResponse.ReadPagedApiResponseAsync<TransactionDto>();
            result.Should().NotBeNull("a resposta da listagem não deve ser nula");
            var items = result!.Data?.Items ?? new List<TransactionDto>();
            items.Should().NotContain(t => t.Id == _lastTransaction!.Id);
    }

    [Then(@"devo ver (.*) transações na lista")]
    public void EntaoDevoVerTransacoesNaLista(int quantidadeEsperada)
    {
        _transactionList.Should().NotBeNull();
        _transactionList!.Count.Should().Be(quantidadeEsperada);
    }

    [Then(@"a transação ""(.*)"" deve estar na lista")]
    public void EntaoATransacaoDeveEstarNaLista(string descricao)
    {
        _transactionList.Should().NotBeNull();
        _transactionList!.Should().Contain(t => t.Description == descricao);
    }

    [Then(@"a transação deve ser retornada")]
    public void EntaoATransacaoDeveSerRetornada()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.StatusCode.Should().Be(HttpStatusCode.OK);
        _lastTransaction.Should().NotBeNull();
    }

    [Then(@"devo ver (.*) registros de histórico")]
    public void EntaoDevoVerRegistrosDeHistorico(int quantidadeEsperada)
    {
        _historyList.Should().NotBeNull();
        _historyList!.Count.Should().Be(quantidadeEsperada);
    }

    [Then(@"o primeiro registro deve ter ação ""(.*)""")]
    public void EntaoOPrimeiroRegistroDeveTerAcao(string acaoEsperada)
    {
        _historyList.Should().NotBeNull();
        _historyList!.Should().NotBeEmpty();

        var expectedAction = Enum.Parse<FinTrack.Domain.Enums.HistoryActionType>(acaoEsperada);
        _historyList.First().Action.Should().Be(expectedAction);
    }

    [Then(@"os próximos (.*) registros devem ter ação ""(.*)""")]
    public void EntaoOsProximosRegistrosDevemTerAcao(int quantidade, string acaoEsperada)
    {
        _historyList.Should().NotBeNull();
        var nextRecords = _historyList!.Skip(1).Take(quantidade).ToList();
        nextRecords.Should().HaveCount(quantidade);

        var expectedAction = Enum.Parse<FinTrack.Domain.Enums.HistoryActionType>(acaoEsperada);
        nextRecords.Should().OnlyContain(h => h.Action == expectedAction);
    }

    private string MapearCategoria(string nomeCategoria)
    {
        return nomeCategoria switch
        {
            "Salary" => TestData.SalaryCategoryId,
            "Bills" => TestData.BillsCategoryId,
            "Savings" => TestData.SavingsCategoryId,
            _ => throw new ArgumentException($"Categoria desconhecida: {nomeCategoria}")
        };
    }
}
