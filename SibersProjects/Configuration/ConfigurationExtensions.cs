namespace SibersProjects.Configuration;

public static class ConfigurationExtensions
{
    public static void AddApplicationConfigurationSections(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped(provider =>
            provider.GetRequiredService<IConfiguration>().GetSection("JwtSettings").Get<JwtSettings>());
    }
}