using AutoMapper;
using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Core.Models.CompaniesHouse;
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

            CreateMap<CompaniesHouseResponse, CompanyDetailsHaveNotChangedViewModel>()
                .ForMember(d => d.CompanyName, o => o.MapFrom(s => s.Organisation.Name))
                .ForMember(d => d.CompanyHouseNumber, o => o.MapFrom(s => s.Organisation.RegistrationNumber))
                .ForMember(d => d.SubBuildingName, o => o.MapFrom(s => s.Organisation.RegisteredOffice.SubBuildingName))
                .ForMember(d => d.BuildingName, o => o.MapFrom(s => s.Organisation.RegisteredOffice.BuildingName))
                .ForMember(d => d.BuildingNumber, o => o.MapFrom(s => s.Organisation.RegisteredOffice.BuildingNumber))
                .ForMember(d => d.Street, o => o.MapFrom(s => s.Organisation.RegisteredOffice.Street))
                .ForMember(d => d.Locality, o => o.MapFrom(s => s.Organisation.RegisteredOffice.Locality))
                .ForMember(d => d.DependentLocality, o => o.MapFrom(s => s.Organisation.RegisteredOffice.DependentLocality))
                .ForMember(d => d.Town, o => o.MapFrom(s => s.Organisation.RegisteredOffice.Town))
                .ForMember(d => d.County, o => o.MapFrom(s => s.Organisation.RegisteredOffice.County))
                .ForMember(d => d.Country, o => o.MapFrom(s => s.Organisation.RegisteredOffice.Country.Name))
                .ForMember(d => d.Postcode, o => o.MapFrom(s => s.Organisation.RegisteredOffice.Postcode))
                .ForMember(d => d.CompaniesHouseChangeDetailsUrl, o => o.MapFrom((src, dest, destMember, context) => context.Items["CompaniesHouseChangeDetailsUrl"]));
        }
    }
}