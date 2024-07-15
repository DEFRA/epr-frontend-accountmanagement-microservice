using AutoMapper;
using FrontendAccountManagement.Core.Models.CompaniesHouse;
using FrontendAccountManagement.Web.Profiles;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;

namespace FrontendAccountManagement.Web.UnitTests.Profiles
{
    [TestClass]
    public class CompaniesHouseResponseMapperTests
    {
        private IMapper _mapper;

        [TestInitialize]
        public void Setup()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CompaniesHouseResponseMapper>();
            });

            _mapper = config.CreateMapper();
        }

        [TestMethod]
        public void CompaniesHouseResponseToConfirmCompanyDetailsViewModel_ShouldMapCorrectly()
        {
            // Arrange
            var companiesHouseResponse = new CompaniesHouseResponse
            {
                Organisation = new OrganisationDto
                {
                    Name = "Test Company",
                    RegistrationNumber = "12345678",
                    RegisteredOffice = new AddressDto
                    {
                        SubBuildingName = "SubBuilding",
                        BuildingName = "Building",
                        BuildingNumber = "42",
                        Street = "Test Street",
                        Town = "Test Town",
                        County = "Test County",
                        Postcode = "12345",
                        Locality = "Test Locality",
                        DependentLocality = "Test Dependent Locality",
                        Country = new CountryDto
                        {
                            Name = "Test Country",
                            Iso = "TC"
                        }
                    }
                }
            };

            // Act
            var viewModel = _mapper.Map<ConfirmCompanyDetailsViewModel>(companiesHouseResponse);

            // Assert
            Assert.IsNotNull(viewModel);
            Assert.AreEqual("Test Company", viewModel.CompanyName);
            Assert.AreEqual("12345678", viewModel.CompaniesHouseNumber);
            Assert.IsNotNull(viewModel.BusinessAddress);
            Assert.AreEqual("SubBuilding", viewModel.BusinessAddress.SubBuildingName);
            Assert.AreEqual("Building", viewModel.BusinessAddress.BuildingName);
            Assert.AreEqual("42", viewModel.BusinessAddress.BuildingNumber);
            Assert.AreEqual("Test Street", viewModel.BusinessAddress.Street);
            Assert.AreEqual("Test Town", viewModel.BusinessAddress.Town);
            Assert.AreEqual("Test County", viewModel.BusinessAddress.County);
            Assert.AreEqual("12345", viewModel.BusinessAddress.Postcode);
            Assert.AreEqual("Test Locality", viewModel.BusinessAddress.Locality);
            Assert.AreEqual("Test Dependent Locality", viewModel.BusinessAddress.DependentLocality);
            Assert.AreEqual("Test Country", viewModel.BusinessAddress.Country);
        }

        [TestMethod]
        public void AddressDtoToAddressViewModel_ShouldMapCorrectly()
        {
            // Arrange
            var addressDto = new AddressDto
            {
                SubBuildingName = "SubBuilding",
                BuildingName = "Building",
                BuildingNumber = "42",
                Street = "Test Street",
                Town = "Test Town",
                County = "Test County",
                Postcode = "12345",
                Locality = "Test Locality",
                DependentLocality = "Test Dependent Locality",
                Country = new CountryDto
                {
                    Name = "Test Country",
                    Iso = "TC"
                }
            };

            // Act
            var addressViewModel = _mapper.Map<AddressViewModel>(addressDto);

            // Assert
            Assert.IsNotNull(addressViewModel);
            Assert.AreEqual("SubBuilding", addressViewModel.SubBuildingName);
            Assert.AreEqual("Building", addressViewModel.BuildingName);
            Assert.AreEqual("42", addressViewModel.BuildingNumber);
            Assert.AreEqual("Test Street", addressViewModel.Street);
            Assert.AreEqual("Test Town", addressViewModel.Town);
            Assert.AreEqual("Test County", addressViewModel.County);
            Assert.AreEqual("12345", addressViewModel.Postcode);
            Assert.AreEqual("Test Locality", addressViewModel.Locality);
            Assert.AreEqual("Test Dependent Locality", addressViewModel.DependentLocality);
            Assert.AreEqual("Test Country", addressViewModel.Country);
        }
    }
}