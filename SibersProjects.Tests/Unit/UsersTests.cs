using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SibersProjects.Services.UsersService;
using SibersProjects.Services.UsersService.Exceptions;

namespace SibersProjects.Tests.Unit;

public class UsersTests : BaseTest
{
    [Fact]
    public async Task TestCreateNewUser()
    {
        await ServiceProvider.GetRequiredService<IUsersService>().Create(new NewUserOptions
        {
            Email = "user@user.net",
            FirstName = "John",
            LastName = "Smith",
            Password = "1234$%Qwerty",
            UserName = "user"
        });
        Assert.Equal(1, await DbContext.Users.CountAsync());
    }
    
    [Fact]
    public async Task TestNoDuplicateUsers()
    {
        var newUser = new NewUserOptions
        {
            Email = "user@user.net",
            FirstName = "John",
            LastName = "Smith",
            Password = "1234$%Qwerty",
            UserName = "user"
        };
        var usersService = ServiceProvider.GetRequiredService<IUsersService>();
        await usersService.Create(newUser);
        await Assert.ThrowsAsync<IdentityUserException>(async () =>
        {
            await usersService.Create(newUser);
        });
        Assert.Equal(1, await DbContext.Users.CountAsync());
    }
}