using System.Net;
using System.Net.Http.Json;
using SibersProjects.Dto;
using SibersProjects.Tests.Integration.Fixtures;

namespace SibersProjects.Tests.Integration;

public class EmployeeTests : AuthenticationTestsBase
{
    public EmployeeTests(PlaygroundApplication application) : base(application)
    {
    }

    [Fact]
    public async Task TestNewEmployee()
    {
        await CreateDefaultUserAndLoginClient();
        var response = await Client.PostAsync("/api/Employees", JsonContent.Create(new
        {
            firstName = "1",
            lastName = "1",
            password = "Qwerty01$",
            userName = "NewUser",
            email = "qwert@qwer.ru"
        }));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var data = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(data);
        Assert.NotNull(data!.Id);
        Assert.Equal("NewUser", data.UserName);


    }

}