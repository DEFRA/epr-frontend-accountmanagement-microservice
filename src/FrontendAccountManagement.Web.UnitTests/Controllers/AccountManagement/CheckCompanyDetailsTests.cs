using FluentAssertions;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Controllers.AccountManagement;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using EPR.Common.Authorization.Models;
using AutoFixture;
using FrontendAccountManagement.Web.Controllers.Errors;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using FrontendAccountManagement.Core.Enums;
using FrontendAccountManagement.Core.Models;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using FrontendAccountManagement.Core.Services.Dto.CompaniesHouse;
using System.IO;
using FluentAssertions.Execution;
using System.Net;
using FrontendAccountManagement.Web.Configs;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement;

[TestClass]
public class CheckCompanyDetailsTests : AccountManagementTestBase
{
    private UserData _userData;    
    private JourneySession _journeySession;
    private Fixture _fixture = new Fixture();

    [TestInitialize]
    public void Setup()
    {
        _userData = _fixture.Create<UserData>();
        _userData.Organisations[0].CompaniesHouseNumber = null;

        SetupBase(_userData);

        _journeySession = new JourneySession
        {
            UserData = _userData,
            AccountManagementSession = new AccountManagementSession()
            {
                Journey = new List<string> {
                PagePath.UpdateCompanyAddress, PagePath.CheckCompanyDetails
            },
                BusinessAddress = new FrontendAccountManagement.Core.Addresses.Address()
            }            
        };

        SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_journeySession);
    }

    [TestMethod]
    public async Task GivenOrganisationNameModified_WhenCheckCompanyDetailsPostCalled_ThenRedirectToCompanyDetailsUpdatedPage_FacadeUpdate_AndClearSession()
    {
        // Arrange
        _journeySession.AccountManagementSession.IsUpdateCompanyName = true;
        _journeySession.AccountManagementSession.OrganisationName = "New Name";

        FacadeServiceMock.Setup(mock => mock.GetUserAccount())
         .Returns(Task.FromResult(new UserAccountDto
         {

         }));

        // Act
        var result = await SystemUnderTest.CheckCompanyDetailsPost();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountManagementController.CompanyDetailsUpdated));

        FacadeServiceMock.Verify(
             x => x.UpdateOrganisationDetails(
                 It.IsAny<Guid>(),
                 It.Is<OrganisationUpdateDto>(dto => dto.Name == "New Name")
             ),
             Times.Once
         );

        SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
    }

    [TestMethod]
    public async Task GivenAddressModified_WhenCheckCompanyDetailsPostCalled_ThenRedirectToCompanyDetailsUpdatedPage_FacadeUpdate_AndClearSession()
    {
        // Arrange
        _journeySession.AccountManagementSession.IsUpdateCompanyAddress = true;
        _journeySession.AccountManagementSession.IsUpdateCompanyName = false;
        _journeySession.AccountManagementSession.OrganisationName = "New Name";
        _journeySession.AccountManagementSession.BusinessAddress = new Core.Addresses.Address
        {
            BuildingName = "BuildingName",
            BuildingNumber = "BuildingNumber",
            Country = "BuildingNumber",
            County = "County",
            DependentLocality = "DependentLocality",
            Locality = "Locality",
            Postcode = "Postcode",
            Street = "Street",
            SubBuildingName = "SubBuildingName",
            Town = "Town"
        };
        _journeySession.AccountManagementSession.UkNation = Core.Enums.Nation.England;

        FacadeServiceMock.Setup(mock => mock.GetUserAccount())
         .Returns(Task.FromResult(new UserAccountDto
         {

         }));

        // Act
        var result = await SystemUnderTest.CheckCompanyDetailsPost();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountManagementController.CompanyDetailsUpdated));

        FacadeServiceMock.Verify(
             x => x.UpdateOrganisationDetails(
                 It.IsAny<Guid>(),
                 It.Is<OrganisationUpdateDto>(dto => dto.Name == "New Name")
             ),
             Times.Never
         );

        FacadeServiceMock.Verify(
             x => x.UpdateOrganisationDetails(
                 It.IsAny<Guid>(),
                 It.Is<OrganisationUpdateDto>(dto => 
                 dto.BuildingName == "BuildingName" &&
                    dto.BuildingNumber == "BuildingNumber" &&
                    dto.Country == "BuildingNumber" &&
                    dto.County == "County" &&
                    dto.DependentLocality == "DependentLocality" &&
                    dto.Locality == "Locality" &&
                    dto.Postcode == "Postcode" &&
                    dto.Street == "Street" &&
                    dto.SubBuildingName == "SubBuildingName" &&
                    dto.Town == "Town"
                 )
             ),
             Times.Once
         );

        SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
    }

    [TestMethod]
    public async Task GivenAddressAndOrganisationNameModified_WhenCheckCompanyDetailsPostCalled_ThenRedirectToCompanyDetailsUpdatedPage_FacadeUpdate_AndClearSession()
    {
        // Arrange
        _journeySession.AccountManagementSession.IsUpdateCompanyAddress = true;
        _journeySession.AccountManagementSession.IsUpdateCompanyName = true;
        _journeySession.AccountManagementSession.OrganisationName = "New Name";
        _journeySession.AccountManagementSession.BusinessAddress = new Core.Addresses.Address
        {
            BuildingName = "BuildingName",
            BuildingNumber = "BuildingNumber",
            Country = "BuildingNumber",
            County = "County",
            DependentLocality = "DependentLocality",
            Locality = "Locality",
            Postcode = "Postcode",
            Street = "Street",
            SubBuildingName = "SubBuildingName",
            Town = "Town"
        };
        _journeySession.AccountManagementSession.UkNation = Core.Enums.Nation.England;

        FacadeServiceMock.Setup(mock => mock.GetUserAccount())
         .Returns(Task.FromResult(new UserAccountDto
         {

         }));

        // Act
        var result = await SystemUnderTest.CheckCompanyDetailsPost();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountManagementController.CompanyDetailsUpdated));

        FacadeServiceMock.Verify(
             x => x.UpdateOrganisationDetails(
                 It.IsAny<Guid>(),
                 It.Is<OrganisationUpdateDto>(dto =>
                    dto.Name == "New Name" &&
                    dto.BuildingName == "BuildingName" &&
                    dto.BuildingNumber == "BuildingNumber" &&
                    dto.Country == "BuildingNumber" &&
                    dto.County == "County" &&
                    dto.DependentLocality == "DependentLocality" &&
                    dto.Locality == "Locality" &&
                    dto.Postcode == "Postcode" &&
                    dto.Street == "Street" &&
                    dto.SubBuildingName == "SubBuildingName" &&
                    dto.Town == "Town"
                 )
             ),
             Times.Once
         );

        SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
    }

    [TestMethod]
    public async Task GivenAddressAndOrganisationNameNotModified_WhenCheckCompanyDetailsPostCalled_ThenRedirectToCompanyDetailsUpdatedPage_FacadeUpdate_AndClearSession()
    {
        // Arrange
        _journeySession.AccountManagementSession.IsUpdateCompanyName = false;
        _journeySession.AccountManagementSession.IsUpdateCompanyAddress = false;

        FacadeServiceMock.Setup(mock => mock.GetUserAccount())
         .Returns(Task.FromResult(new UserAccountDto
         {

         }));

        // Act
        var result = await SystemUnderTest.CheckCompanyDetailsPost();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(PagePath.Error);

        FacadeServiceMock.Verify(
             x => x.UpdateOrganisationDetails(
                 It.IsAny<Guid>(),
                 It.IsAny<OrganisationUpdateDto>()
             ),
             Times.Never
         );

        SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Never);
    }

    [TestMethod]
    public async Task GivenPreviousPageUpdateCompanyAddress_WhenCheckCompanyDetailsCalled_ThenCheckCompanyDetailsPageReturned_WithUpdateCompanyAddressAsTheBackLink()
    {
        //Arrange
        _journeySession.AccountManagementSession.BusinessAddress = null;

        //Act
        var result = await SystemUnderTest.CheckCompanyDetails();

        //Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<CheckCompanyDetailsViewModel>();

        AssertBackLink(viewResult, PagePath.UpdateCompanyAddress);
    }

    [TestMethod]
    public async Task GivenPreviousPageNonCompaniesHouseUkNation_WhenCheckCompanyDetailsCalled_ThenCheckCompanyDetailsPageReturned_WithNonCompaniesHouseUkNationAsTheBackLink()
    {
        //Arrange
        _journeySession.AccountManagementSession.Journey = new List<string>{
                PagePath.NonCompaniesHouseUkNation, PagePath.CheckCompanyDetails
            };

        //Act
        var result = await SystemUnderTest.CheckCompanyDetails();

        //Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<CheckCompanyDetailsViewModel>();

        AssertBackLink(viewResult, PagePath.NonCompaniesHouseUkNation);
    }

    [TestMethod]
    public async Task GivenPreviousPageBusinessAddress_WhenCheckCompanyDetailsCalled_ThenCheckCompanyDetailsPageReturned_WithNonCompaniesHouseUkNationAsTheBackLink()
    {
        //Arrange
        _journeySession.AccountManagementSession.Journey = new List<string>{
                PagePath.BusinessAddress, PagePath.CheckCompanyDetails
            };

        //Act
        var result = await SystemUnderTest.CheckCompanyDetails();

        //Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(PagePath.Error);
    }


    [TestMethod]
    public async Task GivenNoPreviousPage_WhenCheckCompanyDetailsCalled_ThenRedirectToError()
    {
        //Arrange
        _journeySession.AccountManagementSession.Journey = new List<string>();

        //Act
        var result = await SystemUnderTest.CheckCompanyDetails();

        //Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(PagePath.Error);
    }

    [TestMethod]
    public async Task GivenCompaniesHouseUser_WhenGetCheckCompanyDetailsCalled_ThenRedirectToErrorPage()
    {
        // Arrange
        _userData.Organisations[0].CompaniesHouseNumber = "123";
        SetupBase(_userData);

        // Act
        var result = await SystemUnderTest.CheckCompanyDetails();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(PagePath.Error);
    }

    [TestMethod]
    public async Task GivenCompaniesHouseUser_WhenCheckCompanyDetailsPostCalled_ThenRedirectToErrorPage()
    {
        // Arrange
        _userData.Organisations[0].CompaniesHouseNumber = "123";
        SetupBase(_userData);

        // Act
        var result = await SystemUnderTest.CheckCompanyDetailsPost();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(PagePath.Error);
    }


    [TestMethod]
    public async Task GivenUserWithNoOrganisation_WhenBusinessAddressCalled_ThrowInvalidOperationException()
    {
        // Arrange
        Exception result = null;

        _userData.Organisations[0] = null;

        SetupBase(_userData);

        // Act
        try
        {
            await SystemUnderTest.CheckCompanyDetailsPost();
        }
        catch (Exception ex)
        {
            result = ex;
        }

        // Assert
        result.Should().BeOfType<InvalidOperationException>();
    }


    [TestMethod]
    public async Task GivenNullSession_WhenCheckCompanyDetailsCalled_ShouldRedirectToError()
    {
        // Arrange
        SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null); // Mock session as null

        // Act
        var result = await SystemUnderTest.CheckCompanyDetails();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(PagePath.Error);
    }

    [TestMethod]
    public async Task GivenNullSession_WhenCheckCompanyDetailsPostCalled_ShouldRedirectToError()
    {
        // Arrange
        SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync((JourneySession)null); // Mock session as null

        // Act
        var result = await SystemUnderTest.CheckCompanyDetailsPost();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(PagePath.Error);
    }

    [TestMethod]
    public async Task GivenFeatureIsDisabled_WhenCheckCompanyDetailsCalled_ThenReturnsToErrorPage_WithNotFoundStatusCode()
    {
        // Arrange
        FeatureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.ManageCompanyDetailChanges))
        .ReturnsAsync(false);

        // Act
        var result = await SystemUnderTest.CheckCompanyDetails();

        // Assert
        using (new AssertionScope())
        {
            result.Should().BeOfType<RedirectToActionResult>();

            var actionResult = result as RedirectToActionResult;
            actionResult.Should().NotBeNull();
            actionResult.ActionName.Should().Be(PagePath.Error);
            actionResult.ControllerName.Should().Be(nameof(ErrorController.Error));
            actionResult.RouteValues["statusCode"].Should().Be((int)HttpStatusCode.NotFound);
        }
    }

    [TestMethod]
    public async Task GivenFeatureIsDisabled_WhenCheckCompanyDetailsSubmitted_ThenReturnsToErrorPage_WithNotFoundStatusCode()
    {
        // Arrange
        FeatureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.ManageCompanyDetailChanges))
        .ReturnsAsync(false);

        // Act
        var result = await SystemUnderTest.CheckCompanyDetailsPost();

        // Assert
        using (new AssertionScope())
        {
            result.Should().BeOfType<RedirectToActionResult>();

            var actionResult = result as RedirectToActionResult;
            actionResult.Should().NotBeNull();
            actionResult.ActionName.Should().Be(PagePath.Error);
            actionResult.ControllerName.Should().Be(nameof(ErrorController.Error));
            actionResult.RouteValues["statusCode"].Should().Be((int)HttpStatusCode.NotFound);
        }
    }
}
