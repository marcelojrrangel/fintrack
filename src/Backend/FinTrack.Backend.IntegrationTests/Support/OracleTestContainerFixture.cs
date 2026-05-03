using Oracle.ManagedDataAccess.Client;

namespace FinTrack.Backend.IntegrationTests.Support;

/// <summary>
/// Fixture que gerencia a conexão com o banco de dados Oracle para testes
/// IMPORTANTE: Execute o script setup-test-db.ps1 ANTES de rodar os testes
/// </summary>
public class OracleTestContainerFixture : IDisposable
{
    // Usar schema SYSTEM para testes (Oracle Free permite isso)
    // Cada teste criará e destruirá suas tabelas
    private const string ConnectionString = "User Id=system;Password=TestPassword123;Data Source=localhost:1523/FREEPDB1;";
    private static readonly SemaphoreSlim _lock = new(1, 1);
    private static bool _initialized;

    public string GetConnectionString() => ConnectionString;

    public async Task InitializeAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (_initialized)
            {
                return;
            }

            Console.WriteLine("🔍 Verificando conexão com Oracle para testes...");

            // Verificar se conseguimos conectar
            var maxAttempts = 3;
            var attempt = 0;
            var connected = false;

            while (attempt < maxAttempts && !connected)
            {
                attempt++;
                try
                {
                    using var connection = new OracleConnection(ConnectionString);
                    await connection.OpenAsync();

                    // Limpar todas as tabelas do schema de testes no início
                    Console.WriteLine("🧹 Limpando schema de testes...");
                    await LimparSchemaAsync(connection);

                    await connection.CloseAsync();
                    connected = true;
                    Console.WriteLine($"✅ Conexão com Oracle estabelecida e schema limpo!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️  Tentativa {attempt}/{maxAttempts} - Erro ao conectar: {ex.Message}");

                    if (attempt < maxAttempts)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2));
                    }
                }
            }

            if (!connected)
            {
                var errorMessage = @"
❌ NÃO FOI POSSÍVEL CONECTAR AO BANCO DE DADOS ORACLE PARA TESTES!

📋 INSTRUÇÕES PARA CONFIGURAR O BANCO DE TESTES:

1. Execute o script de configuração:
   .\src\Backend\FinTrack.Backend.IntegrationTests\setup-test-db.ps1

2. Aguarde o Oracle inicializar (1-2 minutos)

3. Execute os testes novamente:
   dotnet test src\Backend\FinTrack.Backend.IntegrationTests

💡 O script criará um container Docker com Oracle na porta 1523
";
                Console.WriteLine(errorMessage);
                throw new InvalidOperationException("Banco de dados Oracle para testes não está disponível. Execute o script setup-test-db.ps1 primeiro.");
            }

            _initialized = true;
        }
        finally
        {
            _lock.Release();
        }
    }

    private static readonly HashSet<string> TabelasAplicacao = new(StringComparer.OrdinalIgnoreCase)
    {
        "USERS",
        "TRANSACTIONS",
        "TRANSACTION_HISTORY",
        "CATEGORIES"
    };

    private static async Task LimparSchemaAsync(OracleConnection connection)
    {
        try
        {
            // Listar e dropar cada tabela da aplicação
            var tabelas = new List<string>();

            using (var cmdList = connection.CreateCommand())
            {
                cmdList.CommandText = "SELECT table_name FROM user_tables ORDER BY table_name";
                using var reader = await cmdList.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var tableName = reader.GetString(0);
                    if (TabelasAplicacao.Contains(tableName))
                    {
                        tabelas.Add(tableName);
                    }
                }
            }

            Console.WriteLine($"🔍 Limpeza inicial: {tabelas.Count} tabela(s) encontrada(s)");

            foreach (var tabela in tabelas)
            {
                try
                {
                    using var cmdDrop = connection.CreateCommand();
                    cmdDrop.CommandText = $"DROP TABLE \"{tabela}\" CASCADE CONSTRAINTS PURGE";
                    await cmdDrop.ExecuteNonQueryAsync();
                    Console.WriteLine($"  ✅ {tabela} removida");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ⚠️  Erro ao remover {tabela}: {ex.Message}");
                }
            }

            Console.WriteLine("✅ Schema limpo para testes");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Aviso ao limpar schema: {ex.Message}");
        }
    }

    public void Dispose()
    {
        // Não fazemos nada aqui pois o container é mantido entre execuções de teste
        // O usuário deve parar o container manualmente quando terminar os testes
    }
}
