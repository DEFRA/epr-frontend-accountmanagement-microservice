using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Core.Enums;
using FrontendAccountManagement.Core.Models;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement;

[TestClass]
public class RegulatorTeamMemberRoleTests : AccountManagementTestBase
{
    private const string PreviouslyEnteredEmail = "fakeemail@test.com";
    private const string ViewName = "TeamMemberPermissions";
    private const string DeploymentRole = "Regulator";
    private const string RolesNotFoundException = "Could not retrieve service roles or none found";

    private readonly Core.Models.ServiceRole RegulatorTestRole = new()
    {
        ServiceRoleId = 4,
        PersonRoleId = 1,
        Key = "test.role",
        DescriptionKey = "test.descriptionKey"
    };

    [TestInitialize]
    public void Setup()
    {
        SetupBase(null, DeploymentRole);

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

        var sessionJourney = new JourneySession
        {
            AccountManagementSession = new AccountManagementSession
            {
                AddUserJourney = new AddUserJourneyModel
                {
                    UserRole = "RegulatorAdmin"
                },
                Journey = new List<string> { PagePath.TeamMemberEmail, PagePath.TeamMemberPermissions }
            },
            UserData = mockUserData
        };

        SetupBase(mockUserData, DeploymentRole, journeySession: sessionJourney);

        FacadeServiceMock.Setup(x => x.GetAllServiceRolesAsync())
            .Returns(Task.FromResult<IEnumerable<Core.Models.ServiceRole>>(new List<Core.Models.ServiceRole>
            {
                RegulatorTestRole,

            }));

        // Act
        var result = await SystemUnderTest.TeamMemberPermissions() as ViewResult;
        var model = result.Model as TeamMemberPermissionsViewModel;

        // Assert
        result.ViewName.Should().Be(ViewName);
        AssertBackLink(result, PagePath.TeamMemberEmail);
        Assert.IsTrue(model.ServiceRoles.Contains(RegulatorTestRole));
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException), RolesNotFoundException)]
    public async Task GivenOnTeamMemberRolePage_WhenTeamMemberPermissionsHttpGetCalled_AndServiceRolesIsNull_ThenTeamMemberPermissionsViewModelReturnedAndBackLinkSet()
    {
        // Arrange
        FacadeServiceMock.Setup(x => x.GetAllServiceRolesAsync())
            .Returns(Task.FromResult<IEnumerable<Core.Models.ServiceRole>>(null));

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
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException), RolesNotFoundException)]
    public async Task GivenOnTeamMemberRolePage_WhenTeamMemberPermissionsHttpGetCalled_AndServiceRolesIsEmpty_ThenTeamMemberPermissionsViewModelReturnedAndBackLinkSet()
    {
        // Arrange
        FacadeServiceMock.Setup(x => x.GetAllServiceRolesAsync())
            .Returns(Task.FromResult<IEnumerable<Core.Models.ServiceRole>>(new List<Core.Models.ServiceRole>()));

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
    }

    [TestMethod]
    public async Task GivenOnTeamMemberRolePage_WhenUserIsBasicEmployee_ThenDisplayPageNotFound()
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

    //[TestMethod]
    //public async Task GivenOnTeamMemberRolePage_WhenUserIsBasicAdmin_ThenDisplayPageAsNormal()
    //{
    //    // Arrange
    //    var userData = new UserData
    //    {
    //        ServiceRole = Core.Enums.ServiceRole.Basic.ToString(),
    //        ServiceRoleId = 3,
    //        RoleInOrganisation = PersonRole.Admin.ToString(),
    //    };

    //    SetupBase(userData);

    //    // Act
    //    var result = await SystemUnderTest.TeamMemberPermissions();

    //    // Assert
    //    result.Should().BeOfType<NotFoundResult>();
    //    SessionManagerMock.Verify(m => m.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
    //}
}