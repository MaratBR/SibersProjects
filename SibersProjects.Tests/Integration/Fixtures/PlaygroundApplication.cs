using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SibersProjects.Models;

namespace SibersProjects.Tests.Integration.Fixtures;

public class PlaygroundApplication : WebApplicationFactory<Program>
{

    public PlaygroundApplication()
    {
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Add mock/test services to the builder here
        builder.ConfigureServices(services =>
        {
            services.AddMvcCore().AddApplicationPart(typeof(Program).Assembly);
            services.AddScoped(sp => new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("Testing")
                .UseApplicationServiceProvider(sp)
                .Options);
        });

        return base.CreateHost(builder);
    }
}
