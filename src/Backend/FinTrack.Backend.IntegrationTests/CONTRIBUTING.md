# Guia de Contribuição - Testes de Integração

## 🎯 Como Adicionar Novos Testes

Este guia mostra como adicionar novos cenários de teste ao projeto.

## Passo 1: Escrever o Cenário em Gherkin

Abra ou crie um arquivo `.feature` em `Features/` e escreva o cenário em português:

```gherkin
# language: pt-BR

Funcionalidade: Nova Funcionalidade
  Como um usuário
  Eu quero fazer algo
  Para alcançar um objetivo

  Contexto:
    Dado que eu sou o usuário "11111111-1111-1111-1111-111111111111"

  Cenário: Fazer algo com sucesso
    Dado que existe uma pré-condição
    Quando eu executo uma ação
    Então o resultado esperado acontece
    E uma validação adicional é feita
```

## Passo 2: Compilar o Projeto

O Reqnroll gera automaticamente os esqueletos dos steps:

```bash
dotnet build tests\Backend\FinTrack.Backend.IntegrationTests
```

Você verá avisos indicando quais steps precisam ser implementados:

```
warning: No matching step definition found for one or more steps.
```

## Passo 3: Implementar os Steps

Crie ou edite um arquivo em `StepDefinitions/`:

```csharp
using FinTrack.Backend.IntegrationTests.Support;
using FluentAssertions;
using Reqnroll;

namespace FinTrack.Backend.IntegrationTests.StepDefinitions;

[Binding]
public class MinhaFeatureSteps
{
    private readonly TestContext _context;

    public MinhaFeatureSteps(TestContext context)
    {
        _context = context;
    }

    [Given(@"que existe uma pré-condição")]
    public void DadoQueExisteUmaPreCondicao()
    {
        // Setup inicial
    }

    [When(@"eu executo uma ação")]
    public async Task QuandoEuExecutoUmaAcao()
    {
        _context.LastResponse = await _context.ApiClient.PostAsync(...);
    }

    [Then(@"o resultado esperado acontece")]
    public void EntaoOResultadoEsperadoAcontece()
    {
        _context.LastResponse.Should().NotBeNull();
        _context.LastResponse!.IsSuccessStatusCode.Should().BeTrue();
    }
}
```

## Passo 4: Executar e Validar

```bash
dotnet test tests\Backend\FinTrack.Backend.IntegrationTests --filter "FullyQualifiedName~MinhaFeature"
```

## 📋 Checklist de Qualidade

Antes de commitar seu teste, verifique:

- [ ] O cenário está escrito em português claro e objetivo
- [ ] O cenário segue o padrão Given-When-Then
- [ ] Os steps são reutilizáveis quando possível
- [ ] As assertions são expressivas com FluentAssertions
- [ ] O teste passa consistentemente (rode 3x para garantir)
- [ ] O teste limpa seus próprios dados (isolamento)
- [ ] Mensagens de erro são descritivas

## 🎨 Padrões de Nomenclatura

### Steps (Regex)

```csharp
// BOM ✅
[Given(@"que existe uma transação com descrição ""(.*)""")]
[When(@"eu atualizo o valor para (.*)")]
[Then(@"devo ver (.*) transações na lista")]

// RUIM ❌
[Given(@"transacao_existe")]  // Não usar snake_case
[When(@"AtualizarValor")]      // Não usar PascalCase sem regex
```

### Variáveis

```csharp
// BOM ✅
private readonly TestContext _context;
private TransactionDto? _lastTransaction;
private List<TransactionDto>? _transactionList;

// RUIM ❌
private readonly TestContext context;  // Falta underscore
private TransactionDto trans;           // Nome abreviado
```

## 🔧 Helpers Disponíveis

### TestContext

```csharp
_context.ApiClient              // HttpClient configurado com X-User-Id
_context.UserId                 // ID do usuário atual
_context.LastResponse           // Última resposta HTTP
_context.TransactionIds         // Dicionário de IDs de transações
_context.StoreTransactionId()   // Armazenar ID para uso posterior
_context.GetTransactionId()     // Recuperar ID armazenado
```

### TestData

```csharp
TestData.DefaultUserId          // "11111111-1111-1111-1111-111111111111"
TestData.SalaryCategoryId       // "22222222-2222-2222-2222-222222222221"
TestData.BillsCategoryId        // "22222222-2222-2222-2222-222222222222"
TestData.SavingsCategoryId      // "22222222-2222-2222-2222-222222222223"
TestData.CreateIncomeRequest()  // Helper para criar receita
TestData.CreateExpenseRequest() // Helper para criar despesa
```

## 📊 Exemplos de Patterns Comuns

### 1. Criar Recurso e Armazenar ID

```csharp
[Given(@"que existe uma transação com descrição ""(.*)""")]
public async Task DadoQueExisteUmaTransacao(string descricao)
{
    var request = TestData.CreateIncomeRequest(1000m, descricao);
    var response = await _context.ApiClient.PostAsJsonAsync("/api/transactions", request);
    response.EnsureSuccessStatusCode();

    var result = await response.Content.ReadFromJsonAsync<ApiResponse<TransactionDto>>();
    _context.StoreTransactionId(descricao, result!.Data!.Id.ToString());
    _lastTransaction = result.Data;
}
```

### 2. Validar Lista de Resultados

```csharp
[Then(@"devo ver (.*) transações na lista")]
public void EntaoDevoVerTransacoesNaLista(int quantidade)
{
    _transactionList.Should().NotBeNull();
    _transactionList!.Count.Should().Be(quantidade);
}

[Then(@"a transação ""(.*)"" deve estar na lista")]
public void EntaoATransacaoDeveEstarNaLista(string descricao)
{
    _transactionList.Should().NotBeNull();
    _transactionList!.Should().Contain(t => t.Description == descricao);
}
```

### 3. Validar Erros

```csharp
[Then(@"a requisição deve falhar com status (.*)")]
public void EntaoARequisicaoDeveFalhar(int statusCode)
{
    _context.LastResponse.Should().NotBeNull();
    _context.LastResponse!.StatusCode.Should().Be((HttpStatusCode)statusCode);
}

[Then(@"a mensagem de erro deve conter ""(.*)""")]
public async Task EntaoAMensagemDeErroDeveConter(string texto)
{
    var content = await _context.LastResponse!.Content.ReadAsStringAsync();
    content.Should().Contain(texto);
}
```

### 4. Usar Tabelas (Table)

```gherkin
Dado que existem as seguintes transações:
  | Tipo    | Valor | Descrição |
  | Income  | 3000  | Salário   |
  | Expense | 500   | Aluguel   |
```

```csharp
[Given(@"que existem as seguintes transações:")]
public async Task DadoQueExistemAsSeguintesTransacoes(Table table)
{
    foreach (var row in table.Rows)
    {
        var tipo = Enum.Parse<TransactionType>(row["Tipo"]);
        var valor = decimal.Parse(row["Valor"]);
        var descricao = row["Descrição"];

        // Criar transação...
    }
}
```

### 5. Esquema do Cenário (Scenario Outline)

```gherkin
Esquema do Cenário: Validar tipo inválido
  Quando eu tento criar uma transação com tipo "<Tipo>"
  Então a requisição deve falhar com status 400

  Exemplos:
    | Tipo |
    | 0    |
    | 999  |
```

## 🐛 Troubleshooting

### Problema: Container Oracle não inicia

**Solução:**
```bash
# Verificar se Docker está rodando
docker ps

# Limpar containers antigos
docker container prune -f

# Verificar logs do container
docker logs fintrack-oracle-test
```

### Problema: Teste falha intermitentemente

**Causas comuns:**
1. Oracle ainda não terminou de inicializar (aumentar delay no Hook)
2. Dados não foram limpos entre cenários
3. Race condition (use `await` corretamente)

### Problema: Step não é encontrado

**Solução:**
1. Verificar se o regex está correto
2. Recompilar o projeto
3. Verificar se o step está em uma classe com `[Binding]`

## 📚 Recursos Adicionais

- [Gherkin Best Practices](https://cucumber.io/docs/bdd/better-gherkin/)
- [Reqnroll Documentation](https://reqnroll.net/docs/)
- [FluentAssertions Tips](https://fluentassertions.com/tips/)

## ✅ Template Completo

```gherkin
# language: pt-BR

Funcionalidade: [Nome da Funcionalidade]
  Como um [tipo de usuário]
  Eu quero [fazer algo]
  Para [alcançar objetivo]

  Contexto:
    Dado que eu sou o usuário "11111111-1111-1111-1111-111111111111"

  Cenário: [Descrição do cenário]
    Dado [pré-condição]
    Quando [ação]
    Então [resultado]
    E [validação adicional]

  Esquema do Cenário: [Descrição com exemplos]
    Quando [ação com "<parâmetro>"]
    Então [resultado com "<outro>"]

    Exemplos:
      | parâmetro | outro |
      | valor1    | valor1 |
      | valor2    | valor2 |
```

```csharp
using FinTrack.Backend.IntegrationTests.Support;
using FluentAssertions;
using Reqnroll;
using System.Net.Http.Json;

namespace FinTrack.Backend.IntegrationTests.StepDefinitions;

[Binding]
public class MinhaFeatureSteps
{
    private readonly TestContext _context;
    private object? _resultado;

    public MinhaFeatureSteps(TestContext context)
    {
        _context = context;
    }

    [Given(@"pré-condição")]
    public void DadoPreCondicao()
    {
        // Setup
    }

    [When(@"ação")]
    public async Task QuandoAcao()
    {
        _context.LastResponse = await _context.ApiClient.GetAsync("/api/endpoint");
    }

    [Then(@"resultado")]
    public void EntaoResultado()
    {
        _context.LastResponse!.IsSuccessStatusCode.Should().BeTrue();
    }
}
```

---

Dúvidas? Abra uma issue ou consulte a documentação completa no `README.md`.
