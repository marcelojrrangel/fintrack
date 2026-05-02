# 💰 FinTrack - Sistema de Controle Financeiro Pessoal

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12.0-239120?logo=csharp)
![Angular](https://img.shields.io/badge/Angular-20-DD0031?logo=angular)
![Oracle](https://img.shields.io/badge/Oracle-21c-F80000?logo=oracle)
![License](https://img.shields.io/badge/license-MIT-blue.svg)

Sistema completo de controle financeiro pessoal desenvolvido com **.NET 8 WebAPI** e **Angular 20**. Implementa arquitetura limpa, padrão CQRS, logging profissional em português e integração com Oracle Database.

## 🚀 Tecnologias

### Backend (.NET 8 WebAPI)
- **.NET 8** - Framework principal
- **ASP.NET Core WebAPI** - API RESTful
- **Entity Framework Core 8** - ORM
- **Oracle.EntityFrameworkCore** - Provider Oracle
- **MediatR** - Padrão CQRS e Mediator
- **FluentValidation** - Validação de comandos
- **Serilog** - Logging estruturado profissional
- **Swagger/OpenAPI** - Documentação da API
- **xUnit** - Testes unitários (70+ testes)
- **Reqnroll** - Testes BDD em português

### Frontend (Angular 20)
- **Angular 20** - Framework SPA
- **TypeScript 5.7** - Linguagem tipada
- **RxJS** - Programação reativa
- **Angular Material 20** - Componentes UI
- **Chart.js** - Gráficos e dashboards
- **Jasmine/Karma** - Testes unitários

### Banco de Dados
- **Oracle Database 21c** - Banco de dados relacional
- **Docker** - Containerização do Oracle

## 📋 Funcionalidades

### ✅ Gerenciamento de Transações
- ✅ Criar transações (receitas e despesas)
- ✅ Editar transações existentes
- ✅ Excluir transações (soft delete)
- ✅ **Listar transações com paginação** (5 registros por página por padrão)
- ✅ Visualizar detalhes de transações
- ✅ Histórico completo de alterações (auditoria)

### 📊 Dashboard Financeiro
- ✅ Saldo atual consolidado
- ✅ Total de receitas do mês
- ✅ Total de despesas do mês
- ✅ Evolução do saldo (gráfico temporal)
- ✅ Distribuição por categorias (gráfico pizza)
- ✅ Indicador visual de saúde financeira (verde/vermelho)

### 🏷️ Categorias
- ✅ Categorias pré-definidas (Salário, Alimentação, Transporte, etc.)
- ✅ Suporte a categorias personalizadas por usuário

### 📝 Auditoria Completa
- ✅ Histórico de todas as operações (Create, Update, Delete)
- ✅ Registro de valores anteriores e novos (snapshots)
- ✅ Rastreabilidade de ações por usuário e timestamp

## 📊 Logging Profissional

O projeto implementa logging profissional com **Serilog**:
- ✅ **Logs estruturados em JSON** com rotação diária (arquivo por dia)
- ✅ **Mensagens em Português Brasileiro** (parâmetros técnicos em inglês)
- ✅ **Correlation ID** automático para rastreamento de requisições
- ✅ **Performance logging** com alertas para requisições > 500ms
- ✅ **Global exception handler** com categorização de erros
- ✅ **Logs de auditoria** para todas as operações financeiras
- ✅ **Níveis apropriados**: Information, Warning, Error, Fatal

### Exemplos de Logs:
```
[16:33:00 INF] Iniciando aplicação FinTrack WebAPI
[16:33:00 INF] Banco de dados inicializado com sucesso
[16:33:00 WRN] Requisição lenta detectada: GET /api/transactions levou 1234ms
[16:33:00 INF] AUDITORIA: Histórico de transação registrado. TransactionId: {id}
```

📖 **Documentação completa:** [docs/LOGGING_STRATEGY.md](docs/LOGGING_STRATEGY.md)

## 🏗️ Arquitetura

O projeto segue **Clean Architecture** e **Domain-Driven Design (DDD)**:

```
FinTrack/
├── src/
│   ├── Backend/
│   │   ├── FinTrack.WebAPI/                      # Controllers, Middlewares, Startup
│   │   ├── FinTrack.Application/                 # Handlers, Validações, DTOs
│   │   ├── FinTrack.Domain/                      # Entidades, Enums, Lógica de Negócio
│   │   ├── FinTrack.Infrastructure/              # DbContext, Migrations, Repositories
│   │   ├── FinTrack.Backend.UnitTests/           # 70+ testes unitários (xUnit)
│   │   └── FinTrack.Backend.IntegrationTests/    # Testes BDD (Reqnroll/Gherkin)
│   └── Frontend/
│       └── fintrack-ui/                          # Aplicação Angular 20
├── docker/                                       # Docker Compose (Oracle 21c)
├── docs/                                         # Documentação técnica
└── logs/                                         # Logs estruturados (gitignored)
```

### Padrões de Design Implementados
- ✅ **CQRS** (Command Query Responsibility Segregation)
- ✅ **Mediator** (MediatR)
- ✅ **Repository Pattern**
- ✅ **Unit of Work**
- ✅ **Dependency Injection**
- ✅ **Domain Events**
- ✅ **Value Objects**

## 🐳 Docker & Oracle

O projeto usa **Oracle Database 21c Free** em Docker:

```bash
# Iniciar Oracle Database
cd docker/oracle
docker-compose up -d

# Verificar logs
docker logs oracle-db

# Conectar ao banco
docker exec -it oracle-db sqlplus sys/Oracle123@//localhost:1521/FREEPDB1 as sysdba
```

**Configuração:**
- **Host:** localhost
- **Porta:** 1521
- **SID:** FREEPDB1
- **Usuário:** fintrack_user
- **Senha:** fintrack_pass

## ⚙️ Configuração e Execução

### Pré-requisitos
- **.NET 8 SDK**
- **Node.js 18+** e npm
- **Docker Desktop**
- **Visual Studio 2022** ou VS Code

### Backend (.NET 8 WebAPI)

```bash
# 1. Clonar o repositório
git clone https://github.com/marcelojrrangel/fintrack.git
cd fintrack

# 2. Iniciar Oracle Database
cd docker/oracle
docker-compose up -d
cd ../..

# 3. Restaurar dependências
cd src/Backend
dotnet restore

# 4. Executar migrações do banco de dados
cd FinTrack.WebAPI
dotnet ef database update --project ../FinTrack.Infrastructure

# 5. Executar a aplicação
dotnet run

# API disponível em: https://localhost:5001
# Swagger: https://localhost:5001/swagger
```

### Frontend (Angular 20)

```bash
# 1. Navegar para o diretório do frontend
cd src/Frontend/fintrack-ui

# 2. Instalar dependências
npm install

# 3. Executar em modo de desenvolvimento
npm start

# Aplicação disponível em: http://localhost:4200
```

### Testes

```bash
# Todos os testes backend
cd src/Backend
dotnet test

# Apenas testes unitários (70+)
dotnet test src/Backend/FinTrack.Backend.UnitTests

# Apenas testes de integração (BDD)
dotnet test src/Backend/FinTrack.Backend.IntegrationTests

# Com cobertura de código
dotnet test src/Backend --collect:"XPlat Code Coverage"
```

## 📡 Endpoints da API

### Transactions
```http
GET    /api/transactions              # Listar transações (paginado: 5 por página)
       ?pageNumber=1                   # Número da página (padrão: 1)
       &pageSize=5                     # Tamanho da página (padrão: 5, máx: 100)

GET    /api/transactions/{id}         # Obter transação por ID
POST   /api/transactions              # Criar nova transação
PUT    /api/transactions/{id}         # Atualizar transação
DELETE /api/transactions/{id}         # Excluir transação (soft delete)
GET    /api/transactions/{id}/history # Histórico de alterações
```

**Resposta de paginação:**
```json
{
  "success": true,
  "data": {
    "items": [...],
    "pageNumber": 1,
    "pageSize": 5,
    "totalCount": 50,
    "totalPages": 10,
    "hasPreviousPage": false,
    "hasNextPage": true
  },
  "message": "Transactions retrieved successfully."
}
```

### Dashboard
```http
GET    /api/dashboard                 # Resumo financeiro (saldo, receitas, despesas)
```

### Health Check
```http
GET    /api/health                    # Status da aplicação
```

📖 **Documentação completa:** Acesse o Swagger em `https://localhost:5001/swagger`

## 🧪 Testes

### Cobertura
- ✅ **72+ testes unitários** (xUnit + FluentAssertions)
  - Testes de paginação de transações
  - Testes de handlers com logging
- ✅ **Testes de integração** com Oracle em Docker
- ✅ **Testes BDD** (Reqnroll/Gherkin em português)
- ✅ **Testes de middleware** (Exceptions, Performance, Correlation)
- ✅ **Testes de validação** (FluentValidation)

### Testes BDD (Gherkin)
```gherkin
Cenário: Criar uma transação de despesa válida
  Dado que tenho um usuário com ID "123e4567-e89b-12d3-a456-426614174000"
  E existe uma categoria "Alimentação" para o usuário
  Quando eu criar uma transação com os seguintes dados:
    | Tipo    | Valor  | Descrição       |
    | Despesa | 150.00 | Almoço no restaurante |
  Então a transação deve ser criada com sucesso
  E o histórico deve conter a ação "Created"
```

## 📚 Documentação

- 📖 [Estratégia de Logging Profissional](docs/LOGGING_STRATEGY.md)
- 📖 [Paginação de Transações](docs/PAGINATION.md)
- 📖 [Guia de Testes BDD](src/Backend/FinTrack.Backend.IntegrationTests/README.md)
- 📖 [Arquitetura Backend](src/Backend/README.md)
- 📖 [API Reference (Swagger)](https://localhost:5001/swagger)

## 🔒 Segurança

- ✅ Validação rigorosa de entrada (FluentValidation)
- ✅ Soft delete (dados nunca são apagados permanentemente)
- ✅ Logs não registram dados sensíveis (senhas, tokens)
- ✅ Correlation ID para rastreamento seguro
- ✅ Exception handling global com categorização

## 🛠️ Desenvolvimento

### Convenção de Commits
```
feat: Nova funcionalidade
fix: Correção de bug
docs: Atualização de documentação
test: Adiciona ou corrige testes
refactor: Refatoração de código
style: Formatação/lint
perf: Melhorias de performance
```

### Branch Strategy
- `master` - Produção
- `develop` - Desenvolvimento
- `feature/*` - Novas funcionalidades
- `bugfix/*` - Correções de bugs

## 📄 Licença

Projeto licenciado sob **MIT License**. Veja [LICENSE](LICENSE) para detalhes.

## 👨‍💻 Autor

**Marcelo Jr Rangel**
- GitHub: [@marcelojrrangel](https://github.com/marcelojrrangel)

## 🙏 Agradecimentos

Projeto de demonstração de competências em:
- Arquitetura de software limpa e escalável
- Padrões de design avançados (CQRS, Mediator, Repository)
- Logging e observabilidade profissional
- Integração com Oracle Database
- Testes automatizados (unitários, integração, BDD)
- DevOps com Docker

## 📈 Roadmap

- [ ] Autenticação JWT + Refresh Tokens
- [ ] Autorização baseada em roles/claims
- [ ] Suporte a múltiplas moedas
- [ ] Exportação de relatórios (PDF, Excel)
- [ ] Notificações por e-mail/push
- [ ] API de integração com Open Banking
- [ ] Deploy automatizado (Azure/AWS)
- [ ] CI/CD com GitHub Actions

---

⭐ **Se este projeto foi útil, considere dar uma estrela no GitHub!**
