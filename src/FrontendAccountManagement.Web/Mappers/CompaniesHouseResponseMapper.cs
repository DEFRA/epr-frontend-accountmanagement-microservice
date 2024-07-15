using AutoMapper;
using FrontendAccountManagement.Core.Models.CompaniesHouse;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;

namespace FrontendAccountManagement.Web.Mappers
{
    internal class CompaniesHouseResponseMapper : Profile
    {
        public CompaniesHouseResponseMapper()
        {
            CreateMap<CompaniesHouseResponse, ConfirmCompanyDetailsViewModel>()
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Organisation.Name))
                .ForMember(dest => dest.CompaniesHouseNumber, opt => opt.MapFrom(src => src.Organisation.RegistrationNumber))
                .ForMember(dest => dest.BusinessAddress, opt => opt.MapFrom(src => src.Organisation.RegisteredOffice))
                .ForMember(dest => dest.ExternalCompanyHouseChangeRequestLink, opt => opt.Ignore());

            // Mapping from AddressDto to AddressViewModel
            CreateMap<AddressDto, AddressViewModel>()
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country.Name));
        }
    }
}
