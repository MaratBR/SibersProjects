using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SibersProjects.Models;

namespace SibersProjects.Tests.Integration.Fixtures;

public class PlaygroundApplication : WebApplicationFactory<Program>
{
    private readonly Guid _appId = Guid.NewGuid();
    
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        var dbFile = Path.GetTempFileName();

        // Add mock/test services to the builder here
        builder.ConfigureServices(services =>
        {
            services.AddMvcCore().AddApplicationPart(typeof(Program).Assembly);
            services.AddScoped(sp => new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={dbFile}")
                .UseApplicationServiceProvider(sp)
                .Options);
        });

        return base.CreateHost(builder);
    }
}
