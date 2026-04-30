using FinTrack.Backend.IntegrationTests.Support;
using FluentAssertions;
using Reqnroll;
using System.Net;

namespace FinTrack.Backend.IntegrationTests.StepDefinitions;

[Binding]
public class PassosComuns
{
    private readonly TestContext _context;

    public PassosComuns(TestContext context)
    {
        _context = context;
    }

    [Given(@"que eu sou o usuário ""(.*)""")]
    public void DadoQueEuSouOUsuario(string userId)
    {
        _context.UserId = userId;
        _context.ApiClient.DefaultRequestHeaders.Clear();
        _context.ApiClient.DefaultRequestHeaders.Add("X-User-Id", userId);
    }

    [Then(@"a requisição deve ser bem-sucedida")]
    public void EntaoARequisicaoDeveSerBemSucedida()
    {
        _context.LastResponse.Should().NotBeNull("deve haver uma resposta HTTP");
        _context.LastResponse!.IsSuccessStatusCode.Should().BeTrue(
            $"esperava sucesso mas recebeu {_context.LastResponse.StatusCode}");
    }

    [Then(@"a requisição deve falhar com status (.*)")]
    public void EntaoARequisicaoDeveFalharComStatus(int statusCode)
    {
        _context.LastResponse.Should().NotBeNull("deve haver uma resposta HTTP");
        _context.LastResponse!.StatusCode.Should().Be((HttpStatusCode)statusCode);
    }

    [Then(@"a mensagem de erro deve conter ""(.*)""")]
    public async Task EntaoAMensagemDeErroDeveConter(string textoEsperado)
    {
        _context.LastResponse.Should().NotBeNull();
        var content = await _context.LastResponse!.Content.ReadAsStringAsync();
        content.Should().Contain(textoEsperado,
            $"o conteúdo da resposta deve conter '{textoEsperado}'");
    }
}
