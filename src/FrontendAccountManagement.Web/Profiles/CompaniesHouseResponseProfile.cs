using AutoMapper;
using FrontendAccountManagement.Core.Models.CompaniesHouse;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;

namespace FrontendAccountManagement.Web.Profiles;

public class CompaniesHouseResponseProfile : Profile
{
    public CompaniesHouseResponseProfile()
    {
        CreateMap<CompaniesHouseResponse, CompanyDetailsHaveNotChangedViewModel>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CompanyName, o => o.MapFrom(s => s.Organisation.Name))
            .ForMember(d => d.TradingName, o => o.MapFrom(s => s.Organisation.TradingName))
            .ForMember(d => d.RegistrationNumber, o => o.MapFrom(s => s.Organisation.RegistrationNumber))
            .ForMember(d => d.CompaniesHouseNumber, o => o.MapFrom(s => s.Organisation.RegistrationNumber))
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

        CreateMap<CompaniesHouseResponse, ConfirmCompanyDetailsViewModel>()
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Organisation.Name))
            .ForMember(dest => dest.CompaniesHouseNumber, opt => opt.MapFrom(src => src.Organisation.RegistrationNumber))
            .ForMember(dest => dest.BusinessAddress, opt => opt.MapFrom(src => src.Organisation.RegisteredOffice))
            .ForMember(dest => dest.ExternalCompanyHouseChangeRequestLink, opt => opt.Ignore());

        CreateMap<AddressDto, AddressViewModel>()
            .ForMember(dest => dest.AddressFields, opt => opt.Ignore())
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country.Name));
    }
}
