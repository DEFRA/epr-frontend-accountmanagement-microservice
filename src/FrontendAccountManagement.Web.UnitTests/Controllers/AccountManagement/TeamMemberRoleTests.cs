using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Controllers.AccountManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using FrontendAccountManagement.Core.Models;
using FrontendAccountManagement.Web.Constants;
using Moq;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement;

[TestClass]
public class TeamMemberRoleTests : AccountManagementTestBase
{
    private const string ViewName = "TeamMemberPermissions";
    private const string ModelErrorValueSelectARole = "Please select a role";
    private const string SelectedRole = "3:2:Manage team and upload data";
    private const string RolesNotFoundException = "Could not retrieve service roles or none found";
    private const string PreviouslyEnteredEmail = "fakeemail@test.com";

    private readonly ServiceRole TestRole = new()
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
        FacadeServiceMock.Setup(x => x.GetAllServiceRolesAsync())
            .Returns(Task.FromResult<IEnumerable<ServiceRole>>(new List<ServiceRole>
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
        // Act
        FacadeServiceMock.Setup(x => x.GetAllServiceRolesAsync())
            .Returns(Task.FromResult<IEnumerable<ServiceRole>>(new List<ServiceRole>()));
        
        // Arrange
        var result = await SystemUnderTest.TeamMemberPermissions() as ViewResult;
        
        // Assert
        result.ViewName.Should().Be(ViewName);
        result.Model.Should().BeOfType<TeamMemberPermissionsViewModel>();
    }
    
    [TestMethod]
    [ExpectedException(typeof(Exception), RolesNotFoundException)]
    public async Task GivenOnTeamMemberRolePage_WhenTeamMemberPermissionsHttpGetCalled_AndExceptionReturnedFromFacade_ThenThrowException()
    {
        // Act
        FacadeServiceMock.Setup(x => x.GetAllServiceRolesAsync())
            .Throws(new Exception());
        
        // Arrange
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
        FacadeServiceMock.Setup(x => x.GetAllServiceRolesAsync())
            .Returns(Task.FromResult<IEnumerable<ServiceRole>>(new List<ServiceRole> { TestRole }));
        
        JourneySessionMock.AccountManagementSession.AddUserJourney.UserRole = role;
        
        // Act
        var result = await SystemUnderTest.TeamMemberPermissions() as ViewResult;
        var model = result.Model as TeamMemberPermissionsViewModel;
        
        // Assert
        Assert.AreEqual(role ,model.SavedUserRole);
    }
}