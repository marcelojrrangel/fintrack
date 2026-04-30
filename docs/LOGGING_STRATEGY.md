# Estratégia de Logging Profissional - FinTrack WebAPI

## 📋 Visão Geral

Esta documentação descreve a estratégia de logging implementada na aplicação **FinTrack WebAPI** seguindo as melhores práticas de engenharia de software para observabilidade e rastreabilidade de operações.

**Idioma dos Logs:** Todas as mensagens de log estão em **Português Brasileiro** para facilitar a leitura e análise pela equipe. Apenas parâmetros e termos técnicos permanecem em inglês (ex: `UserId`, `TransactionId`, `StatusCode`).

---

## 🎯 Objetivos Alcançados

### 1. **Abstração e Desacoplamento**
- ✅ Utilização da interface nativa `ILogger<T>` em controllers e serviços
- ✅ Configuração centralizada via Injeção de Dependência no `Program.cs`
- ✅ Fácil substituição do provedor de log sem alterar código de negócio

### 2. **Implementação Técnica**
- ✅ **Serilog** configurado como provedor de log
- ✅ **Rolling File** com rotação diária (`logs/fintrack-YYYY-MM-DD.json`)
- ✅ Formato **JSON estruturado** (CompactJsonFormatter)
- ✅ Retenção de 30 dias de logs
- ✅ Flush automático a cada 1 segundo

### 3. **Enriquecimento de Logs**
Cada log contém:
- ✅ `Timestamp` - Data/hora do evento
- ✅ `Level` - Nível do log (Information, Warning, Error)
- ✅ `Message` - Mensagem estruturada
- ✅ `CorrelationId` - Identificador único da requisição (via middleware)
- ✅ `Exception` - Stack trace completo quando aplicável
- ✅ `UserId` - Identificação do usuário da requisição
- ✅ `MachineName` - Nome da máquina/servidor
- ✅ `ThreadId` - Identificador da thread

---

## 🏗️ Arquitetura de Logging

### **Camadas de Logging**

```
┌─────────────────────────────────────────────┐
│          Program.cs (Bootstrap)             │
│  • Inicializa Serilog                       │
│  • Configura sinks (File, Console)          │
│  • Define níveis mínimos                    │
└─────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────┐
│         Middlewares (Pipeline)              │
│  • CorrelationIdMiddleware                  │
│  • PerformanceLoggingMiddleware             │
│  • ExceptionHandlingMiddleware              │
└─────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────┐
│      Application Layer (Handlers)           │
│  • CreateTransactionCommandHandler          │
│  • UpdateTransactionCommandHandler          │
│  • DeleteTransactionCommandHandler          │
└─────────────────────────────────────────────┘
```

---

## 📦 Pacotes NuGet Instalados

```xml
<PackageReference Include="Serilog.AspNetCore" />
<PackageReference Include="Serilog.Sinks.File" />
<PackageReference Include="Serilog.Formatting.Compact" />
<PackageReference Include="Serilog.Enrichers.Environment" />
<PackageReference Include="Serilog.Enrichers.Thread" />
```

---

## 🔧 Configuração no Program.cs

```csharp
// Configuração inicial do Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithProperty("Application", "FinTrack.WebAPI")
    .WriteTo.Console()
    .WriteTo.File(
        new CompactJsonFormatter(),
        path: "logs/fintrack-.json",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        shared: true,
        flushToDiskInterval: TimeSpan.FromSeconds(1))
    .CreateLogger();

// Integração com ASP.NET Core
builder.Host.UseSerilog();

// Request logging enriquecido
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"]);
        diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress);
        if (httpContext.Request.Headers.TryGetValue("X-User-Id", out var userId))
        {
            diagnosticContext.Set("UserId", userId);
        }
    };
});
```

---

## 🎯 Pontos de Logging Implementados

### **1. Global Exception Handler** ✅
**Arquivo:** `ExceptionHandlingMiddleware.cs`

**Funcionalidade:**
- Captura todas as exceções não tratadas
- Loga stack trace completo
- Registra detalhes da requisição (path, método, userId)

**Níveis de Log:**
- `LogWarning`: ValidationException, NotFoundException, UnauthorizedException
- `LogError`: Exceções inesperadas (críticas)

**Exemplo de Log:**
```json
{
  "@t": "2024-04-28T16:33:00.1234567Z",
  "@l": "Error",
  "@mt": "CRÍTICO: Exceção não tratada ocorreu para {RequestPath}",
  "RequestPath": "POST /api/transactions",
  "UserId": "123e4567-e89b-12d3-a456-426614174000",
  "StatusCode": 500,
  "ExceptionType": "SqlException",
  "Mensagem": "Connection timeout",
  "StackTrace": "...",
  "CorrelationId": "7f8c9d2e-a1b3-4c5d-8e9f-1234567890ab"
}
```

---

### **2. Transações Financeiras** ✅
**Arquivos:** 
- `CreateTransactionCommandHandler.cs`
- `UpdateTransactionCommandHandler.cs`
- `DeleteTransactionCommandHandler.cs`

**Funcionalidade:**
- Log de início da operação
- Log de sucesso com ID gerado
- Log de erros de validação

**Exemplo - Criação de Transação:**
```csharp
_logger.LogInformation(
    "Iniciando criação de transação. UserId: {UserId}, CategoryId: {CategoryId}, Amount: {Amount}",
    userId, categoryId, amount);

// ... operação de criação ...

_logger.LogInformation(
    "Transação criada com sucesso. TransactionId: {TransactionId}, Amount: {Amount}",
    transactionId, amount);
```

---

### **3. Performance Monitoring** ✅
**Arquivo:** `PerformanceLoggingMiddleware.cs`

**Funcionalidade:**
- Mede tempo de processamento de cada requisição
- Emite `LogWarning` quando requisição > 500ms
- Registra `LogInformation` para requisições normais

**Threshold:** 500ms

**Exemplo de Log (Requisição Lenta):**
```json
{
  "@t": "2024-04-28T16:33:00.1234567Z",
  "@l": "Warning",
  "@mt": "Requisição lenta detectada: {RequestPath} levou {ElapsedMilliseconds}ms",
  "RequestPath": "GET /api/transactions",
  "ElapsedMilliseconds": 1234,
  "ThresholdMs": 500,
  "StatusCode": 200,
  "CorrelationId": "7f8c9d2e-a1b3-4c5d-8e9f-1234567890ab"
}
```

---

### **4. Auditoria** ✅
**Funcionalidade:**
- Registra todas as operações em `transaction_history`
- Log de criação, atualização e exclusão de transações
- Inclui identificação do usuário e ação realizada

**Exemplo de Log de Auditoria:**
```csharp
_logger.LogInformation(
    "AUDITORIA: Histórico de transação registrado. TransactionId: {TransactionId}, UserId: {UserId}, Action: {Action}, HistoryId: {HistoryId}",
    transactionId, userId, HistoryActionType.Created, historyId);
```

**Tipos de Ação Auditados:**
- `Created` - Transação criada
- `Updated` - Transação atualizada
- `Deleted` - Transação excluída (soft delete)

---

### **5. Correlation ID** ✅
**Arquivo:** `CorrelationIdMiddleware.cs`

**Funcionalidade:**
- Gera ou lê `X-Correlation-Id` de cada requisição
- Propaga o ID para todos os logs da requisição
- Facilita rastreamento de fluxo completo

**Header HTTP:**
```
X-Correlation-Id: 7f8c9d2e-a1b3-4c5d-8e9f-1234567890ab
```

---

## 📊 Níveis de Log Utilizados

| Nível | Uso | Exemplos |
|-------|-----|----------|
| **Information** | Fluxos normais de operação | Criação de transação, inicialização, requisições bem-sucedidas |
| **Warning** | Falhas esperadas, lentidão | Validação falhou, recurso não encontrado, requisição > 500ms |
| **Error** | Falhas críticas de infraestrutura | Exceções não tratadas, falhas de DB, erros inesperados |
| **Fatal** | Falha catastrófica na aplicação | Erro durante startup, impossível iniciar a aplicação |

---

## 📁 Estrutura de Arquivos de Log

```
D:\projetos\provas\tce\
├── logs/
│   ├── fintrack-20240428.json    # Logs de hoje
│   ├── fintrack-20240427.json    # Logs de ontem
│   ├── fintrack-20240426.json    # Logs de 2 dias atrás
│   └── ...                        # Até 30 dias de retenção
```

**Política de Rotação:**
- Um arquivo por dia
- Formato: `fintrack-YYYYMMDD.json`
- Retenção: 30 dias (configurável)
- Modo compartilhado (shared: true) para múltiplos processos

---

## 🧪 Validação e Testes

### **Testes Unitários**
Todos os handlers e middlewares possuem testes unitários com `NullLogger<T>.Instance`:

```csharp
var handler = new CreateTransactionCommandHandler(
    context, 
    currentUserService, 
    NullLogger<CreateTransactionCommandHandler>.Instance
);
```

### **Logs de Teste**
Os testes de integração produzem logs reais visíveis no output:
```
[16:33:00 INF] Iniciando aplicação FinTrack WebAPI
[16:33:00 INF] Banco de dados inicializado com sucesso
[16:33:00 INF] FinTrack WebAPI iniciada com sucesso
[16:33:00 WRN] Falha na validação para POST /api/transactions
[16:33:00 INF] Requisição POST /api/transactions concluída em 170ms com status 400
```

---

## 🔍 Consultando Logs

### **Logs Estruturados (JSON)**
Os logs em formato JSON podem ser facilmente consultados com ferramentas como:
- **jq** (linha de comando)
- **Seq** (plataforma de log agregado)
- **ELK Stack** (Elasticsearch, Logstash, Kibana)
- **Azure Application Insights**

### **Exemplo de Consulta (jq):**
```bash
# Buscar todos os erros do dia
cat logs/fintrack-20240428.json | jq 'select(.@l == "Error")'

# Buscar logs de um CorrelationId específico
cat logs/fintrack-20240428.json | jq 'select(.CorrelationId == "7f8c9d2e-a1b3-4c5d-8e9f-1234567890ab")'

# Buscar requisições lentas
cat logs/fintrack-20240428.json | jq 'select(.ElapsedMilliseconds > 500)'
```

---

## 🚀 Benefícios da Implementação

1. ✅ **Rastreabilidade Completa**: CorrelationId permite seguir toda a jornada de uma requisição
2. ✅ **Desacoplamento**: Trocar provedor de log não requer mudança no código de negócio
3. ✅ **Observabilidade**: Logs estruturados facilitam análise e alertas automáticos
4. ✅ **Performance**: Identificação imediata de requisições lentas
5. ✅ **Auditoria**: Histórico completo de operações financeiras
6. ✅ **Depuração**: Stack traces detalhados para erros críticos
7. ✅ **Conformidade**: Logs atendem requisitos de compliance e auditoria

---

## 📝 Boas Práticas Seguidas

- ✅ Uso de `ILogger<T>` para tipagem forte
- ✅ Logging estruturado com propriedades nomeadas
- ✅ Evitar logging excessivo (performance)
- ✅ Níveis de log apropriados para cada situação
- ✅ Dados sensíveis não são logados (senhas, tokens)
- ✅ Logs enriquecidos com contexto da requisição
- ✅ Shutdown gracioso com `Log.CloseAndFlush()`

---

## 🔒 Segurança e Privacidade

**Dados NÃO logados:**
- Senhas ou tokens de autenticação
- Informações de cartão de crédito
- Dados pessoais sensíveis (CPF, RG)

**Dados logados:**
- UserId (GUID anonimizado)
- Valores de transações (necessário para auditoria financeira)
- IPs e User-Agents (para análise de segurança)

---

## 📚 Referências

- [Serilog Documentation](https://serilog.net/)
- [ASP.NET Core Logging](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/)
- [Structured Logging Best Practices](https://messagetemplates.org/)

---

**Desenvolvido por:** Backend Engineer Sênior  
**Data:** Abril 2024  
**Versão:** 1.0  
