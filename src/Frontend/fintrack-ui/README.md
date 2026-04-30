# FinTrack UI

Frontend Angular 20 da aplicacao FinTrack, construida com **Standalone Components**, **Signals**, **Reactive Forms**, **Bootstrap 5 dark mode** e **Chart.js**.

## Pre-requisitos

- Node.js 20+
- npm 10+
- Backend FinTrack WebAPI em execucao

## Instalacao

```bash
npm install
```

## Executando em desenvolvimento

```bash
npm start
```

- URL local: `http://localhost:4200`
- As chamadas HTTP usam `proxy.conf.json`
- O proxy aponta para `https://localhost:7295`
- O header `X-User-Id` usa o usuario seed configurado em `src/environments`

## Build de producao

```bash
npm run build
```

Saida gerada em `dist\fintrack-ui`.

## Testes unitarios

Execucao headless:

```bash
npm run test:ci
```

Modo watch:

```bash
npm test
```

## Relatorio de cobertura

```bash
npm run test:coverage
```

Arquivos gerados em `coverage\fintrack-ui\`.

Para abrir o relatorio HTML, acesse:

```bash
coverage\fintrack-ui\index.html
```

## Estrutura principal

```text
src\app
|-- core
|   |-- constants
|   |-- models
|   `-- services
|-- features
|   |-- dashboard
|   `-- transactions
`-- testing
```

## Funcionalidades implementadas

- Dashboard com cards de saldo, entradas e saidas
- Atualizacao via Signals
- Grafico de evolucao do saldo
- Listagem de transacoes com filtro por categoria
- Modal de nova transacao com formulario reativo
- Tela de detalhes com grafico por categoria e historico de auditoria

## Observacoes

- As categorias exibidas no frontend seguem o seed padrao do backend.
- Para uso local completo, suba primeiro a API e o banco Oracle descritos no README da raiz do repositorio.
