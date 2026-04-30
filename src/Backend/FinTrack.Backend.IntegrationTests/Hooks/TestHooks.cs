using FinTrack.Application.Common.Abstractions;
using FinTrack.Backend.IntegrationTests.Support;
using FinTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace FinTrack.Backend.IntegrationTests.Hooks;

/// <summary>
/// Fake DatabaseInitializer que não faz nada - usado nos testes para evitar inicialização do banco de desenvolvimento
/// </summary>
internal class FakeDatabaseInitializer : IDatabaseInitializer
{
    public Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}

[Binding]
public class TestHooks
{
    private static readonly OracleTestContainerFixture _oracleFixture = new();
    private static readonly SemaphoreSlim _schemaLock = new(1, 1);
    private static bool _containerInitialized;

    private WebApplicationFactory<Program>? _factory;
    private readonly TestContext _testContext;
    private readonly ScenarioContext _scenarioContext;

    public TestHooks(TestContext testContext, ScenarioContext scenarioContext)
    {
        _testContext = testContext;
        _scenarioContext = scenarioContext;

        // Inicializar container se ainda não foi inicializado
        if (!_containerInitialized)
        {
            _oracleFixture.InitializeAsync().GetAwaiter().GetResult();
            _containerInitialized = true;
        }
    }

    [BeforeScenario(Order = -10000)]
    public async Task BeforeScenario()
    {
        // CRITICAL: Aguardar o lock para evitar concorrência no schema
        await _schemaLock.WaitAsync();

        try
        {
            Console.WriteLine($"🧪 Iniciando cenário: {_scenarioContext.ScenarioInfo.Title}");

        // Criar WebApplicationFactory com banco de teste
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remover o DatabaseInitializer para não executar durante os testes
                    var initializerDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(IDatabaseInitializer));
                    if (initializerDescriptor != null)
                        services.Remove(initializerDescriptor);

                    // Adicionar um initializer fake que não faz nada
                    services.AddScoped<IDatabaseInitializer>(sp => 
                        new FakeDatabaseInitializer());

                    // Remover DbContext existente
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<FinTrackDbContext>));

                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Adicionar DbContext com Oracle de teste
                    var connectionString = _oracleFixture.GetConnectionString();
                    Console.WriteLine($"📊 Connection String para teste: {connectionString}");

                    services.AddDbContext<FinTrackDbContext>(options =>
                    {
                        options.UseOracle(connectionString);
                        options.EnableSensitiveDataLogging();
                        options.EnableDetailedErrors();
                    });
                });
            });

        _testContext.ApiClient = _factory.CreateClient();

        // Inicializar banco de dados
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FinTrackDbContext>();

        try
        {
            // Dropar todas as tabelas antes de criar novas
            await DroparTodasTabelasAsync(dbContext);

            // Criar o banco de dados
            await dbContext.Database.EnsureCreatedAsync();
            Console.WriteLine("✅ Banco de dados criado com sucesso");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao criar banco de dados: {ex.Message}");
            Console.WriteLine($"❌ Connection String usada: {_oracleFixture.GetConnectionString()}");
            Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");

            if (ex.InnerException != null)
            {
                Console.WriteLine($"❌ Inner Exception: {ex.InnerException.Message}");
            }

            // Liberar o lock antes de propagar
            _schemaLock.Release();
            throw;
        }
        }
        catch
        {
            // Em caso de erro, liberar o lock externo também
            _schemaLock.Release();
            throw;
        }
    }

    private async Task DroparTodasTabelasAsync(FinTrackDbContext dbContext)
    {
        try
        {
            var connection = dbContext.Database.GetDbConnection();
            var wasOpened = false;

            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync();
                wasOpened = true;
            }

            // Primeiro, listar as tabelas existentes
            var tabelas = new List<string>();
            using (var cmdList = connection.CreateCommand())
            {
                cmdList.CommandText = "SELECT table_name FROM user_tables ORDER BY table_name";
                using var reader = await cmdList.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    tabelas.Add(reader.GetString(0));
                }
            }

            Console.WriteLine($"🔍 Tabelas encontradas: {tabelas.Count} ({string.Join(", ", tabelas)})");

            // Dropar cada tabela individualmente
            foreach (var tabela in tabelas)
            {
                try
                {
                    using var cmdDrop = connection.CreateCommand();
                    cmdDrop.CommandText = $"DROP TABLE \"{tabela}\" CASCADE CONSTRAINTS PURGE";
                    await cmdDrop.ExecuteNonQueryAsync();
                    Console.WriteLine($"  ✅ Tabela {tabela} removida");
                }
                catch (Exception exDrop)
                {
                    Console.WriteLine($"  ⚠️  Erro ao remover {tabela}: {exDrop.Message}");
                }
            }

            Console.WriteLine($"🧹 Limpeza concluída");

            if (wasOpened)
            {
                await connection.CloseAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Erro geral ao dropar tabelas: {ex.Message}");
            // Não lançar exceção, pode ser a primeira execução
        }
    }

    [AfterScenario(Order = 10000)]
    public void AfterScenario()
    {
        Console.WriteLine($"🏁 Finalizando cenário: {_scenarioContext.ScenarioInfo.Title}");

        _testContext.ApiClient?.Dispose();
        _factory?.Dispose();

        // Limpar dados do contexto
        _testContext.TransactionIds.Clear();
        _testContext.TestData.Clear();
        _testContext.LastResponse = null;
        _testContext.LastTransactionId = null;

        // CRITICAL: Liberar o lock após o cenário
        _schemaLock.Release();
    }
}
