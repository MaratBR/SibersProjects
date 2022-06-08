using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SibersProjects.Models;
using SibersProjects.Services.ProjectService;
using SibersProjects.Services.TaskService;
using SibersProjects.Services.TaskService.Exceptions;
using SibersProjects.Services.UsersService;

namespace SibersProjects.Tests.Unit;

public class TaskTests : BaseTest
{
    [Fact]
    public async Task CreateTask()
    {
        var projectService = ServiceProvider.GetRequiredService<IProjectService>();
        var taskService = ServiceProvider.GetRequiredService<ITaskService>();
        var user = await ServiceProvider.GetRequiredService<IUsersService>().GetOrCreateDefaultUser();

        var project = await projectService.Create(new ()
        {
            Name = "Test",
            ClientCompany = "1",
            ContractorCompany = "1",
            EndsAt = DateTime.Now.AddDays(1),
            StartsAt = DateTime.Now,
            ProjectManagerId = user.Id,
            Priority = 1,
        });

        await taskService.Create(user.Id, new()
        {
            Description = "12",
            Name = "New task",
            Priority = 9000,
            ProjectId = project.Id
        });

        var context = ServiceProvider.GetRequiredService<AppDbContext>();
        var task = await context.Tasks.SingleAsync();
        Assert.Equal(project.Id, task.ProjectId);
        Assert.Equal("New task", task.Name);
        Assert.Equal(9000, task.Priority);
    }

    [Fact]
    public async Task AssignTask()
    {
        var projectService = ServiceProvider.GetRequiredService<IProjectService>();
        var taskService = ServiceProvider.GetRequiredService<ITaskService>();
        var user = await ServiceProvider.GetRequiredService<IUsersService>().GetOrCreateDefaultUser();
        
        var project = await projectService.Create(new ()
        {
            Name = "Test",
            ClientCompany = "1",
            ContractorCompany = "1",
            EndsAt = DateTime.Now.AddDays(1),
            StartsAt = DateTime.Now,
            ProjectManagerId = user.Id,
            Priority = 1,
        });

        var task = await taskService.Create(user.Id,
        new() {
            Description = "123",
            Name = "123",
            Priority = 1,
            ProjectId = project.Id
        });
        
        
        await Assert.ThrowsAsync<InvalidAssigneeException>(() => taskService.AssignTo(task, user.Id));
        await projectService.AssignEmployee(user, project);
        await taskService.AssignTo(task, user.Id);
    }

    [Fact]
    public async Task CreateInvalidTask()
    {
        var user = await ServiceProvider.GetRequiredService<IUsersService>().GetOrCreateDefaultUser();
        var taskService = ServiceProvider.GetRequiredService<ITaskService>();
        await Assert.ThrowsAsync<TaskException>(() => taskService.Create(user.Id, new NewTaskData
        {
            Description = "123",
            Name = "234",
            ProjectId = 1000
        }));
    }
    
}