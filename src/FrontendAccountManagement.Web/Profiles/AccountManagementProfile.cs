using AutoMapper;
using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;

namespace FrontendAccountManagement.Web.Profiles
{
    public class AccountManagementProfile : Profile
    {
        public AccountManagementProfile()
        {
            CreateMap<UserData, EditUserDetailsViewModel>()
                .ForMember(d => d.JobTitle, o => o.MapFrom(s => s.JobTitle))
                .ForMember(d => d.OriginalJobTitle, o => o.MapFrom(s => s.JobTitle))
                .ForMember(d => d.Telephone, o => o.MapFrom(s => s.Telephone))
                .ForMember(d => d.OriginalTelephone, o => o.MapFrom(s => s.Telephone));
        }
    }
}