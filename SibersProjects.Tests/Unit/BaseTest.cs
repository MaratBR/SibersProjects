using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SibersProjects.Configuration;
using SibersProjects.Dto;
using SibersProjects.Models;
using SibersProjects.Services;

namespace SibersProjects.Tests.Unit;

public class BaseTest
{
    protected readonly IServiceCollection ServiceCollection;

    private IServiceProvider? _serviceProvider;

    public BaseTest()
    {
        ServiceCollection = new ServiceCollection();
        ServiceCollection.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Testing.json")
            .Build());
        ServiceCollection.AddLogging();
        ServiceCollection.AddIdentity<User, Role>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
        ServiceCollection.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("Testing" + Guid.NewGuid()));
        ServiceCollection.AddApplicationServices();
        ServiceCollection.AddApplicationConfigurationSections();
        ServiceCollection.AddAutoMapper(typeof(AutoMapperProfile));
    }

    protected IServiceProvider ServiceProvider => _serviceProvider ??= ServiceCollection.BuildServiceProvider();
    protected AppDbContext DbContext => ServiceProvider.GetRequiredService<AppDbContext>();
}