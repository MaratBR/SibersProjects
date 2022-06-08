using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SibersProjects.Services.ProjectService;
using SibersProjects.Services.ProjectService.Exceptions;
using SibersProjects.Services.UsersService;

namespace SibersProjects.Tests.Unit;

public class ProjectTests : BaseTest
{
    [Fact]
    public async Task TestNewProject()
    {
        var usersService = ServiceProvider.GetRequiredService<IUsersService>();
        var user = await usersService.GetOrCreateDefaultUser();
        var projectService = ServiceProvider.GetRequiredService<IProjectService>();
        await projectService.Create(new NewProjectOptions
        {
            Name = "my project",
            StartsAt = DateTime.Now,
            EndsAt = DateTime.Now.AddDays(1),
            ClientCompany = "A",
            ContractorCompany = "B",
            ProjectManagerId = user.Id
        });
        Assert.Equal(1, await DbContext.Projects.CountAsync());
    }
    
    [Fact]
    public async Task TestNewProjectDateValidation()
    {
        var usersService = ServiceProvider.GetRequiredService<IUsersService>();
        var user = await usersService.GetOrCreateDefaultUser();
        var projectService = ServiceProvider.GetRequiredService<IProjectService>();
        await Assert.ThrowsAsync<InvalidProjectTimeSpan>(async () =>
        {
            await projectService.Create(new NewProjectOptions
            {
                Name = "my project",
                EndsAt = DateTime.Now,
                StartsAt = DateTime.Now.AddDays(1),
                ClientCompany = "A",
                ContractorCompany = "B",
                ProjectManagerId = user.Id
            });
        });
    }
}