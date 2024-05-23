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

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement;

[TestClass]
public class AccountManagementTests : AccountManagementTestBase
{
    private const string ViewName = "ManageAccount";

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

        redirectResult.ControllerName.Should().Be(nameof(ErrorController.Index));
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

        redirectResult.ControllerName.Should().Be(nameof(ErrorController.Index));
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
}