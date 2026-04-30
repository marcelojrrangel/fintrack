# language: pt-BR

Funcionalidade: Gerenciamento de Transações
  Como um usuário do sistema FinTrack
  Eu quero gerenciar minhas transações financeiras
  Para manter meu histórico financeiro organizado

  Contexto:
    Dado que eu sou o usuário "11111111-1111-1111-1111-111111111111"

  Cenário: Criar uma transação de receita
    Quando eu crio uma transação com os seguintes dados:
      | Campo              | Valor                                |
      | Tipo               | Income                               |
      | Categoria          | 22222222-2222-2222-2222-222222222221 |
      | Valor              | 5000.00                              |
      | Data               | 2026-01-15                           |
      | Descrição          | Salário de Janeiro                   |
    Então a transação deve ser criada com sucesso
    E a resposta deve conter o ID da transação
    E o tipo deve ser "Income"
    E o valor deve ser 5000.00

  Cenário: Criar uma transação de despesa
    Quando eu crio uma transação com os seguintes dados:
      | Campo     | Valor                                |
      | Tipo      | Expense                              |
      | Categoria | 22222222-2222-2222-2222-222222222222 |
      | Valor     | 1200.00                              |
      | Data      | 2026-01-20                           |
      | Descrição | Aluguel                              |
    Então a transação deve ser criada com sucesso
    E um registro de histórico deve ser criado com ação "Created"

  Cenário: Atualizar uma transação existente
    Dado que existe uma transação de despesa no valor de 1000.00 com descrição "Despesa Original"
    Quando eu atualizo o valor para 1500.00
    E eu atualizo a descrição para "Aluguel Atualizado"
    Então a transação deve ser atualizada com sucesso
    E o valor deve ser 1500.00
    E a descrição deve ser "Aluguel Atualizado"

  Cenário: Excluir uma transação
    Dado que existe uma transação com descrição "Despesa Temporária"
    Quando eu excluo a transação
    Então a transação deve ser excluída com sucesso
    E a transação não deve aparecer na listagem de transações

  Cenário: Listar todas as transações do usuário
    Dado que existem as seguintes transações:
      | Tipo    | Valor | Descrição    |
      | Income  | 3000  | Salário      |
      | Expense | 500   | Supermercado |
      | Expense | 200   | Transporte   |
    Quando eu listo todas as transações
    Então devo ver 3 transações na lista
    E a transação "Salário" deve estar na lista
    E a transação "Supermercado" deve estar na lista

  Cenário: Buscar transação por ID
    Dado que existe uma transação com descrição "Minha Transação"
    Quando eu busco a transação pelo ID
    Então a transação deve ser retornada
    E a descrição deve ser "Minha Transação"

  Cenário: Consultar histórico de alterações de uma transação
    Dado que existe uma transação com descrição "Transação Original"
    E a transação foi atualizada com descrição "Transação Modificada 1"
    E a transação foi atualizada com descrição "Transação Modificada 2"
    Quando eu consulto o histórico da transação
    Então devo ver 3 registros de histórico
    E o primeiro registro deve ter ação "Created"
    E os próximos 2 registros devem ter ação "Updated"
