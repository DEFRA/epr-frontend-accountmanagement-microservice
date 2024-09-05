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
using Organisation = EPR.Common.Authorization.Models.Organisation;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement;

[TestClass]
public class RemoveTeamMemberConfirmationTests : AccountManagementTestBase
{
    private const string FirstName = "Test";
    private const string LastName = "User";
    private const string ViewName = "RemoveTeamMemberConfirmation";
    private readonly Guid _personId = Guid.NewGuid();
    private string _personExternalId;
    private string _organisationId;
    private int _serviceRoleId;
    private UserData _userData;

    [TestInitialize]
    public void Setup()
    {
        _userData = new UserData
        {
            ServiceRoleId = 3,
            Organisations = new List<Organisation>
            {
                new()
                {
                    Id = Guid.NewGuid()
                }
            }
        };

        SetupBase(_userData);

        JourneySessionMock = new JourneySession
        {
            AccountManagementSession = new AccountManagementSession
            {
                Journey = new List<string>
                {
                    PagePath.ManageAccount,
                    PagePath.RemoveTeamMember
                },
                RemoveUserJourney = new()
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    PersonId = _personId
                },
            }
        };

        _personExternalId = Guid.NewGuid().ToString();
        _organisationId = Guid.NewGuid().ToString();
        _serviceRoleId = 3;

        SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(JourneySessionMock);
    }

    [TestMethod]
    public async Task GivenOnRemoveTeamMemberConfirmationPage_WhenRemoveTeamMemberConfirmationPageHttpGetCalled_ThenRemoveTeamMemberConfirmationViewModelReturned_AndBackLinkSet()
    {
        // Arrange
        var mockUserData = new UserData
        {
            ServiceRole = Core.Enums.ServiceRole.Approved.ToString(),
            ServiceRoleId = 1,
            RoleInOrganisation = PersonRole.Admin.ToString(),
        };

        JourneySessionMock.AccountManagementSession.AddUserJourney = new AddUserJourneyModel
        {
            UserRole = "RegulatorAdmin"
        };

        var sessionJourney = new JourneySession
        {
            AccountManagementSession = new AccountManagementSession
            {
                RemoveUserJourney = new RemoveUserJourneyModel
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    PersonId = _personId
                },
                Journey = new List<string> { PagePath.ManageAccount, PagePath.TeamMemberDetails, PagePath.RemoveTeamMember }
            },
            UserData = mockUserData
        };

        SetupBase(mockUserData, journeySession: sessionJourney);

        // Act
        var result = await SystemUnderTest.RemoveTeamMemberConfirmation() as ViewResult;
        var model = result.Model as RemoveTeamMemberConfirmationViewModel;

        // Assert
        result.ViewName.Should().Be(ViewName);
        AssertBackLink(result, PagePath.TeamMemberDetails);

        Assert.AreEqual(FirstName, model.FirstName);
        Assert.AreEqual(LastName, model.LastName);
        Assert.AreEqual(_personId, model.PersonId);
    }

    [TestMethod]
    public async Task GivenOnRemoveTeamMemberPreConfirmationPage_CheckDetailsSaved()
    {
        // Act
        await SystemUnderTest.RemoveTeamMemberPreConfirmation(FirstName, LastName, _personId);

        // Assert
        Assert.AreEqual(FirstName, JourneySessionMock.AccountManagementSession.RemoveUserJourney.FirstName);
        Assert.AreEqual(LastName, JourneySessionMock.AccountManagementSession.RemoveUserJourney.LastName);
        Assert.AreEqual(_personId, JourneySessionMock.AccountManagementSession.RemoveUserJourney.PersonId);
    }

    [TestMethod]
    public async Task GivenOnRemoveTeamMemberPreConfirmationPage_CheckDetailsSaved_WithNullRemoveUserJourney_ToStart()
    {
        // arrange
        JourneySessionMock.AccountManagementSession.RemoveUserJourney = null;

        // Act
        await SystemUnderTest.RemoveTeamMemberPreConfirmation(FirstName, LastName, _personId);

        // Assert
        Assert.AreEqual(FirstName, JourneySessionMock.AccountManagementSession.RemoveUserJourney.FirstName);
        Assert.AreEqual(LastName, JourneySessionMock.AccountManagementSession.RemoveUserJourney.LastName);
        Assert.AreEqual(_personId, JourneySessionMock.AccountManagementSession.RemoveUserJourney.PersonId);
    }

    [TestMethod]
    public async Task GivenOnRemoveTeamMemberPreConfirmationPage_CheckDetailsSaved_WithNullValues_ToStart()
    {
        // Act
        await SystemUnderTest.RemoveTeamMemberPreConfirmation(null, null, Guid.Empty);

        // Assert
        Assert.AreEqual(FirstName, JourneySessionMock.AccountManagementSession.RemoveUserJourney.FirstName);
        Assert.AreEqual(LastName, JourneySessionMock.AccountManagementSession.RemoveUserJourney.LastName);
        Assert.AreEqual(_personId, JourneySessionMock.AccountManagementSession.RemoveUserJourney.PersonId);
    }

    [TestMethod]
    public async Task GivenOnRemoveTeamMemberConfirmationPage_WhenRemoveTeamMemberConfirmationPageHttpPostCalled_AndSuccessfulResponse_ThenUserRemoved()
    {
        // Arrange
        FacadeServiceMock.Setup(x => x.RemoveUserForOrganisation(_personExternalId, _organisationId, _serviceRoleId))
            .ReturnsAsync(EndpointResponseStatus.Success);
        var model = new RemoveTeamMemberConfirmationViewModel
        {
            PersonId = _personId,
            FirstName = FirstName,
            LastName = LastName
        };

        // Act
        var result = await SystemUnderTest.RemoveTeamMemberConfirmation(model) as RedirectToActionResult;

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        result.ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));
        SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
    }

    [TestMethod]
    public async Task GivenOnRemoveTeamMemberConfirmationPage_WhenRemoveTeamMemberConfirmationPageHttpPostCalled_AndOrganisationInvalid_ThenUserNotRemovedAndRedirected()
    {
        // Arrange
        _userData = new UserData
        {
            ServiceRoleId = 3,
            Organisations = new List<Organisation>()
        };

        SetupBase(_userData);

        FacadeServiceMock.Setup(x => x.RemoveUserForOrganisation(_personExternalId, _organisationId, _serviceRoleId))
            .ReturnsAsync(EndpointResponseStatus.Success);

        var model = new RemoveTeamMemberConfirmationViewModel
        {
            PersonId = _personId,
            FirstName = FirstName,
            LastName = LastName
        };

        // Act
        var result = await SystemUnderTest.RemoveTeamMemberConfirmation(model);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));
    }

    [TestMethod]
    public async Task GivenOnRemoveTeamMemberConfirmationPage_DisplayPageNotFound_WhenUserIsBasicEmployee()
    {
        // Arrange
        var userData = new UserData
        {
            ServiceRole = Core.Enums.ServiceRole.Basic.ToString(),
            ServiceRoleId = 3,
            RoleInOrganisation = PersonRole.Employee.ToString(),
        };

        SetupBase(userData);

        // Act
        var result = await SystemUnderTest.RemoveTeamMemberConfirmation();

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        SessionManagerMock.Verify(m => m.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
    }

    [TestMethod]
    public async Task GivenOnRemoveTeamMemberConfirmationPage_DisplayPageAsNormal_WhenUserIsBasicAdmin()
    {
        // Arrange
        var mockUserData = new UserData
        {
            ServiceRole = Core.Enums.ServiceRole.Basic.ToString(),
            ServiceRoleId = 3,
            RoleInOrganisation = PersonRole.Admin.ToString(),
        };

        var sessionJourney = new JourneySession
        {
            AccountManagementSession = new AccountManagementSession
            {
                RemoveUserJourney = new RemoveUserJourneyModel
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    PersonId = _personId
                },
                Journey = new List<string> { PagePath.ManageAccount, PagePath.TeamMemberDetails, PagePath.RemoveTeamMember }
            },
            UserData = mockUserData
        };

        SetupBase(mockUserData, journeySession: sessionJourney);

        // Act
        var result = await SystemUnderTest.RemoveTeamMemberConfirmation();

        // Assert
        result.Should().BeOfType<ViewResult>();
        SessionManagerMock.Verify(m => m.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
    }
}