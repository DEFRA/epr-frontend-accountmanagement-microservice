using EPR.Common.Authorization.Models;
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
public class TeamMemberDetailTests : AccountManagementTestBase
{
    private const string organisationName = "test organisation";
    private const string ViewName = "TeamMemberDetails";
    private const string Email = "unit@test.com";
    private const string SelectedUserRole = "Basic.Employee";
    private UserData _userData;

    [TestInitialize]
    public void Setup()
    {
        _userData = new UserData
        {
            FirstName = "FName",
            LastName = "LName",
            Organisations = new List<Organisation>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Name"
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
                    PagePath.TeamMemberEmail,
                    PagePath.TeamMemberPermissions,
                    PagePath.TeamMemberDetails
                },
                OrganisationName = organisationName,
                InviteeEmailAddress = "unit@test.com",
                RoleKey = "Basic.Employee",
            }
        };

        SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
            .ReturnsAsync(JourneySessionMock);
    }
    
    [TestMethod]
    public async Task GivenOnTeamMemberDetailsPage_WhenTeamMemberDetailsPageHttpGetCalled_ThenTeamMemberDetailsViewModelReturned_AndBackLinkSet()
    {
        // Act
        var result = await SystemUnderTest.TeamMemberDetails() as ViewResult;
        var model = result.Model as TeamMemberDetailsViewModel;

        // Assert
        result.ViewName.Should().Be(ViewName);
        AssertBackLink(result, PagePath.TeamMemberPermissions);
        
        Assert.AreEqual(Email, model.Email);
        Assert.AreEqual(SelectedUserRole, model.SelectedUserRole);
    }
    
    [TestMethod]
    public async Task GivenOnTeamMemberDetailsPage_WhenTeamMemberDetailsHttpPostCalled_ThenRedirectToManageAccount_AndUpdateSession()
    {
        // Act
        var result = await SystemUnderTest.TeamMemberDetailsSubmission() as RedirectToActionResult;

        // Assert
        result.ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));
        SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
    }
    
    [TestMethod]
    public async Task GivenOnTeamMemberDetailsPage_WhenTeamMemberDetailsHttpPostCalled_AndSendInviteFailed_ThenRedirectToManageAccount_AndUpdateSession()
    {
        // Act
        var request = new InviteUserRequest
        {
            InvitingUser = new()
            {
                FirstName = "Fname",
                LastName = "Lname"
            },
            InvitedUser = new ()
            {
                
                Email = "test@abc.com"
            }
        };
        FacadeServiceMock.Setup(x => x.SendUserInvite(request))
            .Throws(new Exception());

        var result = await SystemUnderTest.TeamMemberDetailsSubmission() as RedirectToActionResult;

        // Assert
        result.ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));
        SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
    }

    [TestMethod]
    public async Task GivenOnTeamMemberDetailsPage_WhenOrganisationIsNull_ThenRedirectToManageAccount()
    {
        // Arrange 
        _userData = new UserData
        {
            FirstName = "FName",
            LastName = "LName",
            Organisations = new List<Organisation>()
        };

        SetupBase(_userData);

        var request = new InviteUserRequest
        {
            InvitingUser = new()
            {
                FirstName = "Fname",
                LastName = "Lname"
            },
            InvitedUser = new()
            {
                Email = "test@abc.com"
            }
        };

        FacadeServiceMock.Setup(x => x.SendUserInvite(request))
            .Throws(new Exception());

        // Act
        var result = await SystemUnderTest.TeamMemberDetailsSubmission();

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));
    }
}