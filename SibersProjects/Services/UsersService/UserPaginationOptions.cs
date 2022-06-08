using System.ComponentModel.DataAnnotations;

namespace SibersProjects.Services.UsersService;

public class UserPaginationOptions : UserFilterOptions
{
    [Range(10, 50)] public int PageSize { get; set; } = 20;
    [Range(1, 1000)] public int Page { get; set; } = 1;
}