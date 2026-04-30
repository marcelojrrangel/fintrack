# 🏗️ FinTrack - Backend

Backend do sistema FinTrack desenvolvido em **.NET 8** seguindo os princípios de **Clean Architecture** e **CQRS**.

---

## 📂 Estrutura da Solução

```
src\Backend\
├── FinTrack.Domain\                      # Camada de Domínio
│   ├── Entities\                         # Entidades do domínio
│   │   ├── BaseEntity.cs                 # Classe base com Id, CreatedAt, etc.
│   │   ├── Transaction.cs                # Entidade de transação financeira
│   │   ├── TransactionHistory.cs         # Histórico de alterações
│   │   ├── Category.cs                   # Categoria de transação
│   │   └── User.cs                       # Usuário do sistema
│   ├── Enums\                            # Enumerações
│   │   ├── TransactionType.cs            # Tipo: Income (1), Expense (2)
│   │   └── HistoryActionType.cs          # Created, Updated, Deleted
│   └── FinTrack.Domain.csproj
│
├── FinTrack.Application\                 # Camada de Aplicação
│   ├── Common\                           # Componentes compartilhados
│   │   ├── Interfaces\                   # Contratos/Interfaces
│   │   │   └── IFinTrackDbContext.cs     # Interface do DbContext
│   │   ├── Models\                       # DTOs e Requests
│   │   │   ├── DashboardDto.cs           # Dashboard financeiro
│   │   │   ├── TransactionDto.cs         # DTO de transação
│   │   │   ├── TransactionHistoryDto.cs  # DTO de histórico
│   │   │   └── TransactionMutationRequest.cs  # Request de criação/atualização
│   │   └── Validators\                   # Validações FluentValidation
│   │       └── TransactionCommandValidator.cs  # Valida Type, Amount, Description, etc.
│   ├── Transactions\                     # Feature: Transações
│   │   ├── Commands\                     # Comandos (Write)
│   │   │   ├── CreateTransaction\
│   │   │   │   ├── CreateTransactionCommand.cs
│   │   │   │   └── CreateTransactionCommandHandler.cs
│   │   │   ├── UpdateTransaction\
│   │   │   │   ├── UpdateTransactionCommand.cs
│   │   │   │   └── UpdateTransactionCommandHandler.cs
│   │   │   └── DeleteTransaction\
│   │   │       ├── DeleteTransactionCommand.cs
│   │   │       └── DeleteTransactionCommandHandler.cs
│   │   └── Queries\                      # Queries (Read)
│   │       ├── GetAllTransactions\
│   │       │   ├── GetAllTransactionsQuery.cs
│   │       │   └── GetAllTransactionsQueryHandler.cs
│   │       ├── GetTransactionById\
│   │       │   ├── GetTransactionByIdQuery.cs
│   │       │   └── GetTransactionByIdQueryHandler.cs
│   │       └── GetTransactionHistory\
│   │           ├── GetTransactionHistoryQuery.cs
│   │           └── GetTransactionHistoryQueryHandler.cs
│   ├── Dashboard\                        # Feature: Dashboard
│   │   └── Queries\
│   │       └── GetDashboard\
│   │           ├── GetDashboardQuery.cs
│   │           └── GetDashboardQueryHandler.cs
│   └── FinTrack.Application.csproj
│
├── FinTrack.Infrastructure\              # Camada de Infraestrutura
│   ├── Data\                             # Configuração do EF Core
│   │   ├── FinTrackDbContext.cs          # DbContext principal
│   │   └── Configurations\               # Fluent API
│   │       ├── UserConfiguration.cs      # Mapeamento de User
│   │       ├── CategoryConfiguration.cs  # Mapeamento de Category
│   │       ├── TransactionConfiguration.cs  # Mapeamento de Transaction + CHECK constraint
│   │       └── TransactionHistoryConfiguration.cs
│   ├── Migrations\                       # Migrations do EF Core
│   └── FinTrack.Infrastructure.csproj
│
├── FinTrack.WebAPI\                      # Camada de Apresentação
│   ├── Controllers\                      # Endpoints da API
│   │   ├── TransactionsController.cs     # CRUD de transações
│   │   └── DashboardController.cs        # Dashboard financeiro
│   ├── Middleware\                       # Middlewares personalizados
│   │   └── UserContextMiddleware.cs      # Extrai X-User-Id do header
│   ├── Program.cs                        # Configuração da aplicação
│   ├── appsettings.json                  # Configurações
│   └── FinTrack.WebAPI.csproj
│
├── FinTrack.Backend.UnitTests\           # Testes Unitários
│   ├── Application\                      # Testes de handlers
│   ├── Domain\                           # Testes de entidades
│   └── FinTrack.Backend.UnitTests.csproj
│
└── FinTrack.Backend.IntegrationTests\    # Testes de Integração BDD
    ├── Features\                         # Cenários Gherkin (pt-BR)
    ├── StepDefinitions\                  # Implementação dos steps
    ├── Hooks\                            # Setup/Teardown
    ├── Support\                          # Helpers
    └── FinTrack.Backend.IntegrationTests.csproj
```

---

## 🎯 Arquitetura

### Clean Architecture

A solução segue os princípios de **Clean Architecture** com separação clara de responsabilidades:

1. **Domain** (núcleo): Entidades, enums, regras de negócio puras
2. **Application**: Casos de uso (commands/queries), DTOs, validações
3. **Infrastructure**: Implementação técnica (banco de dados, EF Core)
4. **WebAPI**: Controllers, middlewares, configuração HTTP

### CQRS (Command Query Responsibility Segregation)

- **Commands**: Operações de escrita (Create, Update, Delete)
- **Queries**: Operações de leitura (GetAll, GetById, GetHistory, Dashboard)

Cada comando/query tem sua própria classe + handler, facilitando manutenção e testes.

---

## 🗄️ Banco de Dados

### Oracle Database

O sistema utiliza **Oracle Database 23c Free** via Docker:

```yaml
# docker-compose.yml (na raiz do projeto)
services:
  oracle:
    image: gvenzl/oracle-free:23-slim-faststart
    ports:
      - "1521:1521"
    environment:
      ORACLE_PASSWORD: fintrack
```

### Modelo de Dados

**Entidades principais:**

- **USERS**: Usuários do sistema
- **CATEGORIES**: Categorias de transações (Salário, Contas, Investimentos, etc.)
- **TRANSACTIONS**: Transações financeiras (receitas/despesas)
  - CHECK CONSTRAINT: `Type IN ('Income', 'Expense')`
  - Soft Delete: campo `DeletedAt`
- **TRANSACTION_HISTORY**: Histórico de alterações (auditoria)

### Entity Framework Core

Configuração em `FinTrackDbContext.cs`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
}
```

**Conversões personalizadas:**
- `Guid` → `VARCHAR2(36)` para melhor visualização
- `TransactionType` → `VARCHAR2(20)` com valores `'Income'` ou `'Expense'`

---

## 🚀 Executando o Backend

### Pré-requisitos

- ✅ .NET 8 SDK
- ✅ Docker Desktop
- ✅ Oracle Database (via Docker)

### Passo a Passo

1. **Subir o Oracle:**
   ```bash
   docker-compose up -d
   ```

2. **Aplicar as migrations:**
   ```bash
   dotnet ef database update --project src\Backend\FinTrack.Infrastructure --startup-project src\Backend\FinTrack.WebAPI
   ```

3. **Executar a API:**
   ```bash
   dotnet run --project src\Backend\FinTrack.WebAPI
   ```

4. **Acessar o Swagger:**
   ```
   https://localhost:7151/swagger
   ```

---

## 🧪 Executando os Testes

### Testes Unitários

```bash
# Todos os testes unitários
dotnet test src\Backend\FinTrack.Backend.UnitTests

# Com cobertura de código
dotnet test src\Backend\FinTrack.Backend.UnitTests --collect:"XPlat Code Coverage"
```

### Testes de Integração (BDD)

```bash
# Todos os testes de integração (Reqnroll em pt-BR)
dotnet test src\Backend\FinTrack.Backend.IntegrationTests

# Com relatório detalhado
dotnet test src\Backend\FinTrack.Backend.IntegrationTests --logger "console;verbosity=detailed"

# Filtrar por feature
dotnet test src\Backend\FinTrack.Backend.IntegrationTests --filter "FullyQualifiedName~Dashboard"
```

**Observação:** Os testes de integração criam um container Oracle isolado (porta `1522`) via Testcontainers.

---

## 📡 Endpoints da API

### Transações

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `GET` | `/api/transactions` | Lista todas as transações do usuário |
| `GET` | `/api/transactions/{id}` | Busca uma transação por ID |
| `POST` | `/api/transactions` | Cria uma nova transação |
| `PUT` | `/api/transactions/{id}` | Atualiza uma transação existente |
| `DELETE` | `/api/transactions/{id}` | Exclui uma transação (soft delete) |
| `GET` | `/api/transactions/{id}/history` | Consulta o histórico de alterações |

### Dashboard

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `GET` | `/api/dashboard` | Retorna resumo financeiro (saldo, receitas, despesas) |

### Headers Obrigatórios

Todas as requisições exigem o header:

```
X-User-Id: <guid-do-usuario>
```

Exemplo:
```
X-User-Id: 11111111-1111-1111-1111-111111111111
```

---

## 🔒 Validações Implementadas

### Nível 1: FluentValidation (Application)

**`TransactionCommandValidator.cs`** valida:

- ✅ `Type`: Deve ser `Income` (1) ou `Expense` (2)
- ✅ `Amount`: Deve ser maior que zero
- ✅ `Description`: Não pode ser vazia e máximo 250 caracteres
- ✅ `CategoryId`: Não pode ser `Guid.Empty`
- ✅ `Date`: Não pode ser `DateTime.MinValue` (01/01/0001)

### Nível 2: Oracle CHECK Constraint (Database)

**`TransactionConfiguration.cs`** adiciona:

```csharp
entity.ToTable("TRANSACTIONS", t => 
    t.HasCheckConstraint("CK_TRANSACTIONS_TYPE", "\"Type\" IN ('Income', 'Expense')")
);
```

Garante integridade no banco mesmo se a aplicação for contornada.

---

## 🌱 Dados de Seed

O sistema já vem com dados pré-configurados em `FinTrackDbContext.OnModelCreating`:

### Usuário Padrão
- **ID:** `11111111-1111-1111-1111-111111111111`
- **Nome:** `Usuário Teste`

### Categorias Padrão
- **Salário** (`22222222-2222-2222-2222-222222222221`) - Income
- **Freelance** (`22222222-2222-2222-2222-222222222222`) - Income
- **Contas** (`22222222-2222-2222-2222-222222222223`) - Expense
- **Alimentação** (`22222222-2222-2222-2222-222222222224`) - Expense
- **Transporte** (`22222222-2222-2222-2222-222222222225`) - Expense
- **Lazer** (`22222222-2222-2222-2222-222222222226`) - Expense
- **Investimentos** (`22222222-2222-2222-2222-222222222227`) - Expense

---

## 🛠️ Tecnologias e Pacotes

### Principais Pacotes NuGet

**Domain:**
- Apenas classes C# puras (zero dependências)

**Application:**
- `MediatR` - Implementação de CQRS
- `FluentValidation` - Validações de comandos
- `FluentValidation.DependencyInjectionExtensions`

**Infrastructure:**
- `Oracle.EntityFrameworkCore` - Provider EF Core para Oracle
- `Microsoft.EntityFrameworkCore.Design` - Ferramentas de migrations

**WebAPI:**
- `Swashbuckle.AspNetCore` - Documentação Swagger/OpenAPI

**Tests:**
- `xUnit` - Framework de testes
- `FluentAssertions` - Assertions fluentes
- `Moq` - Mocks para testes unitários
- `Reqnroll` - BDD/Gherkin (testes de integração)
- `Testcontainers.Oracle` - Oracle isolado para testes

---

## 📝 Exemplo de Uso

### Criar uma transação de receita

```bash
POST https://localhost:7151/api/transactions
Headers:
  X-User-Id: 11111111-1111-1111-1111-111111111111
  Content-Type: application/json

Body:
{
  "type": "Income",
  "categoryId": "22222222-2222-2222-2222-222222222221",
  "amount": 5000.00,
  "date": "2026-01-15",
  "description": "Salário de Janeiro"
}
```

### Consultar o dashboard

```bash
GET https://localhost:7151/api/dashboard
Headers:
  X-User-Id: 11111111-1111-1111-1111-111111111111
```

**Resposta:**
```json
{
  "currentBalance": 5000.00,
  "totalIncomeMonth": 5000.00,
  "totalExpenseMonth": 0.00
}
```

---

## 🐛 Troubleshooting

### Erro: "Oracle.ManagedDataAccess.Client.OracleException: ORA-12154"

**Solução:** Verificar se o Oracle está rodando:
```bash
docker ps
docker logs <container-id>
```

### Erro: "The JSON value could not be converted to TransactionType"

**Solução:** Usar valores válidos: `"Income"` ou `"Expense"` (case-sensitive).

### Erro: "CK_TRANSACTIONS_TYPE violated"

**Solução:** O banco rejeitou um valor inválido de `Type`. Isso significa que a validação da aplicação falhou ou foi contornada.

---

## 📚 Referências

- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [Entity Framework Core Documentation](https://learn.microsoft.com/ef/core/)
- [Oracle Database Free](https://www.oracle.com/database/free/)
- [MediatR](https://github.com/jbogard/MediatR)
- [FluentValidation](https://docs.fluentvalidation.net/)

---

## 📄 Licença

Este projeto faz parte do sistema FinTrack.
