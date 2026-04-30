# 📄 Paginação de Transações - FinTrack API

## 📋 Visão Geral

A API FinTrack implementa paginação para o endpoint de listagem de transações, permitindo carregar grandes volumes de dados de forma eficiente e com melhor experiência do usuário.

---

## 🎯 Configuração Padrão

- **Tamanho de página padrão:** 5 registros
- **Tamanho mínimo:** 1 registro
- **Tamanho máximo:** 100 registros por página
- **Número da página base:** 1 (não zero-based)

---

## 📡 Endpoint

### GET /api/transactions

Lista todas as transações do usuário autenticado com suporte a paginação.

#### Query Parameters

| Parâmetro | Tipo | Obrigatório | Padrão | Descrição |
|-----------|------|-------------|--------|-----------|
| `pageNumber` | `int` | Não | `1` | Número da página desejada (base 1) |
| `pageSize` | `int` | Não | `5` | Quantidade de itens por página (máx: 100) |

#### Headers Requeridos

```http
X-User-Id: {guid}
```

---

## 📦 Resposta

### Estrutura JSON

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "categoryId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "categoryName": "Salário",
        "amount": 5000.00,
        "transactionDateUtc": "2025-04-30T20:30:00Z",
        "type": "Income",
        "description": "Salário mensal",
        "isDeleted": false,
        "createdAtUtc": "2025-04-30T20:30:00Z",
        "updatedAtUtc": null
      }
    ],
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

### Propriedades da Paginação

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `items` | `array` | Lista de transações da página atual |
| `pageNumber` | `int` | Número da página atual |
| `pageSize` | `int` | Tamanho da página solicitada |
| `totalCount` | `int` | Total de transações (todas as páginas) |
| `totalPages` | `int` | Total de páginas disponíveis |
| `hasPreviousPage` | `bool` | Indica se há uma página anterior |
| `hasNextPage` | `bool` | Indica se há uma próxima página |

---

## 🔍 Exemplos de Uso

### Primeira Página (Padrão)

```bash
curl -X GET "https://localhost:5001/api/transactions" \
  -H "X-User-Id: 3fa85f64-5717-4562-b3fc-2c963f66afa6"
```

Equivalente a:
```bash
curl -X GET "https://localhost:5001/api/transactions?pageNumber=1&pageSize=5" \
  -H "X-User-Id: 3fa85f64-5717-4562-b3fc-2c963f66afa6"
```

### Segunda Página (5 itens)

```bash
curl -X GET "https://localhost:5001/api/transactions?pageNumber=2&pageSize=5" \
  -H "X-User-Id: 3fa85f64-5717-4562-b3fc-2c963f66afa6"
```

### Página com 10 Itens

```bash
curl -X GET "https://localhost:5001/api/transactions?pageNumber=1&pageSize=10" \
  -H "X-User-Id: 3fa85f64-5717-4562-b3fc-2c963f66afa6"
```

### Página Máxima (100 itens)

```bash
curl -X GET "https://localhost:5001/api/transactions?pageNumber=1&pageSize=100" \
  -H "X-User-Id: 3fa85f64-5717-4562-b3fc-2c963f66afa6"
```

---

## ⚙️ Comportamento e Validações

### Valores Inválidos

- **`pageNumber < 1`**: Será ajustado automaticamente para `1`
- **`pageSize < 1`**: Será ajustado automaticamente para `5` (padrão)
- **`pageSize > 100`**: Será limitado a `100` (máximo)

### Página Vazia

Quando `pageNumber` excede o total de páginas disponíveis:

```json
{
  "success": true,
  "data": {
    "items": [],
    "pageNumber": 99,
    "pageSize": 5,
    "totalCount": 12,
    "totalPages": 3,
    "hasPreviousPage": true,
    "hasNextPage": false
  },
  "message": "Transactions retrieved successfully."
}
```

### Usuário Sem Transações

```json
{
  "success": true,
  "data": {
    "items": [],
    "pageNumber": 1,
    "pageSize": 5,
    "totalCount": 0,
    "totalPages": 0,
    "hasPreviousPage": false,
    "hasNextPage": false
  },
  "message": "Transactions retrieved successfully."
}
```

---

## 🔧 Implementação Técnica

### Arquitetura

```
Controller (TransactionsController)
    ↓
Query (GetTransactionsQuery)
    ↓
Handler (GetTransactionsQueryHandler)
    ↓
EF Core (Skip/Take)
    ↓
Oracle Database
```

### Ordenação

As transações são **sempre ordenadas** por `TransactionDateUtc` em ordem **decrescente** (mais recentes primeiro).

### Performance

- **Contagem eficiente**: `COUNT(*)` é executado apenas uma vez
- **Skip/Take otimizado**: EF Core traduz para `OFFSET/FETCH` no Oracle
- **Sem tracking**: Queries utilizam `.AsNoTracking()` para melhor performance
- **Índices recomendados**:
  ```sql
  CREATE INDEX IDX_TRANSACTIONS_USER_DATE 
  ON TRANSACTIONS(USER_ID, TRANSACTION_DATE_UTC DESC, IS_DELETED);
  ```

---

## 📊 Logs

A paginação gera logs estruturados em português:

```log
[17:30:00 INF] Buscando transações paginadas para o usuário. 
               UserId: 3fa85f64-5717-4562-b3fc-2c963f66afa6, 
               PageNumber: 1, 
               PageSize: 5

[17:30:00 INF] Transações recuperadas com sucesso. 
               UserId: 3fa85f64-5717-4562-b3fc-2c963f66afa6, 
               TotalCount: 50, 
               ItemsReturned: 5
```

---

## 🧪 Testes

### Cobertura de Testes

✅ **GetTransactionsQueryHandler_ShouldPaginateCorrectly**
- Testa paginação com 12 transações
- Valida primeira página (5 itens)
- Valida segunda página (5 itens)
- Valida terceira página (2 itens restantes)
- Verifica `HasPreviousPage` e `HasNextPage`

✅ **GetTransactionsQueryHandler_ShouldReturnOrderedTransactions**
- Valida ordenação por data (mais recente primeiro)
- Verifica estrutura de `PagedResponse`

✅ **TransactionsController_ShouldHandleReadEndpoints**
- Valida integração controller/handler
- Testa passagem de parâmetros de paginação

---

## 🎨 Integração Frontend

### Angular (TypeScript)

```typescript
interface PagedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

class TransactionService {
  getTransactions(pageNumber: number = 1, pageSize: number = 5): Observable<PagedResponse<Transaction>> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<ApiResponse<PagedResponse<Transaction>>>(
      `${this.apiUrl}/transactions`,
      { params, headers: { 'X-User-Id': this.userId } }
    ).pipe(map(response => response.data));
  }
}
```

### React (JavaScript)

```javascript
const fetchTransactions = async (pageNumber = 1, pageSize = 5) => {
  const response = await fetch(
    `https://localhost:5001/api/transactions?pageNumber=${pageNumber}&pageSize=${pageSize}`,
    {
      headers: {
        'X-User-Id': userId
      }
    }
  );

  const result = await response.json();
  return result.data; // PagedResponse
};
```

---

## 🚀 Boas Práticas

### Frontend

1. **Armazenar metadados de paginação** (totalPages, currentPage)
2. **Desabilitar botões** quando `hasPreviousPage` ou `hasNextPage` são `false`
3. **Mostrar indicador de loading** durante requisições
4. **Cachear páginas** já visitadas para melhorar UX
5. **Permitir usuário escolher** `pageSize` (ex: 5, 10, 25, 50)

### Backend

1. ✅ Validação automática de parâmetros (implementado)
2. ✅ Limite máximo de `pageSize` (implementado: 100)
3. ✅ Logging de performance (implementado)
4. ✅ Testes automatizados (implementado: 72+ testes)
5. 🔄 Considerar cache de contagem para queries frequentes (futuro)

---

## 📚 Referências

- [Documentação API Completa](../README.md#-endpoints-da-api)
- [Testes Unitários](../../tests/Backend/FinTrack.Backend.UnitTests/Application/QueryAndCommandHandlerTests.cs)
- [Swagger UI](https://localhost:5001/swagger)

---

## 🔄 Histórico de Versões

| Versão | Data | Descrição |
|--------|------|-----------|
| 1.0.0 | 2025-04-30 | Implementação inicial de paginação para transações |

---

✨ **Nota:** Este documento descreve a implementação técnica da paginação. Para uso geral da API, consulte o [README principal](../README.md).
