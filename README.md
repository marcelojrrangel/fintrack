# 💰 FinTrack - Sistema de Controle Financeiro Pessoal

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![C#](https://img.shields.io/badge/C%23-12.0-239120?logo=csharp)
![Angular](https://img.shields.io/badge/Angular-17-DD0031?logo=angular)
![Oracle](https://img.shields.io/badge/Oracle-21c-F80000?logo=oracle)
![License](https://img.shields.io/badge/license-MIT-blue.svg)

Sistema completo de controle financeiro pessoal desenvolvido com **.NET 8 WebAPI** e **Angular 17**. Implementa arquitetura limpa, padrão CQRS, logging profissional em português e integração com Oracle Database.

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

### Frontend (Angular 17)
- **Angular 17** - Framework SPA
- **TypeScript** - Linguagem tipada
- **RxJS** - Programação reativa
- **Angular Material** - Componentes UI

### Banco de Dados
- **Oracle Database 21c** - Banco de dados relacional
- **Docker** - Containerização do Oracle

## 📊 Logging Profissional

O projeto implementa logging profissional com Serilog:
- ✅ Logs estruturados em JSON com rotação diária
- ✅ Mensagens em Português Brasileiro
- ✅ Correlation ID para rastreamento
- ✅ Performance logging (alertas > 500ms)
- ✅ Logs de auditoria completos

📖 Documentação: [docs/LOGGING_STRATEGY.md](docs/LOGGING_STRATEGY.md)

## ⚙️ Configuração

```bash
# 1. Clonar o repositório
git clone https://github.com/marcelojrrangel/fintrack.git

# 2. Iniciar Oracle
cd docker/oracle
docker-compose up -d

# 3. Executar API
cd src/Backend/FinTrack.WebAPI
dotnet run
```

## 👨‍💻 Autor

**Marcelo Jr Rangel**
- GitHub: [@marcelojrrangel](https://github.com/marcelojrrangel)

⭐ Se este projeto foi útil, considere dar uma estrela!
