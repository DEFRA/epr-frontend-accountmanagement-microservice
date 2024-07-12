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
using System.Security.Policy;
using System;
using FrontendAccountManagement.Core.Enums;
using EPR.Common.Authorization.Constants;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement;

[TestClass]
public class AccountManagementTests : AccountManagementTestBase
{
    private const string ViewName = "ManageAccount";
    private const string FirstName = "Test First Name";
    private const string LastName = "Test Last Name";
    private const string Telephone = "07545822431";

    private const string JobTitle = "Test Job Title";
    private const string OrgName = "Test Organization Name";
    private const string OrganisationType = "Companies House Company";
    private const string OrgAddress = "Test Organisation Address";

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
        RemoveUserJourneyModel userDetails = new() { FirstName = "An", LastName = "Test" };
        AccountManagementSession removeUserAccount = new() { RemoveUserStatus = 0, RemoveUserJourney = userDetails };
        SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
           .Returns(Task.FromResult(new JourneySession { AccountManagementSession =  removeUserAccount }));
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
        var userData = new UserData()
        {
            FirstName = FirstName,
            LastName = LastName
        };

        var userDataToDispaly = new UserOrganisationsListModelDto();
        userDataToDispaly.User = new UserDetailsModel() { FirstName = FirstName, LastName = LastName, Telephone = Telephone, ServiceRoleId = ServiceRoleId, RoleInOrganisation = RoleInOrganisation };
        userDataToDispaly.User.Organisations = new List<OrganisationDetailModel>
            {
                new OrganisationDetailModel() { 
                    JobTitle = JobTitle, 
                    Id = Guid.NewGuid(), 
                    Name = OrgName, 
                    OrganisationType = OrganisationType, 
                    OrgAddress = OrgAddress
                }
            };
        SetupBase(userData: userData, userDataToDispaly: userDataToDispaly);
        var result = await SystemUnderTest.ManageAccount(new ManageAccountViewModel()) as ViewResult;
        var model = result.Model as ManageAccountViewModel;

        // Assert
        result.Should().BeOfType<ViewResult>();
        result.ViewName.Should().Be(ViewName);
        SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);

        Assert.AreEqual(string.Format("{0} {1}", FirstName, LastName), model.UserName);
        Assert.AreEqual(JobTitle, model.JobTitle);
        Assert.AreEqual(Telephone, model.Telephone);
        Assert.AreEqual(OrgName, model.CompanyName);
        Assert.AreEqual(OrgAddress, model.OrgAddress);
        Assert.AreEqual(OrganisationType, model.OrganisationType);
        Assert.AreEqual(ServiceRoleKey, model.ServiceRoleKey);
    }
}