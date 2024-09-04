using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Controllers.AccountManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using FrontendAccountManagement.Core.Models;
using FrontendAccountManagement.Web.Constants;
using Moq;
using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Core.Enums;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement;

[TestClass]
public class TeamMemberRoleTests : AccountManagementTestBase
{
    private const string ViewName = "TeamMemberPermissions";
    private const string ModelErrorValueSelectARole = "Please select a role";
    private const string SelectedRole = "3:2:Manage team and upload data";
    private const string RolesNotFoundException = "Could not retrieve service roles or none found";
    private const string PreviouslyEnteredEmail = "fakeemail@test.com";

    private readonly Core.Models.ServiceRole TestRole = new()
    {
        ServiceRoleId = 3,
        PersonRoleId = 1,
        Key = "test.role",
        DescriptionKey = "test.descriptionKey"
    };

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
                    PagePath.TeamMemberPermissions
                },
                AddUserJourney = new AddUserJourneyModel
                {
                    Email = PreviouslyEnteredEmail
                }
            }
        };

        SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(JourneySessionMock);
    }

    [TestMethod]
    public async Task GivenOnTeamMemberRolePage_WhenTeamMemberPermissionsHttpGetCalled_ThenTeamMemberPermissionsViewModelReturned_AndBackLinkSet()
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
                AddUserJourney = new AddUserJourneyModel
                {
                    UserRole = SelectedRole
                },
                Journey = new List<string> { PagePath.TeamMemberEmail, PagePath.TeamMemberPermissions }
            },
            UserData = mockUserData
        };

        SetupBase(mockUserData, journeySession: sessionJourney);

        FacadeServiceMock.Setup(x => x.GetAllServiceRolesAsync())
            .Returns(Task.FromResult<IEnumerable<Core.Models.ServiceRole>>(new List<Core.Models.ServiceRole>
                {
                    TestRole
                }));

        // Act
        var result = await SystemUnderTest.TeamMemberPermissions() as ViewResult;
        var model = result.Model as TeamMemberPermissionsViewModel;

        // Assert
        result.ViewName.Should().Be(ViewName);
        AssertBackLink(result, PagePath.TeamMemberEmail);
        Assert.IsTrue(model.ServiceRoles.Contains(TestRole));
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException), RolesNotFoundException)]
    public async Task GivenOnTeamMemberRolePage_WhenTeamMemberPermissionsHttpGetCalled_AndNoRolesReturnedFromFacade_ThenThrowException()
    {
        // Arrange
        var mockUserData = new UserData
        {
            ServiceRole = Core.Enums.ServiceRole.Approved.ToString(),
            ServiceRoleId = 1,
            RoleInOrganisation = PersonRole.Admin.ToString(),
        };

        SetupBase(mockUserData);

        FacadeServiceMock.Setup(x => x.GetAllServiceRolesAsync())
           .Returns(Task.FromResult<IEnumerable<Core.Models.ServiceRole>>(new List<Core.Models.ServiceRole>()));

        // Act
        var result = await SystemUnderTest.TeamMemberPermissions() as ViewResult;

        // Assert
        result.ViewName.Should().Be(ViewName);
        result.Model.Should().BeOfType<TeamMemberPermissionsViewModel>();
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException), RolesNotFoundException)]
    public async Task GivenOnTeamMemberRolePage_WhenTeamMemberPermissionsHttpGetCalled_AndExceptionReturnedFromFacade_ThenThrowException()
    {
        // Arrange
        FacadeServiceMock.Setup(x => x.GetAllServiceRolesAsync())
            .Throws(new Exception());

        var mockUserData = new UserData
        {
            ServiceRole = Core.Enums.ServiceRole.Approved.ToString(),
            ServiceRoleId = 1,
            RoleInOrganisation = PersonRole.Admin.ToString(),
        };

        SetupBase(mockUserData);

        // Act
        var result = await SystemUnderTest.TeamMemberPermissions() as ViewResult;

        // Assert
        result.ViewName.Should().Be(ViewName);
        result.Model.Should().BeOfType<TeamMemberPermissionsViewModel>();
    }

    [TestMethod]
    public async Task GivenOnTeamMemberRolePage_WhenTeamMemberPermissionsHttpPostCalled_WhereNoRoleSelected_ThenErrorMessageIsDisplayed()
    {
        // Arrange
        var model = new TeamMemberPermissionsViewModel();
        SystemUnderTest.ModelState.AddModelError(ModelErrorKey, ModelErrorValueSelectARole);

        // Act
        var result = await SystemUnderTest.TeamMemberPermissions(model) as ViewResult;

        // Assert
        result.Should().BeOfType<ViewResult>();
        result.Model.Should().BeOfType<TeamMemberPermissionsViewModel>();
        result.ViewName.Should().Be("TeamMemberPermissions");

        var errors = result.ViewData.ModelState["Error"].Errors;
        Assert.AreEqual(expected: ModelErrorValueSelectARole, actual: errors[0].ErrorMessage);
    }

    [TestMethod]
    public async Task GivenOnTeamMemberRolePage_WhenTeamMemberPermissionsHttpPostCalled_WhereRoleSelected_ThenUserIsTakenToNextPage_AndUpdateSession()
    {
        // Arrange
        var model = new TeamMemberPermissionsViewModel
        {
            SelectedUserRole = SelectedRole
        };

        // Act
        var result = await SystemUnderTest.TeamMemberPermissions(model) as RedirectToActionResult;

        // Assert
        result.ActionName.Should().Be(nameof(AccountManagementController.TeamMemberDetails));
        SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()),
            Times.Once);
    }

    [TestMethod]
    [DataRow(SelectedRole)]
    public async Task GivenOnTeamMemberRolePage_WhenTeamMemberPermissionsHttpGetCalled_RolePreviouslySet_ThenRoleValueIsPopulated(string role)
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
                AddUserJourney = new AddUserJourneyModel
                {
                    UserRole = SelectedRole
                }
            },
            UserData = mockUserData
        };

        SetupBase(mockUserData, "Regulator", journeySession: sessionJourney);

        FacadeServiceMock.Setup(x => x.GetAllServiceRolesAsync())
            .Returns(Task.FromResult<IEnumerable<Core.Models.ServiceRole>>(new List<Core.Models.ServiceRole> { TestRole }));

        // Act
        var result = await SystemUnderTest.TeamMemberPermissions() as ViewResult;
        var model = result.Model as TeamMemberPermissionsViewModel;

        // Assert
        Assert.AreEqual(role, model.SavedUserRole);
    }

    [TestMethod]
    public async Task GivenOnTeamMemberRolePage_DisplayPageNotFound_WhenUserIsBasicEmployee()
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
        var result = await SystemUnderTest.TeamMemberPermissions();

        // Assert
        result.Should().BeOfType<NotFoundResult>();
        SessionManagerMock.Verify(m => m.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
    }

    [TestMethod]
    public async Task GivenOnTeamMemberRolePage_DisplayPageAsNormal_WhenUserIsBasicAdmin()
    {
        // Arrange
        var mockUserData = new UserData
        {
            ServiceRole = Core.Enums.ServiceRole.Basic.ToString(),
            ServiceRoleId = 3,
            RoleInOrganisation = PersonRole.Admin.ToString(),
        };

        JourneySessionMock.AccountManagementSession.AddUserJourney = new AddUserJourneyModel
        {
            UserRole = "BasicAdmin"
        };

        var sessionJourney = new JourneySession
        {
            AccountManagementSession = new AccountManagementSession
            {
                AddUserJourney = new AddUserJourneyModel
                {
                    UserRole = "Admin"
                }
            },
            UserData = mockUserData
        };

        SetupBase(mockUserData, "Admin", journeySession: sessionJourney);

        FacadeServiceMock.Setup(x => x.GetAllServiceRolesAsync())
            .Returns(Task.FromResult<IEnumerable<Core.Models.ServiceRole>>(new List<Core.Models.ServiceRole> { TestRole }));

        // Act
        var result = await SystemUnderTest.TeamMemberPermissions();

        // Assert
        result.Should().BeOfType<ViewResult>();
        SessionManagerMock.Verify(m => m.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
    }
}