using System.Net;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using Microsoft.AspNetCore.Mvc;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Controllers.Errors;
using Microsoft.AspNetCore.Http;
using Moq;
using FrontendAccountManagement.Core.Models;
using EPR.Common.Authorization.Models;
using System;
using FrontendAccountManagement.Core.Enums;
using System.Security.Claims;
using System.Text.Json;
using Organisation = EPR.Common.Authorization.Models.Organisation;
using FrontendAccountManagement.Core.Models.CompaniesHouse;
using AutoMapper;
using FrontendAccountManagement.Web.Constants.Enums;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement;

[TestClass]
public class AccountManagementTests : AccountManagementTestBase
{
    private const string ViewName = "ManageAccount";
    private const string FirstName = "Test First Name";
    private const string LastName = "Test Last Name";
    private const string Telephone = "07545822431";

    private const string JobTitle = "Test Job Title";
    private const string OrganisationName = "Test Organization Name";
    private const string OrganisationType = "Companies House Company";

    private const string SubBuildingName = "Unit 5";
    private const string BuildingNumber = "10";
    private const string BuildingName = "Building";
    private const string Street = "A Street";
    private const string Town = "A Town";
    private const string County = "County";
    private const string Postcode = "AB1 1BA";

    private const string RoleInOrganisation = "Admin";
    private const int ServiceRoleId = 1;
    private const string ServiceRoleKey = "Approved.Admin";
    

    [TestInitialize]
    public void Setup()
    {
        SetupBase();
    }

    [TestMethod]
    public async Task GivenOnManageAccountPage_WhenManageAccountHttpGetCalled_WithNoJourneyPathSet_ThenShowManageAccountPage()
    {
        // Arrange
        SetupUserData(string.Empty);

        // Act
        var result = await SystemUnderTest.ManageAccount(new ManageAccountViewModel()) as ViewResult;

        // Assert
        result.Should().BeOfType<ViewResult>();
        result.ViewName.Should().Be(ViewName);
        SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
    }

    [TestMethod]
    public async Task GivenOnManageAccountPage_WhenDeploymentRoleIsRegulator_WithRegulatorAdminServiceRole_ThenShowManageAccountPage()
    {
        // Arrange
        SetupBase(deploymentRole: DeploymentRoleOptions.RegulatorRoleValue,
            userServiceRoleId: (int)Core.Enums.ServiceRole.RegulatorAdmin);

        var userData = new UserData
        {
            Organisations = new List<Organisation>
            {
                new Organisation
                {
                }
            }
        };

        // Create a mock identity
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.UserData, JsonSerializer.Serialize(userData)),
        };

        var identity = new Mock<ClaimsIdentity>();
        identity.Setup(i => i.IsAuthenticated).Returns(true);
        identity.Setup(i => i.Claims).Returns(claims);

        // Create a mock ClaimsPrincipal
        var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
        mockClaimsPrincipal.Setup(p => p.Identity).Returns(identity.Object);
        mockClaimsPrincipal.Setup(p => p.Claims).Returns(claims);

        // Use the mock ClaimsPrincipal in your tests
        var claimsPrincipal = mockClaimsPrincipal.Object;

        HttpContextMock.Setup(c => c.User).Returns(claimsPrincipal);

        // Act
        var result = await SystemUnderTest.ManageAccount(new ManageAccountViewModel()) as ViewResult;

        // Assert
        result.Should().BeOfType<ViewResult>();
        result.ViewName.Should().Be(ViewName);
        SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
        result.ViewData.Should().ContainKey("CustomBackLinkToDisplay");
        result.ViewData["CustomBackLinkToDisplay"].Should().Be("/back/to/home");
    }

    [TestMethod]
    public async Task GivenOnManageAccountPage_WhenDeploymentRoleIsRegulator_WithInvalidServiceRole_ThenReturnForbidden()
    {
        // Arrange
        SetupBase(deploymentRole: DeploymentRoleOptions.RegulatorRoleValue,
            userServiceRoleId: (int)Core.Enums.ServiceRole.RegulatorBasic);

        // Act
        var result = await SystemUnderTest.ManageAccount(new ManageAccountViewModel());

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;

        redirectResult.ControllerName.Should().Be(nameof(ErrorController.Error));
        redirectResult.ActionName.Should().Be(PagePath.Error);
        redirectResult.RouteValues.Should().ContainKey("statusCode");
        redirectResult.RouteValues["statusCode"].Should().Be((int)HttpStatusCode.Forbidden);
    }

    [TestMethod]
    public async Task GivenOnManageAccountPage_WhenDeploymentRoleIsNotRegulator_WithRegulatorAdminServiceRole_ThenReturnForbidden()
    {
        // Arrange
        SetupBase(deploymentRole: string.Empty,
            userServiceRoleId: (int)Core.Enums.ServiceRole.RegulatorAdmin);

        // Act
        var result = await SystemUnderTest.ManageAccount(new ManageAccountViewModel());

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        var redirectResult = (RedirectToActionResult)result;

        redirectResult.ControllerName.Should().Be(nameof(ErrorController.Error));
        redirectResult.ActionName.Should().Be(PagePath.Error);
        redirectResult.RouteValues.Should().ContainKey("statusCode");
        redirectResult.RouteValues["statusCode"].Should().Be((int)HttpStatusCode.Forbidden);
    }

    [TestMethod]
    public async Task GivenOnManageAccountPage_WhenManageAccountHttpGetCalled_WithRemoveUserStatusNotNull_ThenShowManageAccountPage()
    {
        // Arrange
        SetupUserData(string.Empty);

        RemoveUserJourneyModel userDetails = new() { FirstName = "An", LastName = "Test" };
        AccountManagementSession removeUserAccount = new() { RemoveUserStatus = 0, RemoveUserJourney = userDetails };
        SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
           .Returns(Task.FromResult(new JourneySession { AccountManagementSession = removeUserAccount }));
        // Act
        var result = await SystemUnderTest.ManageAccount(new ManageAccountViewModel()) as ViewResult;

        // Assert
        result.Should().BeOfType<ViewResult>();
        result.ViewName.Should().Be(ViewName);
        SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
    }

    [TestMethod]
    public async Task GivenOnManageAccountPage_WhenManageAccountHttpGetCalled_WithAddUserStatusNotNull_ThenShowManageAccountPage()
    {
        // Arrange
        SetupUserData(string.Empty);

        AddUserJourneyModel userDetails = new() { Email = "an.other@test.com", UserRole = "Test" };
        AccountManagementSession addUserAccount = new() { AddUserStatus = 0, AddUserJourney = userDetails };
        SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
           .Returns(Task.FromResult(new JourneySession { AccountManagementSession = addUserAccount }));
        // Act
        var result = await SystemUnderTest.ManageAccount(new ManageAccountViewModel()) as ViewResult;

        // Assert
        result.Should().BeOfType<ViewResult>();
        result.ViewName.Should().Be(ViewName);
        SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
    }

    [TestMethod]
    public async Task GivenOnManageAccountPage_WhenRemoveTeamMemberConfirmationCalled_ThenShowManageAccountPage()
    {
        // Arrange      
        RemoveTeamMemberConfirmationViewModel teamMember = new();
        var userData = new UserData()
        {
            FirstName = "FName",
            LastName = "LName",
            Organisations = new List<Organisation>()
            {
                new()
                {
                    Id = Guid.NewGuid()
                }
            }
        };

        SetupBase(userData);

        SystemUnderTest.ModelState.AddModelError("key", "error message");

        // Act
        var result = await SystemUnderTest.RemoveTeamMemberConfirmation(teamMember) as ViewResult;

        // Assert
        result.Should().BeOfType<ViewResult>();
        result.ViewName.Should().Be(null);
    }

    [TestMethod]
    public async Task GivenOnManageAccountPage_WhenManageAccountHttpGetCalled_ThenManageAccountViewModelReturnedWithAdditionalFieldsToDisplay()
    {
        // Act
        var userData = SetupUserData(string.Empty);
        
        SetupBase(userData: userData);
        var result = await SystemUnderTest.ManageAccount(new ManageAccountViewModel()) as ViewResult;
        var model = result.Model as ManageAccountViewModel;

        // Assert
        result.Should().BeOfType<ViewResult>();
        result.ViewName.Should().Be(ViewName);
        SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);

        Assert.AreEqual(string.Format("{0} {1}", FirstName, LastName), model.UserName);
        Assert.AreEqual(JobTitle, model.JobTitle);
        Assert.AreEqual(Telephone, model.Telephone);
        Assert.AreEqual(OrganisationName, model.CompanyName);
        Assert.AreEqual($"{SubBuildingName}, {BuildingNumber}, {BuildingName}, {Street}, {Town}, {County}, {Postcode}", model.OrganisationAddress);
        Assert.AreEqual(OrganisationType, model.OrganisationType);
        Assert.AreEqual(ServiceRoleKey, model.ServiceRoleKey);
    }

    [TestMethod]
    public async Task GivenOnEditUserDetailsPage_WhenRequested_TheShowUserDetails()
    {
        // Arrange
        SetupUserData(string.Empty);

        // Act
        var result = await SystemUnderTest.EditUserDetails();

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be(null);

        SessionManagerMock.Verify(m => m.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
        AutoMapperMock.Verify(m => m.Map<EditUserDetailsViewModel>(It.IsAny<UserData>()), Times.Once);
    }

    [TestMethod]
    public async Task GivenOnEditUserDetailsPage_WhenPageSubbmitedWithValidData_RedirectsToNextPage()
    {
        // Arrange
        var editUserDetailsViewModel = new EditUserDetailsViewModel
        {
            FirstName = "FirstName",
            LastName = "LastName",
            OriginalJobTitle = "Job",
            JobTitle = "Job",
            OriginalTelephone = "Telephone",
            Telephone = "Telepohne"
        };

        // Act
        var result = await SystemUnderTest.EditUserDetails(editUserDetailsViewModel);

        // Assert
        var viewResult = result as RedirectToActionResult;
        Assert.IsNotNull(viewResult);
    }

    [TestMethod]
    [DataRow("JobTitle")]
    [DataRow("Telephone")]
    public async Task GivenOnEditUserDetailsPage_WhenPageSubbmitedWithInvalidData_ReturnsOriginalPage(string fieldValidationError)
    {
        // Arrange
        if (fieldValidationError == "JobTitle")
        {
            SystemUnderTest.ModelState.AddModelError("JobTitle", "JobTitle is missing");
        }
        else
        {
            SystemUnderTest.ModelState.AddModelError("Telephone", "JobTitle is missing");
        }

        var editUserDetailsViewModel = new EditUserDetailsViewModel
        {
            FirstName = "FirstName",
            LastName = "LastName",
            OriginalJobTitle = "Job",
            JobTitle = "Job",
            OriginalTelephone = "Telephone",
            Telephone = "Telepohne"
        };

        // Act
        var result = await SystemUnderTest.EditUserDetails(editUserDetailsViewModel) as ViewResult;

        Assert.IsNotNull(result);
        Assert.IsTrue(string.IsNullOrWhiteSpace(result.ViewName));
        Assert.AreSame(editUserDetailsViewModel, result.Model);
    }

    [TestMethod]
    [DataRow("JobTitle")]
    [DataRow("Telephone")]
    public async Task GivenOnEditUserDetailsPage_WhenPageSubbmitedWithInvalidDataAndNoOriginalValues_RedirectsToNextPage(string fieldValidationError)
    {
        // Arrange
        if (fieldValidationError == "JobTitle")
        {
            SystemUnderTest.ModelState.AddModelError("JobTitle", "JobTitle is missing");
        }
        else
        {
            SystemUnderTest.ModelState.AddModelError("Telephone", "JobTitle is missing");
        }

        var editUserDetailsViewModel = new EditUserDetailsViewModel
        {
            FirstName = "FirstName",
            LastName = "LastName",
            JobTitle = "Job",
            Telephone = "Telepohne"
        };

        // Act
        var result = await SystemUnderTest.EditUserDetails(editUserDetailsViewModel);

        // assert
        var viewResult = result as RedirectToActionResult;
        Assert.IsNotNull(viewResult);
    }

    [TestMethod]
    [DataRow("Approved Person")]
    [DataRow("Delegated Person")]
    public async Task GetCheckCompaniesHouseDetails_AllowedPerson_ReturnedPageAsExpected(string serviceRole)
    {
        // Arrange
        SetupUserData(serviceRole);

        var viewModel = new CheckYourOrganisationDetailsViewModel();

        TempDataDictionaryMock.Setup(t => t["CheckYourOrganisationDetails"]).Returns(
            JsonSerializer.Serialize(viewModel));

        // Act
        var result = await SystemUnderTest.CheckCompaniesHouseDetails() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(string.IsNullOrWhiteSpace(result.ViewName));
        Assert.IsInstanceOfType<CheckYourOrganisationDetailsViewModel>(result.Model);
    }

    [TestMethod]
    public async Task GetCheckCompaniesHouseDetails_DisallowedPerson_ReturnedPageAsExpected()
    {
        // arrange
        SetupUserData(string.Empty);

        // act
        var result = await SystemUnderTest.CheckCompaniesHouseDetails() as UnauthorizedResult;

        // assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    [DataRow("Approved Person")]
    [DataRow("Delegated Person")]
    public async Task PostCheckCompaniesHouseDetails_ValidParameters_RequestsToSaveData(string serviceRole)
    {
        // Arrange
        SetupUserData(serviceRole);
        var organisationId = Guid.NewGuid();
        var ukNation = UkNation.England;
        var viewModel = new CheckYourOrganisationDetailsViewModel
        {
            OrganisationId = organisationId,
            UkNation = ukNation
        };

        // Act
        var result = await SystemUnderTest.CheckCompaniesHouseDetails(viewModel) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        FacadeServiceMock.Verify(s =>
            s.UpdateNationIdByOrganisationId(
                organisationId,
                (int)ukNation),
            Times.Once);
    }

    [TestMethod]
    [DataRow("Approved Person")]
    [DataRow("Delegated Person")]
    public async Task PostCheckCompaniesHouseDetails_InvalidModelState_ReturnsOriginalView(string serviceRole)
    {
        // Arrange
        SetupUserData(serviceRole);

        SystemUnderTest.ModelState.AddModelError("Error", "Error");
        var viewModel = new CheckYourOrganisationDetailsViewModel();

        // Act
        var result = await SystemUnderTest.CheckCompaniesHouseDetails(viewModel) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<CheckYourOrganisationDetailsViewModel>(result.Model);
    }

    private UserData SetupUserData(
        string serviceRole)
    {
        var userData = new UserData
        {
            FirstName = FirstName,
            LastName = LastName,
            JobTitle = JobTitle,
            Telephone = Telephone,
            ServiceRole = serviceRole,
            ServiceRoleId = ServiceRoleId,
            RoleInOrganisation = RoleInOrganisation,

            Organisations = new List<Organisation>
            {
                new Organisation
                {
                    Id = Guid.NewGuid(),
                    Name = OrganisationName,
                    OrganisationType = OrganisationType,
                    SubBuildingName = SubBuildingName,
                    BuildingNumber = BuildingNumber,
                    BuildingName = BuildingName,
                    Street = Street,
                    Town = Town,
                    County = County,
                    Postcode = Postcode,
                }
            }
        };

        // Create a mock identity
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.UserData, JsonSerializer.Serialize(userData)),
        };

        var identity = new Mock<ClaimsIdentity>();
        identity.Setup(i => i.IsAuthenticated).Returns(true);
        identity.Setup(i => i.Claims).Returns(claims);

        // Create a mock ClaimsPrincipal
        var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
        mockClaimsPrincipal.Setup(p => p.Identity).Returns(identity.Object);
        mockClaimsPrincipal.Setup(p => p.Claims).Returns(claims);

        // Use the mock ClaimsPrincipal in your tests
        var claimsPrincipal = mockClaimsPrincipal.Object;

        HttpContextMock.Setup(c => c.User).Returns(claimsPrincipal);

        return userData;
    }

    [TestMethod]
    public async Task GivenOnCompanyDetailsHaveNotChangedPage_ForAnApprovedPerson_WhenRequested_ThenShowView()
    {
        // Arrange
        SetupUserData("Approved Person");

        // Act
        var result = await SystemUnderTest.CompanyDetailsHaveNotChanged();

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be(null);

        SessionManagerMock.Verify(m => m.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
    }

    [TestMethod]
    public async Task GivenOnCompanyDetailsHaveNotChangedPage_ForADelegatedPerson_WhenRequested_ThenShowView()
    {
        // Arrange
        SetupUserData("Delegated Person");

        // Act
        var result = await SystemUnderTest.CompanyDetailsHaveNotChanged();

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = result as ViewResult;
        viewResult.ViewName.Should().Be(null);

        SessionManagerMock.Verify(m => m.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
    }

    [TestMethod]
    public async Task GivenOnCompanyDetailsHaveNotChangedPage_ForAnUnauthorisedPerson_WhenRequested_ReturnUnauthroised()
    {
        // Arrange
        SetupUserData("Unauthorised person");

        var companiesHouseData = new CompaniesHouseResponse();

        FacadeServiceMock.Setup(s => s.GetCompaniesHouseResponseAsync("123456")).ReturnsAsync(companiesHouseData);

        // Act
        var result = await SystemUnderTest.CompanyDetailsHaveNotChanged();

        // Assert
        result.Should().BeOfType<UnauthorizedResult>();
        SessionManagerMock.Verify(m => m.GetSessionAsync(It.IsAny<ISession>()), Times.Never);
    }

    private void SetupUserData(string serviceRole)
    {
        var userData = new UserData
        {
            ServiceRole = serviceRole,
            Organisations = new List<Organisation>
            {
                new Organisation
                {

                }
            }
        };

        // Create a mock identity
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.UserData, JsonSerializer.Serialize(userData)),
        };

        var identity = new Mock<ClaimsIdentity>();
        identity.Setup(i => i.IsAuthenticated).Returns(true);
        identity.Setup(i => i.Claims).Returns(claims);

        // Create a mock ClaimsPrincipal
        var mockClaimsPrincipal = new Mock<ClaimsPrincipal>();
        mockClaimsPrincipal.Setup(p => p.Identity).Returns(identity.Object);
        mockClaimsPrincipal.Setup(p => p.Claims).Returns(claims);

        // Use the mock ClaimsPrincipal in your tests
        var claimsPrincipal = mockClaimsPrincipal.Object;

        HttpContextMock.Setup(c => c.User).Returns(claimsPrincipal);
    }
}