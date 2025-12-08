using AutoMapper;
using IDP.Infrastructure.Entities;
using IDP.Infrastructure.ViewModels;

namespace IDP.Profiles;

public class PermissionProfile : Profile
{
    public PermissionProfile()
    {
        CreateMap<Permission, PermissionUserViewModel>();
    }
}
