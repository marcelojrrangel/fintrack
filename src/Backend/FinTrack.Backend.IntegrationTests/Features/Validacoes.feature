# language: pt-BR

Funcionalidade: Validações de Transações
  Como um sistema de controle financeiro
  Eu quero validar os dados das transações
  Para garantir a integridade dos dados

  Contexto:
    Dado que eu sou o usuário "11111111-1111-1111-1111-111111111111"

  Esquema do Cenário: Validar tipo de transação inválido
    Quando eu tento criar uma transação com tipo inválido "<TipoInvalido>"
    Então a requisição deve falhar com status 400
    E a mensagem de erro deve conter "Type"

    Exemplos:
      | TipoInvalido |
      | 0            |
      | 999          |

  Cenário: Validar valor negativo
    Quando eu tento criar uma transação com valor -100.00
    Então a requisição deve falhar com status 400
    E a mensagem de erro deve conter "Amount"

  Cenário: Validar valor zero
    Quando eu tento criar uma transação com valor 0.00
    Então a requisição deve falhar com status 400
    E a mensagem de erro deve conter "Amount"

  Cenário: Validar descrição vazia
    Quando eu tento criar uma transação com descrição vazia
    Então a requisição deve falhar com status 400
    E a mensagem de erro deve conter "Description"

  Cenário: Validar descrição muito longa
    Quando eu tento criar uma transação com descrição de 300 caracteres
    Então a requisição deve falhar com status 400
    E a mensagem de erro deve conter "Description"

  Cenário: Validar categoria inexistente
    Quando eu tento criar uma transação com categoria "99999999-9999-9999-9999-999999999999"
    Então a requisição deve falhar com status 404

  Cenário: Validar data de transação padrão
    Quando eu tento criar uma transação com data padrão
    Então a requisição deve falhar com status 400
    E a mensagem de erro deve conter "TransactionDateUtc"
