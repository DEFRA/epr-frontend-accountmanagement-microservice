using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Core.Enums;
using FrontendAccountManagement.Core.Models;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Controllers.ReEx;
using FrontendAccountManagement.Web.UnitTests.Controllers.ReEx;
using FrontendAccountManagement.Web.ViewModels.ReExAccountManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Organisation = EPR.Common.Authorization.Models.Organisation;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.ReExAccountManagement;

[TestClass]
public class RemoveReExTeamMemberConfirmationTests : ReExAccountManagementTestBase
{
    private const string FirstName = "Test";
    private const string LastName = "User";
    private const string ViewName = "RemoveTeamMemberConfirmation";
    private readonly Guid personId = Guid.NewGuid();
    private readonly Guid organisationId = Guid.NewGuid();
    private string _personExternalId;
    private int _serviceRoleId;
    private UserData _userData;
    private readonly int enrolmentId = 1;
    private ViewDetailsViewModel viewDetailsViewModel;

    [TestInitialize]
    public void Setup()
    {
        _userData = new UserData
        {
            ServiceRoleId = 3,
            Organisations = [ new() { Id = Guid.NewGuid() } ]
        };

        SetupBase(_userData);

        JourneySessionMock = new JourneySession
        {
            ReExAccountManagementSession = new ReExAccountManagementSession
            {
                Journey =
                [
                    PagePath.ReExManageAccount,
                    PagePath.RemoveTeamMember
                ],
                ReExRemoveUserJourney = new()
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    PersonId = personId,
                    OrganisationId = organisationId
                },
            }
        };

        viewDetailsViewModel = new ViewDetailsViewModel
        {
            PersonId = personId,
            OrganisationId = organisationId,
            EnrolmentId = enrolmentId
        };

        _personExternalId = Guid.NewGuid().ToString();
        _serviceRoleId = 3;

        SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(JourneySessionMock);
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

        JourneySessionMock.ReExAccountManagementSession.AddUserJourney = new AddUserJourneyModel
        {
            UserRole = "RegulatorAdmin"
        };

        var sessionJourney = new JourneySession
        {
            ReExAccountManagementSession = new ReExAccountManagementSession
            {
                ReExRemoveUserJourney = new ReExRemoveUserJourneyModel
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    PersonId = personId,
                    OrganisationId = organisationId
                },
                Journey = [PagePath.ReExManageAccount, PagePath.TeamMemberDetails, PagePath.RemoveTeamMember]
            },
            UserData = mockUserData
        };

        SetupBase(mockUserData, journeySession: sessionJourney);

        // Act
        var result = await SystemUnderTest.RemoveTeamMemberConfirmation() as ViewResult;
        var model = result.Model as RemoveReExTeamMemberConfirmationViewModel;

        // Assert
        result.ViewName.Should().Be(ViewName);
        AssertBackLink(result, PagePath.TeamMemberPermissions);

        Assert.AreEqual(FirstName, model.FirstName);
        Assert.AreEqual(LastName, model.LastName);
        Assert.AreEqual(personId, model.PersonId);
    }

    [TestMethod]
    public async Task GivenOnRemoveTeamMemberPreConfirmationPage_CheckDetailsSaved()
    {
        // Act
        await SystemUnderTest.RemoveTeamMemberPreConfirmation(viewDetailsViewModel);

        // Assert
        Assert.AreEqual(FirstName, JourneySessionMock.ReExAccountManagementSession.ReExRemoveUserJourney.FirstName);
        Assert.AreEqual(LastName, JourneySessionMock.ReExAccountManagementSession.ReExRemoveUserJourney.LastName);
        Assert.AreEqual(personId, JourneySessionMock.ReExAccountManagementSession.ReExRemoveUserJourney.PersonId);
    }

    [TestMethod]
    public async Task GivenOnRemoveTeamMemberPreConfirmationPage_CheckDetailsSaved_WithNullRemoveUserJourney_ToStart()
    {
        // arrange
        JourneySessionMock.ReExAccountManagementSession.ReExRemoveUserJourney = null;

        // Act
        await SystemUnderTest.RemoveTeamMemberPreConfirmation(viewDetailsViewModel);

        // Assert
        Assert.AreEqual(personId, JourneySessionMock.ReExAccountManagementSession.ReExRemoveUserJourney.PersonId);
    }

    [TestMethod]
    public async Task GivenOnRemoveTeamMemberPreConfirmationPage_CheckDetailsSaved_WithNullValues_ToStart()
    {
        // Act
        await SystemUnderTest.RemoveTeamMemberPreConfirmation(new ViewDetailsViewModel());

        // Assert
        Assert.AreEqual(FirstName, JourneySessionMock.ReExAccountManagementSession.ReExRemoveUserJourney.FirstName);
        Assert.AreEqual(LastName, JourneySessionMock.ReExAccountManagementSession.ReExRemoveUserJourney.LastName);
        Assert.AreEqual(personId, JourneySessionMock.ReExAccountManagementSession.ReExRemoveUserJourney.PersonId);
    }

    [TestMethod]
    public async Task GivenOnRemoveTeamMemberConfirmationPage_WhenRemoveTeamMemberConfirmationPageHttpPostCalled_AndSuccessfulResponse_ThenUserRemoved()
    {
        // Arrange
        FacadeServiceMock.Setup(x => x.RemoveUserForOrganisation(_personExternalId, organisationId.ToString(), _serviceRoleId)).ReturnsAsync(EndpointResponseStatus.Success);
        var model = new RemoveReExTeamMemberConfirmationViewModel
        {
            PersonId = personId,
            FirstName = FirstName,
            LastName = LastName,
            OrganisationId = organisationId
        };

        // Act
        var result = await SystemUnderTest.RemoveTeamMemberConfirmation(model) as RedirectToActionResult;

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
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

        FacadeServiceMock.Setup(x => x.RemoveUserForOrganisation(_personExternalId, organisationId.ToString(), _serviceRoleId)).ReturnsAsync(EndpointResponseStatus.Success);

        var model = new RemoveReExTeamMemberConfirmationViewModel
        {
            PersonId = personId,
            FirstName = FirstName,
            LastName = LastName,
            OrganisationId = organisationId
        };

        // Act
        var result = await SystemUnderTest.RemoveTeamMemberConfirmation(model);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<RedirectToActionResult>();
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
        result.Should().BeOfType<ViewResult>();
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
            ReExAccountManagementSession = new ReExAccountManagementSession
            {
                ReExRemoveUserJourney = new ReExRemoveUserJourneyModel
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    PersonId = personId,
                    OrganisationId = organisationId
                },
                Journey = [PagePath.ManageAccount, PagePath.TeamMemberDetails, PagePath.RemoveTeamMember]
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