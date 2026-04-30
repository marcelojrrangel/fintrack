# FinTrack - Testes de Integração (Reqnroll/BDD)

## 📋 Visão Geral

Este documento descreve a implementação completa dos **testes de integração end-to-end** do projeto FinTrack utilizando **Reqnroll** (sucessor do SpecFlow) com **Gherkin em Português do Brasil**.

## 🎯 Objetivos

- ✅ Cobertura completa de todas as funcionalidades da API
- ✅ Testes legíveis por stakeholders não-técnicos
- ✅ Documentação viva do comportamento do sistema
- ✅ Isolamento completo com banco de dados Oracle dedicado
- ✅ Execução automatizada em pipelines CI/CD

## 🏗️ Arquitetura dos Testes

### Estrutura de Pastas

```
src/Backend/FinTrack.Backend.IntegrationTests/
├── Features/                    # Cenários Gherkin em pt-BR
│   ├── Dashboard.feature        # Testes do dashboard financeiro
│   ├── Transacoes.feature       # CRUD completo de transações
│   └── Validacoes.feature       # Validações e regras de negócio
│
├── StepDefinitions/             # Implementação dos steps
│   ├── PassosComuns.cs          # Steps compartilhados entre features
│   ├── DashboardSteps.cs        # Steps específicos do dashboard
│   ├── TransacoesSteps.cs       # Steps de CRUD de transações
│   └── ValidacoesSteps.cs       # Steps de validações
│
├── Hooks/                       # Setup e Teardown
│   └── TestHooks.cs             # Lifecycle dos testes e container Oracle
│
├── Support/                     # Classes auxiliares
│   ├── TestContext.cs           # Contexto compartilhado entre steps
│   └── TestData.cs              # Dados de teste e helpers
│
├── reqnroll.json                # Configuração do Reqnroll
└── FinTrack.Backend.IntegrationTests.csproj
```

## 📦 Dependências Principais

| Pacote | Versão | Propósito |
|--------|--------|-----------|
| Reqnroll | 2.2.0 | Framework BDD/Gherkin |
| Reqnroll.xUnit | 2.2.0 | Integração com xUnit |
| Testcontainers.Oracle | 4.0.0 | Container Oracle automático |
| Microsoft.AspNetCore.Mvc.Testing | 8.0.11 | Testing de APIs ASP.NET Core |
| FluentAssertions | 6.12.2 | Assertions expressivas |
| Bogus | 35.6.1 | Geração de dados fake |

## 🐳 Banco de Dados Isolado (Testcontainers)

### Como Funciona

1. **BeforeTestRun**: Um container Oracle é iniciado uma vez antes de todos os testes
2. **BeforeScenario**: O banco é recriado (EnsureDeleted + EnsureCreated) para cada cenário
3. **AfterScenario**: Contexto e recursos são limpos
4. **AfterTestRun**: Container Oracle é parado e removido

### Configuração do Container

```csharp
_oracleContainer = new OracleBuilder()
    .WithImage("gvenzl/oracle-free:23-slim")
    .WithPassword("TestPassword123")
    .WithPortBinding(1522, 1521)  // Porta diferente do dev
    .WithWaitStrategy(...)
    .Build();
```

### Vantagens

✅ **Isolamento total**: Cada cenário roda em um banco limpo  
✅ **Sem configuração manual**: Container é criado automaticamente  
✅ **CI/CD ready**: Funciona em qualquer ambiente com Docker  
✅ **Seed data automático**: EF Core aplica os dados de desenvolvimento  

## 📝 Cenários Implementados

### 1. Dashboard (3 cenários)

#### Cenário 1: Dashboard sem transações
```gherkin
Dado que eu sou o usuário "11111111-1111-1111-1111-111111111111"
Quando eu consulta o dashboard
Então o total de receitas deve ser 0.00
E o total de despesas deve ser 0.00
E o balanço deve ser 0.00
```

#### Cenário 2: Dashboard com transações
- Cria 3 transações (1 receita, 2 despesas)
- Valida os totais corretos

#### Cenário 3: Transações excluídas não são contabilizadas
- Cria transações
- Exclui uma delas
- Valida que não aparece no dashboard

### 2. Transações (7 cenários)

1. **Criar transação de receita**
   - Valida criação bem-sucedida
   - Verifica ID, tipo e valor

2. **Criar transação de despesa**
   - Valida criação
   - Verifica registro de histórico com ação "Created"

3. **Atualizar transação**
   - Atualiza valor e descrição
   - Valida mudanças

4. **Excluir transação**
   - Soft delete
   - Valida que não aparece na listagem

5. **Listar transações**
   - Lista todas as transações
   - Verifica presença das transações criadas

6. **Buscar por ID**
   - Busca transação específica
   - Valida retorno

7. **Consultar histórico**
   - Cria e atualiza transação 2 vezes
   - Valida 3 registros de histórico (Created + 2 Updated)

### 3. Validações (7 cenários)

1. **Tipo inválido** (Esquema do Cenário com exemplos: 0, 999)
2. **Valor negativo**
3. **Valor zero**
4. **Descrição vazia**
5. **Descrição muito longa** (> 250 caracteres)
6. **Categoria inexistente**
7. **Data padrão** (default DateTime)

Todos validam:
- Status HTTP 400 (Bad Request) ou 404 (Not Found)
- Mensagem de erro apropriada

## 🧪 Executando os Testes

### Pré-requisitos

- Docker instalado e rodando
- Memória suficiente para Oracle (mínimo 2GB)

### Comandos

```bash
# Todos os testes de integração
dotnet test \Backend\FinTrack.Backend.IntegrationTests

# Com verbosidade detalhada
dotnet test \Backend\FinTrack.Backend.IntegrationTests --logger "console;verbosity=detailed"

# Filtrar por feature
dotnet test \Backend\FinTrack.Backend.IntegrationTests --filter "FullyQualifiedName~Dashboard"
dotnet test \Backend\FinTrack.Backend.IntegrationTests --filter "FullyQualifiedName~Transacoes"
dotnet test \Backend\FinTrack.Backend.IntegrationTests --filter "FullyQualifiedName~Validacoes"
```

### Primeira Execução

⚠️ **A primeira execução será mais lenta** (pode levar 2-5 minutos) pois:
1. Docker precisa baixar a imagem `gvenzl/oracle-free:23-slim` (~1.2GB)
2. Oracle precisa inicializar (~30 segundos)

Execuções subsequentes serão muito mais rápidas (~30-60 segundos).

## 📊 Exemplo de Saída

```
🚀 Iniciando container Oracle para testes de integração...
⏳ Aguardando Oracle inicializar completamente...
✅ Container Oracle iniciado com sucesso!
📊 Connection String: Data Source=localhost:1522/FREEPDB1;...

🧪 Iniciando cenário: Visualizar dashboard sem transações
✅ Banco de dados criado com sucesso
🏁 Finalizando cenário: Visualizar dashboard sem transações

🧪 Iniciando cenário: Criar uma transação de receita
✅ Banco de dados criado com sucesso
🏁 Finalizando cenário: Criar uma transação de receita

...

✅ 17 cenários executados (17 passou, 0 falhou)
🛑 Parando container Oracle...
✅ Container Oracle finalizado
```

## 🎓 Padrões e Boas Práticas Implementadas

### 1. Given-When-Then Pattern

Todos os cenários seguem a estrutura:
- **Dado** (Given): Pré-condições e estado inicial
- **Quando** (When): Ação executada
- **Então** (Then): Resultado esperado

### 2. Context Injection

```csharp
public DashboardSteps(TestContext context, ScenarioContext scenarioContext)
{
    _context = context;
    _scenarioContext = scenarioContext;
}
```

### 3. Assertions Expressivas (FluentAssertions)

```csharp
_dashboardData.Should().NotBeNull("os dados do dashboard devem estar disponíveis");
_dashboardData!.CurrentBalance.Should().Be(3000.00);
```

### 4. Isolamento de Dados

Cada cenário:
- Recria o banco do zero
- Não depende de estado compartilhado
- Pode rodar em qualquer ordem

### 5. Nomenclatura em Português

- Features em português do Brasil
- Steps com nomes descritivos
- Mensagens de erro claras

## 🔄 Workflow de Desenvolvimento

### Adicionar Novo Cenário

1. **Escrever o cenário em Gherkin** (`Features/*.feature`)
2. **Compilar** - Reqnroll gera a classe parcial automaticamente
3. **Implementar os steps** faltantes em `StepDefinitions/`
4. **Executar e validar**

### Exemplo: Adicionar novo cenário

```gherkin
# Features/Transacoes.feature

Cenário: Restaurar transação excluída
  Dado que existe uma transação excluída
  Quando eu restauro a transação
  Então a transação deve estar ativa novamente
  E um registro de histórico com ação "Restored" deve ser criado
```

Implementar:

```csharp
// StepDefinitions/TransacoesSteps.cs

[When(@"eu restauro a transação")]
public async Task QuandoEuRestauroATransacao()
{
    // Implementação...
}
```

## 🚀 Integração CI/CD

### GitHub Actions Exemplo

```yaml
name: Integration Tests

on: [push, pull_request]

jobs:
  integration-tests:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v3

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build --no-restore

    - name: Run Integration Tests
      run: dotnet test src/Backend/FinTrack.Backend.IntegrationTests --no-build --verbosity normal
```

O Testcontainers usa automaticamente o Docker disponível no runner.

## 📈 Cobertura Atual

| Feature | Cenários | Status |
|---------|----------|--------|
| Dashboard | 3 | ✅ Implementado |
| Transações | 7 | ✅ Implementado |
| Validações | 7 | ✅ Implementado |
| **TOTAL** | **17** | ✅ **100%** |

## 🎯 Próximos Passos (Futuro)

- [ ] Testes de performance com carga
- [ ] Testes de concorrência (múltiplos usuários)
- [ ] Testes de segurança (injeção SQL, XSS)
- [ ] Relatórios HTML com Living Documentation
- [ ] Integração com ferramentas de análise de qualidade (SonarQube)

## 📚 Referências

- [Reqnroll Documentation](https://reqnroll.net/)
- [Gherkin Syntax](https://cucumber.io/docs/gherkin/)
- [Testcontainers .NET](https://dotnet.testcontainers.org/)
- [FluentAssertions](https://fluentassertions.com/)

---

**Implementado por:** Sistema FinTrack  
**Data:** Abril 2026  
**Versão:** 1.0.0
