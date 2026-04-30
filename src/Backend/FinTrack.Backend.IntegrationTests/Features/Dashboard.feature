# language: pt-BR

Funcionalidade: Dashboard Financeiro
  Como um usuário do sistema FinTrack
  Eu quero visualizar meu resumo financeiro
  Para ter controle sobre minhas finanças

  Contexto:
    Dado que eu sou o usuário "11111111-1111-1111-1111-111111111111"

  Cenário: Visualizar dashboard sem transações
    Quando eu consulto o dashboard
    Então a requisição deve ser bem-sucedida
    E o total de receitas deve ser 0.00
    E o total de despesas deve ser 0.00
    E o balanço deve ser 0.00

  Cenário: Visualizar dashboard com transações
    Dado que existem as seguintes transações:
      | Tipo    | Categoria | Valor | Data       | Descrição        |
      | Income  | Salary    | 5000  | 2026-01-01 | Salário Janeiro  |
      | Expense | Bills     | 1500  | 2026-01-05 | Aluguel          |
      | Expense | Bills     | 500   | 2026-01-10 | Conta de Luz     |
    Quando eu consulto o dashboard
    Então a requisição deve ser bem-sucedida
    E o total de receitas deve ser 5000.00
    E o total de despesas deve ser 2000.00
    E o balanço deve ser 3000.00

  Cenário: Dashboard com transações excluídas não deve contabilizá-las
    Dado que existem as seguintes transações:
      | Tipo    | Categoria | Valor | Data       | Descrição |
      | Income  | Salary    | 3000  | 2026-01-01 | Salário   |
      | Expense | Bills     | 1000  | 2026-01-05 | Despesa 1 |
    E a transação "Despesa 1" foi excluída
    Quando eu consulto o dashboard
    Então a requisição deve ser bem-sucedida
    E o total de receitas deve ser 3000.00
    E o total de despesas deve ser 0.00
    E o balanço deve ser 3000.00
