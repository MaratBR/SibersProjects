using SibersProjects.Services.ProjectService;
using SibersProjects.Services.TokenService;
using SibersProjects.Services.UsersService;

namespace SibersProjects.Services;

public static class ServiceCollectionExtensions
{
    public static void AddApplicationServices(this IServiceCollection serviceCollection)
    {
        serviceCollection 
            .AddScoped<ITokenService, TokenServiceImpl>()
            .AddScoped<IUsersService, UsersServiceImpl>()
            .AddScoped<IProjectService, ProjectServiceImpl>();
    }
}