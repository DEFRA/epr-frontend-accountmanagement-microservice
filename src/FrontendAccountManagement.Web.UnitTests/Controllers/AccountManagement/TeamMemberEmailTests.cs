using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Core.Enums;
using FrontendAccountManagement.Core.Models;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Controllers.AccountManagement;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement;

[TestClass]
public class TeamMemberEmailTests : AccountManagementTestBase
{
    private const string InvalidEmailFormat = "test1.com";
    private const string ValidEmailFormat = "test1@test.com";
    private const string EmailExceedsMaxLength =
        "test+abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz@test.com";
    private const string ModelErrorValueEmailFormat = "Enter an email in the correct format, like name@example.com";
    private const string ModelErrorValueMissingString = "Enter team members email";
    private const string ModelErrorEmailTooLong = "Email address must be 254 characters or less";
    private const string ViewName = "TeamMemberEmail";

    [TestInitialize]
    public void Setup()
    {
        SetupBase();

        JourneySessionMock = new JourneySession
        {
            AccountManagementSession = new AccountManagementSession
            {
                Journey = new List<string>
                {
                    PagePath.ManageAccount,
                    PagePath.TeamMemberEmail,
                },
                AddUserJourney = new AddUserJourneyModel()
            }
        };

        SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(JourneySessionMock);
    }

    [TestMethod]
    public async Task GivenOnTeamMemberEmailPage_WhenTeamMemberEmailPageHttpGetCalled_ThenTeamMemberEmailPageReturned_AndBackLinkSet()
    {
        // Arrange
        var mockUserData = new UserData
        {
            ServiceRole = Core.Enums.ServiceRole.Approved.ToString(),
            ServiceRoleId = 1,
            RoleInOrganisation = PersonRole.Admin.ToString(),
        };

        SetupBase(mockUserData);

        // Act
        var result = await SystemUnderTest.TeamMemberEmail() as ViewResult;

        // Assert
        result.ViewName.Should().Be(ViewName);
        AssertBackLink(result, PagePath.ManageAccount);
    }

    [TestMethod]
    public async Task GivenOnTeamMemberEmailPage_WhenTeamMemberEmailPageHttpGetCalled_AndTheAddJourneyIsNull_ThenTeamMemberEmailPageReturnedAndBackLinkSet()
    {
        // Arrange              
        AccountManagementSession addUserAccount = new() { AddUserStatus = 0, AddUserJourney = null };
        SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
           .Returns(Task.FromResult(new JourneySession { AccountManagementSession = addUserAccount }));

        var mockUserData = new UserData
        {
            ServiceRole = Core.Enums.ServiceRole.Approved.ToString(),
            ServiceRoleId = 1,
            RoleInOrganisation = PersonRole.Admin.ToString(),
        };

        SetupBase(mockUserData);

        // Act
        var result = await SystemUnderTest.TeamMemberEmail() as ViewResult;

        // Assert
        result.ViewName.Should().Be(ViewName);
        AssertBackLink(result, PagePath.ManageAccount);
    }

    // happy path
    [TestMethod]
    public async Task GivenOnTeamMemberEmailPage_WhenTeamMemberEmailPageHttpPostCalled_ThenRedirectToPermissionsPage_AndUpdateSession()
    {
        // Act
        var viewModel = new TeamMemberEmailViewModel
        {
            Email = ValidEmailFormat,
        };
        var result = await SystemUnderTest.TeamMemberEmail(viewModel) as RedirectToActionResult;

        // Assert
        result.ActionName.Should().Be(nameof(AccountManagementController.TeamMemberPermissions));
        SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
    }

    // no email input
    [TestMethod]
    public async Task GivenOnTeamMemberEmailPage_WhenTeamMemberEmailPageHttpPostCalled_NoEmailText_ThenThrowValidationError()
    {
        // Act
        var viewModel = new TeamMemberEmailViewModel
        {
            Email = string.Empty,
        };
        SystemUnderTest.ModelState.AddModelError(ModelErrorKey, ModelErrorValueMissingString);

        var result = await SystemUnderTest.TeamMemberEmail(viewModel) as ViewResult;

        // Assert
        result.ViewName.Should().Be(ViewName);
        Assert.AreEqual(ModelErrorValueMissingString, result.ViewData.ModelState["Error"].Errors[0].ErrorMessage);
        SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Never);
    }

    // invalid email
    [TestMethod]
    public async Task GivenOnTeamMemberEmailPage_WhenTeamMemberEmailPageHttpPostCalled_InvalidEmailFormat_ThenThrowValidationError()
    {
        // Act
        var viewModel = new TeamMemberEmailViewModel
        {
            Email = InvalidEmailFormat,
        };
        SystemUnderTest.ModelState.AddModelError(ModelErrorKey, ModelErrorValueEmailFormat);

        var result = await SystemUnderTest.TeamMemberEmail(viewModel) as ViewResult;

        // Assert
        result.ViewName.Should().Be(ViewName);
        Assert.AreEqual(ModelErrorValueEmailFormat, result.ViewData.ModelState["Error"].Errors[0].ErrorMessage);
        SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Never);
    }

    // email exceeds max length
    [TestMethod]
    public async Task GivenOnTeamMemberEmailPage_WhenTeamMemberEmailPageHttpPostCalled_ExceedsMaxLength_ThenThrowValidationError()
    {
        // Act
        var viewModel = new TeamMemberEmailViewModel
        {
            Email = EmailExceedsMaxLength,
        };
        SystemUnderTest.ModelState.AddModelError(ModelErrorKey, ModelErrorEmailTooLong);

        var result = await SystemUnderTest.TeamMemberEmail(viewModel) as ViewResult;

        // Assert
        result.ViewName.Should().Be(ViewName);
        Assert.AreEqual(ModelErrorEmailTooLong, result.ViewData.ModelState["Error"].Errors[0].ErrorMessage);
        SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Never);
    }

    [TestMethod]
    [DataRow(ValidEmailFormat)]
    public async Task GivenOnTeamMemberEmailPage_WhenTeamMemberEmailPageHttpGetCalled_ValidEmailFormat_AndEmailValuePreviouslySet_ThenEmailValueIsPopulated(string email)
    {
        // Arrange
        JourneySessionMock.AccountManagementSession.AddUserJourney.Email = email;

        // Act
        var result = await SystemUnderTest.TeamMemberEmail() as ViewResult;
        var model = result.Model as TeamMemberEmailViewModel;

        // Assert
        Assert.AreEqual(ValidEmailFormat, model.SavedEmail);
    }

    [TestMethod]
    public async Task GivenOnTeamMemberEmailPage_WhenUserIsBasicEmployee_ThenDisplayPageNotFound()
    {
        // Arrange
        var userData = new UserData
        {
            ServiceRole = Core.Enums.ServiceRole.Basic.ToString(),
            RoleInOrganisation = PersonRole.Employee.ToString(),
        };

        SetupBase(userData);

        // Act
        var result = await SystemUnderTest.TeamMemberEmail();

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        SessionManagerMock.Verify(m => m.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
    }

    [TestMethod]
    public async Task GivenOnTeamMemberEmailPage_WhenUserIsBasicAdmin_ThenDisplayPageNotFound()
    {
        // Arrange
        var userData = new UserData
        {
            ServiceRole = Core.Enums.ServiceRole.Basic.ToString(),
            ServiceRoleId = 3,
            RoleInOrganisation = PersonRole.Admin.ToString(),
        };

        SetupBase(userData);

        // Act
        var result = await SystemUnderTest.TeamMemberEmail();

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        SessionManagerMock.Verify(m => m.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
    }
}