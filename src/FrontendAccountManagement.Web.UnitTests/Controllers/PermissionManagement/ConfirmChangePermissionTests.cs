using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Controllers.PermissionManagement;
using FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement;
using FrontendAccountManagement.Web.ViewModels.PermissionManagement;
using FrontendAccountManagement.Web.Controllers.AccountManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using FrontendAccountManagement.Core.Enums;
using FrontendAccountManagement.Web.ViewModels;
using System.Text.Json;
using EPR.Common.Authorization.Models;
using System.Security.Claims;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.PermissionManagement;

[TestClass]
public class ConfirmChangePermissionTests : PermissionManagementTestBase
{
    [TestInitialize]
    public void Setup()
    {
        SetupBase();
    }

    [TestMethod]
    [DataRow(EnrolmentStatus.Approved)]
    [DataRow(EnrolmentStatus.Invited)]
    [DataRow(EnrolmentStatus.Enrolled)]
    [DataRow(EnrolmentStatus.Pending)]
    [DataRow(EnrolmentStatus.Rejected)]
    [DataRow(EnrolmentStatus.NotSet)]
    public async Task ChangeAccountPermission_PageLoad_CorrectModelForEnrolmentStatus(EnrolmentStatus enrolmentStatus)
    {
        var id = new Guid("6abba000-1234-1234-1234-6abba6abba00");

        var journeySession = new JourneySession
        {
            PermissionManagementSession = new PermissionManagementSession
            {
                Items = new List<PermissionManagementSessionItem>
                {
                    new PermissionManagementSessionItem
                    {
                        Id = id
                    }
                }
            }
        };

        var person = new Core.Models.ConnectionPerson { FirstName = "Joey", LastName = "Ramone" };

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).Returns(Task.FromResult<JourneySession?>(journeySession));
        _facadeServiceMock.Setup(x => x.GetPersonDetailsFromConnectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(person));
        _facadeServiceMock.Setup(x => x.GetEnrolmentStatus(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), Core.Constants.ServiceRoles.Packaging.DelegatedPerson)).Returns(Task.FromResult((EnrolmentStatus?)enrolmentStatus));
        _facadeServiceMock.Setup(x => x.GetNationIds(It.IsAny<Guid>())).Returns(Task.FromResult(new List<int>{1}));

        var result = await _systemUnderTest.ConfirmChangePermission(id) as ViewResult;

        result.Model.Should().BeOfType<ConfirmChangePermissionViewModel>();

        var model = result.Model as ConfirmChangePermissionViewModel;
        model.Id.Should().Be(id);
        model.DisplayName.Should().Be("Joey Ramone");
        model.ApprovedByRegulator.Should().Be(enrolmentStatus == EnrolmentStatus.Approved);
        model.NationIds.Should().Contain(1);
    }

    [TestMethod]
    [DataRow(EnrolmentStatus.Pending)]
    public async Task ChangeAccountPermission_PageLoadWithInvalidUserOrganisation_CorrectModelForEnrolmentStatus(EnrolmentStatus enrolmentStatus)
    {
        // Arrange
        var id = new Guid("6abba000-1234-1234-1234-6abba6abba00");

        var userData = new UserData
        {
            Id = Guid.NewGuid(),
            Organisations = new List<Organisation>
             {
                 new Organisation ()
             }
        };

        var userDataString = JsonSerializer.Serialize(userData);
        var identity = new ClaimsIdentity();
        identity.AddClaims(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.UserData, userDataString)
            });
        var user = new ClaimsPrincipal(identity);

        _httpContextMock.Setup(x => x.User).Returns(user);

        // Act
        var result = await _systemUnderTest.ConfirmChangePermission(id);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));
    }

    [TestMethod]
    [DataRow(YesNoAnswer.Yes)]
    [DataRow(YesNoAnswer.No)]
    public async Task ChangeAccountPermission_ChangeRole_RedirectsToManageAccount(YesNoAnswer answer)
    {
        var id = new Guid("aa5129bc-f86c-40fd-b595-328122c7e67d");
        var request = new ConfirmChangePermissionViewModel { Id = id, ConfirmAnswer = answer };

        var journeySession = new JourneySession
        {
            PermissionManagementSession = new PermissionManagementSession
            {
                Items = new List<PermissionManagementSessionItem> { new() { Id = id } }
            }
        };

        var person = new Core.Models.ConnectionPerson { FirstName = "Joey", LastName = "Ramone" };

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).Returns(Task.FromResult<JourneySession?>(journeySession));
        _facadeServiceMock.Setup(x => x.GetPersonDetailsFromConnectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(person));
        _facadeServiceMock.Setup(x => x.GetEnrolmentStatus(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), Core.Constants.ServiceRoles.Packaging.DelegatedPerson))
            .Returns(Task.FromResult((EnrolmentStatus?)EnrolmentStatus.Approved));
        _facadeServiceMock.Setup(x => x.UpdatePersonRoleAdminOrEmployee(It.IsAny<Guid>(), It.IsAny<PersonRole>(), It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();

        var result = await _systemUnderTest.ConfirmChangePermission(request, id);

        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
    }

    [TestMethod]
    public async Task ChangeAccountPermission_ConfirmingChange_ChangesRole()
    {
        var id = new Guid("aa5129bc-f86c-40fd-b595-328122c7e67d");
        var request = new ConfirmChangePermissionViewModel { Id = id, ConfirmAnswer = YesNoAnswer.Yes };

        var journeySession = new JourneySession
        {
            PermissionManagementSession = new PermissionManagementSession
            {
                Items = new List<PermissionManagementSessionItem> { new() { Id = id } }
            }
        };

        var person = new Core.Models.ConnectionPerson { FirstName = "Joey", LastName = "Ramone" };

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).Returns(Task.FromResult<JourneySession?>(journeySession));
        _facadeServiceMock.Setup(x => x.GetPersonDetailsFromConnectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(person));
        _facadeServiceMock.Setup(x => x.GetEnrolmentStatus(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), Core.Constants.ServiceRoles.Packaging.DelegatedPerson))
            .Returns(Task.FromResult((EnrolmentStatus?)EnrolmentStatus.Pending));
        _facadeServiceMock.Setup(x => x.UpdatePersonRoleAdminOrEmployee(It.IsAny<Guid>(), It.IsAny<PersonRole>(), It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();

        await _systemUnderTest.ConfirmChangePermission(request, id);

        _facadeServiceMock.Verify(x => x.UpdatePersonRoleAdminOrEmployee(It.IsAny<Guid>(), It.IsAny<PersonRole>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Once);
    }

    [TestMethod]
    public async Task ChangeAccountPermission_NotConfirmingChange_DoesntChangeRole()
    {
        var id = new Guid("aa5129bc-f86c-40fd-b595-328122c7e67d");
        var request = new ConfirmChangePermissionViewModel { Id = id, ConfirmAnswer = YesNoAnswer.No };

        var journeySession = new JourneySession
        {
            PermissionManagementSession = new PermissionManagementSession
            {
                Items = new List<PermissionManagementSessionItem> { new() { Id = id } }
            }
        };

        var person = new Core.Models.ConnectionPerson { FirstName = "Joey", LastName = "Ramone" };

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).Returns(Task.FromResult<JourneySession?>(journeySession));
        _facadeServiceMock.Setup(x => x.GetPersonDetailsFromConnectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(person));
        _facadeServiceMock.Setup(x => x.GetEnrolmentStatus(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), Core.Constants.ServiceRoles.Packaging.DelegatedPerson))
            .Returns(Task.FromResult((EnrolmentStatus?)EnrolmentStatus.Pending));
        _facadeServiceMock.Setup(x => x.UpdatePersonRoleAdminOrEmployee(It.IsAny<Guid>(), It.IsAny<PersonRole>(), It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();

        await _systemUnderTest.ConfirmChangePermission(request, id);

        _facadeServiceMock.Verify(x => x.UpdatePersonRoleAdminOrEmployee(It.IsAny<Guid>(), It.IsAny<PersonRole>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task ConfirmChangePermissionn_WhenNothingIsSelected_PageReturnsAnError()
    {

        var id = new Guid("aa5129bc-f86c-40fd-b595-328122c7e67d");
        var request = new ConfirmChangePermissionViewModel
        {
            Id = id,
            ConfirmAnswer = null
        };
        var journeySession = new JourneySession
        {
            PermissionManagementSession = new PermissionManagementSession
            {
                Items = new List<PermissionManagementSessionItem> { new() { Id = id } }
            }
        };

        var person = new Core.Models.ConnectionPerson { FirstName = "Joey", LastName = "Ramone" };

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).Returns(Task.FromResult<JourneySession?>(journeySession));
        _facadeServiceMock.Setup(x => x.GetPersonDetailsFromConnectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(person));
        _facadeServiceMock.Setup(x => x.GetEnrolmentStatus(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), Core.Constants.ServiceRoles.Packaging.DelegatedPerson))
            .Returns(Task.FromResult((EnrolmentStatus?)EnrolmentStatus.Approved));

        var result = await _systemUnderTest.ConfirmChangePermission(request, id) as ViewResult;

        result.ViewData.ModelState.IsValid.Should().Be(false);
    }

    [TestMethod]
    [DataRow(YesNoAnswer.Yes)]
    public async Task ChangeAccountPermission_WithInvalidUserOrganisationRole_RedirectsToManageAccount(YesNoAnswer answer)
    {
        // Arrange
        var id = new Guid("aa5129bc-f86c-40fd-b595-328122c7e67d");
        var request = new ConfirmChangePermissionViewModel { Id = id, ConfirmAnswer = answer };

        var journeySession = new JourneySession
        {
            PermissionManagementSession = new PermissionManagementSession
            {
                Items = new List<PermissionManagementSessionItem> { new() { Id = id } }
            }
        };

        var userData = new UserData
        {
            Id = Guid.NewGuid(),
            FirstName = "FName",
            LastName = "LName",
            Organisations = new List<Organisation>()
            {
                new()
                {
                    Id = null
                }
            }
        };

        var userDataString = JsonSerializer.Serialize(userData);
        var identity = new ClaimsIdentity();
        identity.AddClaims(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim(ClaimTypes.UserData, userDataString)
            });
        var user = new ClaimsPrincipal(identity);

        _httpContextMock.Setup(x => x.User).Returns(user);

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).Returns(Task.FromResult<JourneySession?>(journeySession));

        // Act
        var result = await _systemUnderTest.ConfirmChangePermission(request, id);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));
    }
}