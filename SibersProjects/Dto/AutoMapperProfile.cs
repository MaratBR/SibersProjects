using SibersProjects.Models;

namespace SibersProjects.Dto;

public class AutoMapperProfile : AutoMapper.Profile
{
    public AutoMapperProfile()
    {
        CreateMap<User, UserDto>();

        CreateMap<Project, ProjectListItemDto>()
            .ForMember(
                p => p.EmployeesTotal, 
                m => m.MapFrom(p => p.Employees.Count));
        CreateMap<Project, ProjectDetailsDto>();

        CreateMap<Role, string>().ConvertUsing(role => role.Name);
    }
}