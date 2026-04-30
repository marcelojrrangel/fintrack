# Script para configurar o banco de dados Oracle para testes de integração
# Execute este script UMA VEZ antes de rodar os testes

Write-Host "🚀 Configurando banco de dados Oracle para testes de integração..." -ForegroundColor Cyan

# Verificar se o container já existe
$existingContainer = docker ps -a --filter "name=fintrack-oracle-tests" --format "{{.Names}}"

if ($existingContainer) {
    Write-Host "⚠️  Container 'fintrack-oracle-tests' já existe. Removendo..." -ForegroundColor Yellow
    docker stop fintrack-oracle-tests 2>$null
    docker rm fintrack-oracle-tests 2>$null
}

Write-Host "📦 Criando container Oracle para testes..." -ForegroundColor Cyan
docker run -d `
    --name fintrack-oracle-tests `
    -p 1523:1521 `
    -e ORACLE_PASSWORD=TestPassword123 `
    gvenzl/oracle-free:23-slim

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Erro ao criar container Oracle" -ForegroundColor Red
    exit 1
}

Write-Host "⏳ Aguardando Oracle inicializar (isso pode levar 1-2 minutos)..." -ForegroundColor Yellow

# Aguardar até o Oracle estar pronto
$maxAttempts = 60
$attempt = 0
$ready = $false

while (-not $ready -and $attempt -lt $maxAttempts) {
    $attempt++
    Start-Sleep -Seconds 2

    # Verificar se o container está saudável
    $health = docker inspect fintrack-oracle-tests --format='{{.State.Health.Status}}' 2>$null

    if ($health -eq "healthy") {
        $ready = $true
    }
    elseif ($attempt % 10 -eq 0) {
        Write-Host "⏳ Tentativa $attempt/$maxAttempts - Oracle ainda está inicializando..." -ForegroundColor Yellow
    }
}

if (-not $ready) {
    Write-Host "❌ Timeout: Oracle não ficou pronto após $maxAttempts tentativas" -ForegroundColor Red
    Write-Host "💡 Verifique os logs com: docker logs fintrack-oracle-tests" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Container Oracle criado e inicializado com sucesso!" -ForegroundColor Green
Write-Host "" -ForegroundColor White
Write-Host "📊 Informações de Conexão:" -ForegroundColor Cyan
Write-Host "  Host: localhost" -ForegroundColor White
Write-Host "  Port: 1523" -ForegroundColor White
Write-Host "  Service: FREEPDB1" -ForegroundColor White
Write-Host "  User: system" -ForegroundColor White
Write-Host "  Password: TestPassword123" -ForegroundColor White
Write-Host "" -ForegroundColor White
Write-Host "🎯 Connection String:" -ForegroundColor Cyan
Write-Host "  User Id=system;Password=TestPassword123;Data Source=localhost:1523/FREEPDB1;" -ForegroundColor White
Write-Host "" -ForegroundColor White
Write-Host "✅ Pronto! Agora você pode executar os testes de integração com:" -ForegroundColor Green
Write-Host "   dotnet test src\Backend\FinTrack.Backend.IntegrationTests" -ForegroundColor Yellow
Write-Host "" -ForegroundColor White
Write-Host "🛑 Para parar o container após os testes:" -ForegroundColor Cyan
Write-Host "   docker stop fintrack-oracle-tests" -ForegroundColor White
Write-Host "   docker rm fintrack-oracle-tests" -ForegroundColor White
