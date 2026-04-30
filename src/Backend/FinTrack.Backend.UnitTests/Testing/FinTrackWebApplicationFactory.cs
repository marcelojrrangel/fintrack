using FinTrack.Application.Common.Abstractions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FinTrack.Backend.UnitTests.Testing;

public sealed class FinTrackWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDatabaseInitializer>();
            services.AddSingleton<IDatabaseInitializer, NoOpDatabaseInitializer>();
        });
    }
}

public sealed class DevelopmentFinTrackWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDatabaseInitializer>();
            services.AddSingleton<IDatabaseInitializer, NoOpDatabaseInitializer>();
        });
    }
}
