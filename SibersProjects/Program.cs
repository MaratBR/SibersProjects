using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SibersProjects.Configuration;
using SibersProjects.Dto;
using SibersProjects.Models;
using SibersProjects.Services;
using SibersProjects.Services.UsersService;
using SibersProjects.Services.ProjectService;
using SibersProjects.Services.TokenService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
builder.Services.AddApplicationServices();
builder.Services.AddApplicationConfigurationSections();

builder.Services
    .AddIdentity<User, Role>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.ClaimsIssuer = jwtSettings.Issuer;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidateAudience = false,
            ValidAlgorithms = new []{"HS256"},
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        .Build();
});
    

var sqliteConnectionString = builder.Configuration.GetConnectionString("SQLite") ?? "DefaultDatabase.sqlite3";
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite(sqliteConnectionString);
;});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapFallbackToFile("index.html");
});

using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
    var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
    
}


app.Run();


// для тестов: https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-6.0&viewFallbackFrom=aspnetcore-3.0#basic-tests-with-the-default-webapplicationfactory
public partial class Program { }
