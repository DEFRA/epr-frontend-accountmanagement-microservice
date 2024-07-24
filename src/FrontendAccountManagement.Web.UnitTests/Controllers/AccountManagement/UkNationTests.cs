using AutoFixture;
using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Core.Models.CompaniesHouse;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Constants.Enums;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement;

[TestClass]
public class UkNationTests : AccountManagementTestBase
{
    private UserData _userData;
    private CompaniesHouseResponse _companiesHouseResponse;
    private ConfirmCompanyDetailsViewModel _viewModel;
    private Mock<HttpContext>? _httpContextMock;

    private Fixture _fixture = new Fixture();

    [TestInitialize]
    public void Setup()
    {
        _userData = _fixture.Create<UserData>();
        _companiesHouseResponse = _fixture.Create<CompaniesHouseResponse>();
        _viewModel = _fixture.Create<ConfirmCompanyDetailsViewModel>();
        _httpContextMock = new Mock<HttpContext>();

        SetupBase(_userData);

        FacadeServiceMock.Setup(x => x.GetCompaniesHouseResponseAsync(It.IsAny<string>()))
            .ReturnsAsync(_companiesHouseResponse);

        AutoMapperMock.Setup(x => x.Map<ConfirmCompanyDetailsViewModel>(_companiesHouseResponse))
            .Returns(_viewModel);
    }


    [TestMethod]
    public async Task UkNation_ShouldReturnViewWithModel_WhenModelStateIsInvalid()
    {
        // Arrange
        var model = new UkNationViewModel { UkNation = null };
        SystemUnderTest.ModelState.AddModelError("UkNation", "Required");

        // Act
        var result = await SystemUnderTest.UkNation(model);

        // Assert
        var viewResult = result as ViewResult;

        Assert.AreEqual(model, viewResult.Model);
    }

    [TestMethod]
    public async Task UkNation_ShouldRedirectToUkNation_WhenModelStateIsValid()
    {
        // Arrange
        var model = new UkNationViewModel { UkNation = UkNation.England };
        var session = _fixture.Create<JourneySession>();

        session.CompaniesHouseSession = new CompaniesHouseSession
        {
            CompaniesHouseData = new CompaniesHouseResponse
            {
                Organisation = new OrganisationDto
                {
                    RegisteredOffice = _fixture.Create<AddressDto>()
                }
            }
        };

        SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(session);

        AutoMapperMock.Setup(m => m.Map<AddressViewModel>(It.IsAny<AddressDto>()))
            .Returns(_fixture.Create<AddressViewModel>());
      
        // Act
        var result = await SystemUnderTest.UkNation(model);

        // Assert
        var redirectToActionResult = result as RedirectToActionResult;
        Assert.AreEqual(nameof(SystemUnderTest.CheckCompaniesHouseDetails), redirectToActionResult.ActionName);
    }
}