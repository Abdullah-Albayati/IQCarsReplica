using AutoMapper;

namespace Template.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Add CreateMap<Source, Destination>() definitions here.
        CreateMap<Template.Entities.CarEntity.Car, Template.DTOs.CarDto>();
        CreateMap<Template.DTOs.CarForm, Template.Entities.CarEntity.Car>();
        CreateMap<Template.DTOs.CarUpdate, Template.Entities.CarEntity.Car>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Template.Entities.User.User, Template.DTOs.UserDto>()
            .ForMember(dest => dest.Role, opts => opts.MapFrom(src => src.Role.ToString()));

        CreateMap<Template.DTOs.UserForm, Template.Entities.User.User>()
            .ForMember(dest => dest.PasswordHash, opts => opts.Ignore());

        CreateMap<Template.DTOs.UserUpdate, Template.Entities.User.User>()
            .ForMember(dest => dest.Role, opts =>
            {
                opts.PreCondition(src =>
                    !string.IsNullOrWhiteSpace(src.Role)
                    && Enum.TryParse<Template.Entities.User.User.UserRoles>(src.Role, true, out _));
                opts.MapFrom(src => Enum.Parse<Template.Entities.User.User.UserRoles>(src.Role!, true));
            })
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        
      
    }
}
