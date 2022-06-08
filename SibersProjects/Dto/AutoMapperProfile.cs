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
        CreateMap<Project, ProjectDetailsDto>()
            .ForMember(
                p => p.Tasks,
                m => m.MapFrom(p => p.Tasks.Take(20)))
            .ForMember(
                p => p.Tasks,
                m => m.MapFrom(p => p.Tasks.Count));
        CreateMap<Project, ProjectBaseDto>();

        CreateMap<Role, string>().ConvertUsing(role => role.Name);
    }
}