using AutoMapper;
using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Core.Models;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;

namespace FrontendAccountManagement.Web.Profiles;

public class AccountManagementProfile : Profile
{
    public AccountManagementProfile()
    {
        CreateMap<UserData, EditUserDetailsViewModel>()
            .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FirstName))
            .ForMember(d => d.OriginalFirstName, o => o.MapFrom(s => s.FirstName))
            .ForMember(d => d.LastName, o => o.MapFrom(s => s.LastName))
            .ForMember(d => d.OriginalLastName, o => o.MapFrom(s => s.LastName))
            .ForMember(d => d.JobTitle, o => o.MapFrom(s => s.JobTitle))
            .ForMember(d => d.OriginalJobTitle, o => o.MapFrom(s => s.JobTitle))
            .ForMember(d => d.Telephone, o => o.MapFrom(s => s.Telephone))
            .ForMember(d => d.OriginalTelephone, o => o.MapFrom(s => s.Telephone));

        CreateMap<EditUserDetailsViewModel, UserDetailsUpdateModel>()
           .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FirstName))
           .ForMember(d => d.LastName, o => o.MapFrom(s => s.LastName))
           .ForMember(d => d.JobTitle, o => o.MapFrom(s => s.JobTitle))
           .ForMember(d => d.Telephone, o => o.MapFrom(s => s.Telephone));
    }
}