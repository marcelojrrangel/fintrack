# FinTrack Backend

WebAPI em **ASP.NET Core 8** seguindo **Clean Architecture**, **CQRS com MediatR**, **Entity Framework Core** e **Oracle** para desenvolvimento local.

## Estrutura

```text
src\Backend
├─ FinTrack.Domain
├─ FinTrack.Application
├─ FinTrack.Infrastructure
├─ FinTrack.WebAPI
├─ FinTrack.Backend.UnitTests  (Testes Unitários)
└─ FinTrack.Backend.IntegrationTests  (Testes de Integração BDD)
```

## Tecnologias

- .NET 8 / ASP.NET Core WebAPI
- MediatR
- FluentValidation
- Entity Framework Core
- Oracle Database
- Swagger / OpenAPI

## Banco Oracle local com Docker Compose

Suba o banco:

```bash
docker compose up -d
```

Serviço criado:

- **Container**: `fintrack-oracle`
- **Porta Oracle**: `1521`
- **Porta Oracle EM Express**: `5500`
- **Usuário app**: `fintrack`
- **Senha app**: `fintrack123`
- **Service name**: `FREEPDB1`

Connection string usada pela API:

```text
User Id=fintrack;Password=fintrack123;Data Source=localhost:1521/FREEPDB1;
```

### Decisão de design: IDs como VARCHAR2(36)

Os IDs (Guid) são armazenados como **VARCHAR2(36)** em vez de RAW(16) por:

- ✅ **Visualização legível** em ferramentas SQL (DBeaver, SQL Developer)
- ✅ **Facilidade de debugging** - copiar/colar IDs diretamente
- ✅ **Compatibilidade com APIs REST** - JSON usa strings naturalmente
- ✅ **Portabilidade** - mesmo formato em PostgreSQL, SQL Server, MySQL

Exemplo de ID armazenado: `11111111-1111-1111-1111-111111111111`

## Executando a API

1. Suba o Oracle:

   ```bash
   docker compose up -d
   ```

2. Rode a API:

   ```bash
   dotnet run --project .\src\Backend\FinTrack.WebAPI\FinTrack.WebAPI.csproj
   ```

3. Abra o Swagger:

   ```text
   https://localhost:7xxx/swagger
   ```

Na inicialização, a API cria o schema automaticamente com `EnsureCreated`.

### Recriando o banco de dados

Se precisar limpar e recriar o banco (por exemplo, após mudanças no schema):

```bash
docker compose down -v
docker compose up -d
```

**Aguarde ~30 segundos** para o Oracle inicializar completamente antes de rodar a API.

## Executando os testes

O backend possui uma suíte de 68 testes unitários e de integração organizados por camada:

```text
src\Backend\FinTrack.Backend.UnitTests
├── Application     - Testes de handlers, validators e behaviors
├── Domain          - Testes de entidades e guards
├── Infrastructure  - Testes de DbContext e inicializadores
├── WebAPI          - Testes de controllers, middleware e integração
└── Testing         - Helpers e factories para testes
```

### Tecnologias de teste utilizadas

- **xUnit** - Framework de testes
- **FluentAssertions** - Assertions fluentes e legíveis
- **NSubstitute** - Mocking e substituição de dependências
- **Microsoft.AspNetCore.Mvc.Testing** - Testes de integração de APIs
- **EntityFrameworkCore.InMemory** - Banco em memória para testes
- **Coverlet** - Coleta de cobertura de código

### Rodando todos os testes da solução

```bash
dotnet test .\FinTrack.slnx --nologo
```

### Rodando apenas o projeto de testes do backend

```bash
dotnet test .\src\Backend\FinTrack.Backend.UnitTests\FinTrack.Backend.UnitTests.csproj --nologo
```

### Rodando testes específicos por categoria

```bash
# Apenas testes de Domain
dotnet test --filter "FullyQualifiedName~Domain" --nologo

# Apenas testes de Application
dotnet test --filter "FullyQualifiedName~Application" --nologo

# Apenas testes de WebAPI
dotnet test --filter "FullyQualifiedName~WebAPI" --nologo
```

## Gerando relatório de cobertura

Para gerar o relatório Cobertura:

```bash
dotnet test .\src\Backend\FinTrack.Backend.UnitTests\FinTrack.Backend.UnitTests.csproj --nologo /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=TestResults\coverage\ "/p:Include=[FinTrack.*]*"
```

O arquivo gerado fica em:

```text
src\Backend\FinTrack.Backend.UnitTests\TestResults\coverage\coverage.cobertura.xml
```

## Visualizando os relatórios

### Resumo direto no XML

Abra o arquivo `coverage.cobertura.xml` em qualquer editor. O nó raiz mostra o resumo geral:

```xml
<coverage line-rate="1" branch-rate="..." lines-covered="874" lines-valid="874" />
```

### Inspecionando pelo terminal

Você pode consultar rapidamente o resumo no PowerShell com:

```powershell
Select-String '<coverage line-rate=' .\src\Backend\FinTrack.Backend.UnitTests\TestResults\coverage\coverage.cobertura.xml
```

### Visualização em ferramentas compatíveis com Cobertura

O arquivo `coverage.cobertura.xml` também pode ser consumido por:

- IDEs e extensões que leem Cobertura
- pipelines CI/CD
- visualizadores externos de cobertura compatíveis com o formato Cobertura

## Dados seed para desenvolvimento

Para permitir testes imediatos sem endpoints adicionais de usuário/categoria, a aplicação sobe com:

- **UserId**: `11111111-1111-1111-1111-111111111111`
- **Category Salary**: `22222222-2222-2222-2222-222222222221`
- **Category Bills**: `22222222-2222-2222-2222-222222222222`
- **Category Savings**: `22222222-2222-2222-2222-222222222223`

Envie sempre o header abaixo nas rotas de dashboard e transações:

```text
X-User-Id: 11111111-1111-1111-1111-111111111111
```

## Endpoints

### Dashboard

- `GET /api/dashboard`

Retorna:

- `currentBalance`
- `totalIncomeMonth`
- `totalExpenseMonth`
- `cardColor` (`red` quando saldo < 0, senão `green`)

### Transações

- `GET /api/transactions`
- `GET /api/transactions/{id}`
- `POST /api/transactions`
- `PUT /api/transactions/{id}`
- `DELETE /api/transactions/{id}` *(soft delete para preservar auditoria)*
- `GET /api/transactions/{id}/history`

### Exemplo de payload para criar/atualizar transação

```json
{
  "categoryId": "22222222-2222-2222-2222-222222222221",
  "amount": 3500.0,
  "transactionDateUtc": "2026-04-30T00:00:00Z",
  "type": "Income",
  "description": "April salary"
}
```

## Respostas padronizadas

Sucesso:

```json
{
  "success": true,
  "message": "Transaction created successfully.",
  "data": {},
  "errors": []
}
```

Erro:

```json
{
  "success": false,
  "message": "Validation failed.",
  "data": null,
  "errors": [
    "Description must not be empty."
  ]
}
```

## Regras implementadas

- Dashboard calculado dinamicamente a partir das transações ativas.
- Entradas e saídas do mês filtradas pelo mês corrente em UTC.
- Auditoria em `TRANSACTION_HISTORY` para criação, atualização e exclusão.
- Exclusão lógica em transações para preservar histórico.
- Validação com FluentValidation.
- Injeção de dependência em todas as camadas.

---

## Testes de Integração (Reqnroll/BDD)

O projeto inclui testes de integração **end-to-end** escritos em **Gherkin (Português do Brasil)** usando **Reqnroll**, cobrindo todos os cenários de negócio da aplicação.

### 📂 Estrutura dos testes

```text
src\Backend\FinTrack.Backend.IntegrationTests
├── Features\               - Cenários em Gherkin (pt-BR)
│   ├── Dashboard.feature
│   ├── Transacoes.feature
│   └── Validacoes.feature
├── StepDefinitions\        - Implementação dos steps
│   ├── PassosComuns.cs
│   ├── DashboardSteps.cs
│   ├── TransacoesSteps.cs
│   └── ValidacoesSteps.cs
├── Hooks\                  - Setup/Teardown de testes
│   └── TestHooks.cs
├── Support\                - Helpers e contexto compartilhado
│   ├── TestContext.cs
│   └── TestData.cs
└── reqnroll.json           - Configuração do Reqnroll
```

### ✅ Funcionalidades cobertas

**Dashboard:**
- ✅ Visualizar dashboard sem transações
- ✅ Visualizar dashboard com transações
- ✅ Dashboard não contabiliza transações excluídas

**Transações:**
- ✅ Criar transação de receita e despesa
- ✅ Atualizar transação existente
- ✅ Excluir transação (soft delete)
- ✅ Listar todas as transações
- ✅ Buscar transação por ID
- ✅ Consultar histórico de alterações

**Validações:**
- ✅ Validar tipo de transação inválido (0, 999, etc.)
- ✅ Validar valores negativos e zero
- ✅ Validar descrição vazia ou muito longa (> 250 caracteres)
- ✅ Validar categoria inexistente
- ✅ Validar data padrão
- ✅ CHECK constraint do banco de dados

### 🐳 Banco de dados isolado

Os testes utilizam **Testcontainers** para criar um Oracle isolado automaticamente:

- ✅ Container Oracle criado antes de cada execução de teste
- ✅ Banco limpo entre cenários (isolamento completo)
- ✅ Container descartado após os testes
- ✅ **Não é necessário** subir manualmente o banco de desenvolvimento

**Porta utilizada:** `1522` (diferente do ambiente de desenvolvimento que usa `1521`)

### 🚀 Executando os testes de integração

```bash
# Todos os testes de integração
dotnet test src\Backend\FinTrack.Backend.IntegrationTests

# Com relatório detalhado
dotnet test src\Backend\FinTrack.Backend.IntegrationTests --logger "console;verbosity=detailed"

# Filtrar por feature específica
dotnet test src\Backend\FinTrack.Backend.IntegrationTests --filter "FullyQualifiedName~Dashboard"
```

### 📝 Exemplo de cenário de teste

```gherkin
Cenário: Criar uma transação de receita
  Quando eu crio uma transação com os seguintes dados:
    | Campo     | Valor                                |
    | Tipo      | Income                               |
    | Categoria | 22222222-2222-2222-2222-222222222221 |
    | Valor     | 5000.00                              |
    | Data      | 2026-01-15                           |
    | Descrição | Salário de Janeiro                   |
  Então a transação deve ser criada com sucesso
  E a resposta deve conter o ID da transação
  E o tipo deve ser "Income"
  E o valor deve ser 5000.00
```

### 📊 Relatórios de execução

Os relatórios HTML gerados pelo Reqnroll ficam em:

```text
src\Backend\FinTrack.Backend.IntegrationTests\TestResults\
```

### 🔧 Tecnologias utilizadas

- **Reqnroll 2.2.0** - Framework BDD/Gherkin para .NET
- **xUnit** - Execution engine
- **FluentAssertions** - Assertions expressivas
- **Testcontainers.Oracle 4.0.0** - Containers Docker para testes
- **Microsoft.AspNetCore.Mvc.Testing** - Testing de APIs
- **Bogus** - Geração de dados fake

### 💡 Vantagens dos testes BDD

✅ **Documentação viva** - Os cenários em português documentam o comportamento esperado do sistema

✅ **Compreensível por não-técnicos** - Product Owners e stakeholders podem ler e validar os cenários

✅ **Cobertura end-to-end** - Testa toda a stack: API → Application → Infrastructure → Database Oracle

✅ **Isolamento completo** - Cada cenário roda em um banco limpo

✅ **CI/CD ready** - Pode rodar em pipelines sem dependências externas

✅ **Rastreabilidade** - Vincula requisitos de negócio aos testes automatizados

### ⚠️ Requisitos

Para executar os testes de integração, você precisa ter:

- **Docker** instalado e rodando (para Testcontainers)
- Memória suficiente para rodar o container Oracle (mínimo 2GB recomendado)

**Nota:** O primeiro run pode demorar alguns minutos pois o Docker precisa baixar a imagem do Oracle (`gvenzl/oracle-free:23-slim`). Runs subsequentes serão mais rápidos.

