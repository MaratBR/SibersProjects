using System.ComponentModel.DataAnnotations;

namespace SibersProjects.Services.Common;

public class DefaultPaginationOptions : IPaginationOptions
{
    [Range(20, 50)] public int PageSize { get; set; } = 20;
    [Range(1, 1000)] public int Page { get; set; } = 1;
}