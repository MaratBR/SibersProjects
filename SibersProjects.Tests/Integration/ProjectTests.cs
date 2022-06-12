using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SibersProjects.Dto;
using SibersProjects.Models;
using SibersProjects.Services.ProjectService;
using SibersProjects.Services.UsersService;
using SibersProjects.Tests.Integration.Fixtures;
using SibersProjects.Utils;

namespace SibersProjects.Tests.Integration;

public class ProjectTests : AuthenticationTestsBase
{

    public async Task CreateNewProjectThroughApi()
    {
        var user = await CreateDefaultUserAndLoginClient();
        var response = await Client.PostAsync("api/Projects", JsonContent.Create(new NewProjectOptions()
        {
            Name = "Мой проект",
            Priority = 42,
            ProjectManagerId = user.Id,
            StartsAt = DateTime.Now,
            EndsAt = DateTime.Now.AddDays(1),
            ClientCompany = "12",
            ContractorCompany = "132",
        }));
        response.EnsureSuccessStatusCode();
        var context = Application.Services.GetRequiredService<AppDbContext>();
        Assert.Equal(1, await context.Projects.CountAsync());
    }

    [Fact]
    public async Task CreateAndGetNewProject()
    {
        await CreateNewProjectThroughApi();
        var response = await Client.GetAsync("api/Projects/1");
        response.EnsureSuccessStatusCode();
    }
    
    [Fact]
    public async Task ListManagedProjects()
    {
        var user = await CreateDefaultUserAndLoginClient();
        var context = Application.Services.GetRequiredService<AppDbContext>();
        context.Projects.Add(new Project
        {
            Name = "Test",
            ProjectManagerId = user.Id,
            ClientCompany = "12",
            ContractorCompany = "123",
            EndsAt = DateTime.Now.AddDays(1),
            StartsAt = DateTime.Now,
            Priority = 42
        });
        await context.SaveChangesAsync();
        var response = await Client.GetAsync("api/Projects/managed");
        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<Pagination<ProjectListItemDto>>();
        Assert.NotNull(data);
        Assert.Single(data!.Items);
    }
    
    [Fact]
    public async Task AssignToProject_DefaultSuperuser_ListAssignedProjects()
    {
        var user = await CreateDefaultUserAndLoginClient();
        var context = Application.Services.GetRequiredService<AppDbContext>();
        var project = new Project
        {
            Name = "Test",
            ProjectManagerId = user.Id,
            ClientCompany = "12",
            ContractorCompany = "123",
            EndsAt = DateTime.Now.AddDays(1),
            StartsAt = DateTime.Now,
            Priority = 42
        };
        context.Projects.Add(project);
        await context.SaveChangesAsync();
        var response = await Client.PostAsync($"api/Projects/{project.Id}/assignments", JsonContent.Create(new {userId=user.Id}));
        response.EnsureSuccessStatusCode();

        response = await Client.GetAsync($"api/Projects/assigned");;
        response.EnsureSuccessStatusCode();
        {
            var data = await response.Content.ReadFromJsonAsync<Pagination<ProjectListItemDto>>();
            Assert.NotNull(data);
            Assert.Single(data!.Items);
        }
    }

    [Fact]
    public async Task AssignToProject_Manager()
    {
        var usersService = Application.Services.GetRequiredService<IUsersService>();
        var user = await usersService.Create(new()
        {
            UserName = "Manager",
            FirstName = "Bob",
            LastName = "Whatever",
            Email = "it-does-not-matter@whatever.io",
            Password = "frtrhQFE#$-32",
            Roles = new List<string> { RoleNames.ProjectManager } // пусто
        });
        await LoginClient(user.UserName, "frtrhQFE#$-32");
        
        var context = Application.Services.GetRequiredService<AppDbContext>();
        var project = new Project
        {
            Name = "Test",
            ProjectManagerId = user.Id,
            ClientCompany = "12",
            ContractorCompany = "123",
            EndsAt = DateTime.Now.AddDays(1),
            StartsAt = DateTime.Now,
            Priority = 42
        };
        context.Projects.Add(project);
        await context.SaveChangesAsync();
        
        
        var otherUser = await usersService.Create(new()
        {
            UserName = "Guy",
            FirstName = "Bob",
            LastName = "Whatever",
            Email = "it-does-not-matter@whatever.io",
            Password = "frtrhQFE#$-32",
            // Roles = new List<string>() // пусто
        });

        var response = await Client.PostAsync($"api/Projects/{project.Id}/assignments", JsonContent.Create(new {userId=otherUser.Id}));
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task CannotAssignUser_NoRole()
    {
        var context = Application.Services.GetRequiredService<AppDbContext>();
        var usersService = Application.Services.GetRequiredService<IUsersService>();
        var user = await usersService.Create(new()
        {
            UserName = "IDontHaveRights",
            FirstName = "Bob",
            LastName = "Whatever",
            Email = "it-does-not-matter@whatever.io",
            Password = "frtrhQFE#$-32",
            // Roles = new List<string>() // пусто
        });
        
        // хотя менежер этого проекта - IDontHaveRights, у него нет роли менеджера
        var project = new Project
        {
            Name = "Test",
            ProjectManagerId = user.Id,
            ClientCompany = "12",
            ContractorCompany = "123",
            EndsAt = DateTime.Now.AddDays(1),
            StartsAt = DateTime.Now,
            Priority = 42
        };
        context.Projects.Add(project);
        await context.SaveChangesAsync();
        
        var otherUser = await usersService.Create(new()
        {
            UserName = "Guy",
            FirstName = "Bob",
            LastName = "Whatever",
            Email = "i-does-not-matter@whatever.io",
            Password = "frtrhQFE#$-32",
            // Roles = new List<string>() // пусто
        });

        await LoginClient("IDontHaveRights", "frtrhQFE#$-32");
        
        var response = await Client.PostAsync($"api/Projects/{project.Id}/assignments", JsonContent.Create(new {userId=otherUser.Id}));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    
    [Fact]
    public async Task CannotAssignUser_HasRoleButNotSetAsManager()
    {
        var context = Application.Services.GetRequiredService<AppDbContext>();
        var usersService = Application.Services.GetRequiredService<IUsersService>();
        var user = await usersService.Create(new()
        {
            UserName = "IHaveRightsButIAmNotTheManager",
            FirstName = "Bob",
            LastName = "Whatever",
            Email = "i-does-not-matter@whatever.io",
            Password = "frtrhQFE#$-32",
            Roles = new List<string>() // пусто
        });
        
        var project = new Project
        {
            Name = "Test",
            ProjectManagerId = user.Id,
            ClientCompany = "12",
            ContractorCompany = "123",
            EndsAt = DateTime.Now.AddDays(1),
            StartsAt = DateTime.Now,
            Priority = 42
        };
        context.Projects.Add(project);
        await context.SaveChangesAsync();
        
        var otherUser = await usersService.Create(new()
        {
            UserName = "Guy",
            FirstName = "Bob",
            LastName = "Whatever",
            Email = "i-does-not-matter@whatever.io",
            Password = "frtrhQFE#$-32",
            // Roles = new List<string>() // пусто
        });

        await LoginClient("Guy", "frtrhQFE#$-32");
        
        var response = await Client.PostAsync($"api/Projects/{project.Id}/assignments", JsonContent.Create(new {userId=otherUser.Id}));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    
    
    [Fact]
    public async Task CancelAssignment()
    {
        var user = await CreateDefaultUserAndLoginClient();
        var context = Application.Services.GetRequiredService<AppDbContext>();
        var project = new Project
        {
            Name = "Test",
            ProjectManagerId = user.Id,
            ClientCompany = "12",
            ContractorCompany = "123",
            EndsAt = DateTime.Now.AddDays(1),
            StartsAt = DateTime.Now,
            Priority = 42
        };
        context.Add(project);
        await context.SaveChangesAsync();
        context.Add(new ProjectAssignment
        {
            ProjectId = project.Id,
            EmployeeId = user.Id
        });
        await context.SaveChangesAsync();
        var response = await Client.DeleteAsync($"api/Projects/{project.Id}/assignments/{user.Id}");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task DeleteProject()
    {
        var user = await CreateDefaultUserAndLoginClient();
        var context = Application.Services.GetRequiredService<AppDbContext>();
        var project = new Project
        {
            Name = "Test",
            ProjectManagerId = user.Id,
            ClientCompany = "12",
            ContractorCompany = "123",
            EndsAt = DateTime.Now.AddDays(1),
            StartsAt = DateTime.Now,
            Priority = 42
        };
        context.Add(project);
        await context.SaveChangesAsync();
        var response = await Client.DeleteAsync($"api/Projects/{project.Id}");
        response.EnsureSuccessStatusCode();
        Assert.Equal(0, await context.Projects.CountAsync());
    }
    
    [Fact]
    public async Task ProjectCascadeDelete()
    {
        var user = await CreateDefaultUserAndLoginClient();
        var context = Application.Services.GetRequiredService<AppDbContext>();
        var project = new Project
        {
            Name = "Test",
            ProjectManagerId = user.Id,
            ClientCompany = "12",
            ContractorCompany = "123",
            EndsAt = DateTime.Now.AddDays(1),
            StartsAt = DateTime.Now,
            Priority = 42
        };
        context.Add(project);
        await context.SaveChangesAsync();
        context.Add(new ProjectAssignment
        {
            ProjectId = project.Id,
            EmployeeId = user.Id
        });
        await context.SaveChangesAsync();
        var response = await Client.DeleteAsync($"api/Projects/{project.Id}");
        response.EnsureSuccessStatusCode();
        Assert.Equal(0, await context.Projects.CountAsync());
        Assert.Equal(0, await context.Assignments.CountAsync());
    }

    [Fact]
    public async Task ProjectUpdate()
    {
        await CreateNewProjectThroughApi();

        var response = await Client.PatchAsync("api/Projects/1",
            JsonContent.Create(new { Name = "Test2", Priority = 24 }));
        response.EnsureSuccessStatusCode();

        var project = await Application.Services.GetRequiredService<AppDbContext>().Projects.Where(p => p.Id == 1).SingleAsync();
        Assert.Equal("Test2", project.Name);
        Assert.Equal(24, project.Priority);
    }
}
