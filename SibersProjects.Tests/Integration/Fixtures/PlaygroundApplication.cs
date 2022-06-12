using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SibersProjects.Models;

namespace SibersProjects.Tests.Integration.Fixtures;

public class PlaygroundApplication : WebApplicationFactory<Program>
{
    private readonly Guid _appId = Guid.NewGuid();

    private string GetSqliteFile(Guid id)
    {
        var folder = Path.Join(Path.GetTempPath(), "SibersProjectsTesting");
        Directory.CreateDirectory(folder);
        var file = Path.Join(folder, $"test_{id}.sqlite3");
        return file;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var dbFile = GetSqliteFile(_appId);
        builder.UseEnvironment("Testing");

        // Add mock/test services to the builder here
        builder.ConfigureServices(services =>
        {
            services.AddMvcCore().AddApplicationPart(typeof(Program).Assembly);
            services.AddScoped(sp => new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={dbFile}")
                .UseApplicationServiceProvider(sp)
                .Options);
        });

        var host = base.CreateHost(builder);

        using (var serviceScope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var context = serviceScope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.EnsureCreated();
        }

        return host;
    }
}