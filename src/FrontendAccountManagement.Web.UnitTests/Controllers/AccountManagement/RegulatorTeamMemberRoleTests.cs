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

    private readonly ServiceRole RegulatorTestRole = new()
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
        FacadeServiceMock.Setup(x => x.GetAllServiceRolesAsync())
            .Returns(Task.FromResult<IEnumerable<ServiceRole>>(new List<ServiceRole>
            {
                RegulatorTestRole
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
            .Returns(Task.FromResult<IEnumerable<ServiceRole>>(null));

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
            .Returns(Task.FromResult<IEnumerable<ServiceRole>>(new List<ServiceRole>()));

        // Act
        var result = await SystemUnderTest.TeamMemberPermissions() as ViewResult;

        // Assert
        result.ViewName.Should().Be(ViewName);
    }
}