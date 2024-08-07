using EPR.Common.Authorization.Models;
using System.Security.Claims;
using FrontendAccountManagement.Core.Models;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Controllers.AccountManagement;
using FrontendAccountManagement.Web.Controllers.PermissionManagement;
using FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement;
using FrontendAccountManagement.Web.ViewModels.PermissionManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Text.Json;
using Organisation = EPR.Common.Authorization.Models.Organisation;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.PermissionManagement;

[TestClass]
public class SendInviteTests : PermissionManagementTestBase
{
    [TestInitialize]
    public void Setup()
    {
        SetupBase();
    }

    [TestMethod]
    public async Task SendInvite_WhenValidRequest_ThenReturnViewResult()
    {
        // Arrange
        var id = Guid.NewGuid();

        var journeySession = new JourneySession
        {
            PermissionManagementSession = new PermissionManagementSession
            {
                Items = new List<PermissionManagementSessionItem>
                {
                    new()
                    {
                        Id = id,
                        RelationshipWithOrganisation = RelationshipWithOrganisation.Employee
                    }
                }
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
                    Id = id
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

        var connectionPerson = new ConnectionPerson
        {
            FirstName = "An",
            LastName = "Other"
        };

        _sessionManagerMock.Setup(sessionManager => sessionManager.GetSessionAsync(It.IsAny<ISession>()))
           .Returns(Task.FromResult<JourneySession?>(journeySession));

        _facadeServiceMock.Setup(connectionPerson => connectionPerson.GetPersonDetailsFromConnectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(Task.FromResult<ConnectionPerson>(connectionPerson));

        // Act
        var result = await _systemUnderTest.CheckDetailsSendInvite(id);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<ViewResult>();
        var castResult = (ViewResult)result;
        var castResultModel = (CheckDetailsSendInviteViewModel)castResult.Model;
        Assert.AreEqual(expected: id, actual: castResultModel.Id);
        Assert.AreEqual(expected: $"{connectionPerson.FirstName} {connectionPerson.LastName}", actual: castResultModel.InviteeFullname);
    }

    [TestMethod]
    public async Task SendInvite_WhenInvalidConnectedPerson_ThenRedirectToManageAccount()
    {
        // Arrange
        var id = Guid.NewGuid();

        var journeySession = new JourneySession
        {
            PermissionManagementSession = new PermissionManagementSession
            {
                Items = new List<PermissionManagementSessionItem>
                {
                    new()
                    {
                        Id = id,
                        RelationshipWithOrganisation = RelationshipWithOrganisation.Employee
                    }
                }
            }
        };

        _sessionManagerMock.Setup(sessionManager => sessionManager.GetSessionAsync(It.IsAny<ISession>()))
            .Returns(Task.FromResult<JourneySession?>(journeySession));

        // Act
        var result = await _systemUnderTest.CheckDetailsSendInvite(id);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));
    }

    [TestMethod]
    public async Task SendInvite_WhenInvalidOrganisation_ThenRedirectToManageAccount()
    {
        // Arrange
        var id = Guid.NewGuid();

        var journeySession = new JourneySession
        {
            PermissionManagementSession = new PermissionManagementSession
            {
                Items = new List<PermissionManagementSessionItem>
                {
                    new()
                    {
                        Id = id,
                        RelationshipWithOrganisation = RelationshipWithOrganisation.Employee
                    }
                }
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

        _sessionManagerMock.Setup(sessionManager => sessionManager.GetSessionAsync(It.IsAny<ISession>()))
           .Returns(Task.FromResult<JourneySession?>(journeySession));

        // Act
        var result = await _systemUnderTest.CheckDetailsSendInvite(id);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));
    }

    [TestMethod]
    public async Task SendInvite_WhenInvalidUser_ThenRedirectToManageAccount()
    {
        // Arrange
        var id = Guid.NewGuid();

        var journeySession = new JourneySession
        {
            PermissionManagementSession = new PermissionManagementSession
            {
                Items = new List<PermissionManagementSessionItem>
                {
                    new()
                    {
                        Id = id,
                        RelationshipWithOrganisation = RelationshipWithOrganisation.Employee
                    }
                }
            }
        };

        var identity = new ClaimsIdentity();
        var user = new ClaimsPrincipal(identity);

        _httpContextMock.Setup(x => x.User).Returns(user);        

        _sessionManagerMock.Setup(sessionManager => sessionManager.GetSessionAsync(It.IsAny<ISession>()))
            .Returns(Task.FromResult<JourneySession?>(journeySession));

        // Act
        var result = await _systemUnderTest.CheckDetailsSendInvite(id);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));
    }

    [TestMethod]
    public async Task SendInvite_WhenInvalidSession_ThenRedirectToManageAccount()
    {
        // Arrange
        var id = Guid.NewGuid();

        var journeySession = new JourneySession
        {
            PermissionManagementSession = new PermissionManagementSession()
        };

        _sessionManagerMock.Setup(sessionManager => sessionManager.GetSessionAsync(It.IsAny<ISession>()))
           .Returns(Task.FromResult<JourneySession?>(journeySession));

        // Act
        var result = await _systemUnderTest.CheckDetailsSendInvite(id);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));
    }

    [TestMethod]
    public async Task DetailsSendInvite_WhenValidRequest_ThenReturnViewResult()
    {
        // Arrange
        var id = Guid.NewGuid();

        var journeySession = new JourneySession
        {
            PermissionManagementSession = new PermissionManagementSession
            {
                Items = new List<PermissionManagementSessionItem>
                {
                    new()
                    {
                        Id = id,
                        RelationshipWithOrganisation = RelationshipWithOrganisation.Employee
                    }
                }
            }
        };

        var checkDetailsSendInviteViewModel = new CheckDetailsSendInviteViewModel();

        var userData = new UserData
        {
            Id = Guid.NewGuid(),
            FirstName = "FName",
            LastName = "LName",
            Organisations = new List<Organisation>()
            {
                new()
                {
                    Id = id
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

        var connectionPerson = new ConnectionPerson
        {
            FirstName = "An",
            LastName = "Other"
        };

        _sessionManagerMock.Setup(sessionManager => sessionManager.GetSessionAsync(It.IsAny<ISession>()))
           .Returns(Task.FromResult<JourneySession?>(journeySession));

        _facadeServiceMock.Setup(connectionPerson => connectionPerson.GetPersonDetailsFromConnectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(Task.FromResult<ConnectionPerson>(connectionPerson));

        // Act
        var result = await _systemUnderTest.CheckDetailsSendInvite(checkDetailsSendInviteViewModel, id);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(PermissionManagementController.InvitationToChangeSent));
    }

    [TestMethod]
    public async Task DetailsSendInvite_WhenInvalidModelState_ThenReturnViewResult()
    {
        // Arrange
        var id = Guid.NewGuid();

        var journeySession = new JourneySession
        {
            PermissionManagementSession = new PermissionManagementSession
            {
                Items = new List<PermissionManagementSessionItem>
                {
                    new()
                    {
                        Id = id,
                        RelationshipWithOrganisation = RelationshipWithOrganisation.Employee
                    }
                }
            }
        };

        var checkDetailsSendInviteViewModel = new CheckDetailsSendInviteViewModel();

        var userData = new UserData
        {
            Id = Guid.NewGuid(),
            FirstName = "FName",
            LastName = "LName",
            Organisations = new List<Organisation>()
            {
                new()
                {
                    Id = id
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

        var connectionPerson = new ConnectionPerson
        {
            FirstName = "An",
            LastName = "Other"
        };

        _sessionManagerMock.Setup(sessionManager => sessionManager.GetSessionAsync(It.IsAny<ISession>()))
           .Returns(Task.FromResult<JourneySession?>(journeySession));

        _facadeServiceMock.Setup(connectionPerson => connectionPerson.GetPersonDetailsFromConnectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(Task.FromResult<ConnectionPerson>(connectionPerson));

        _systemUnderTest.ModelState.AddModelError("key", "error message");
        // Act
        var result = await _systemUnderTest.CheckDetailsSendInvite(checkDetailsSendInviteViewModel, id);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<ViewResult>();
        var castResult = (ViewResult)result;
        var castResultModel = (CheckDetailsSendInviteViewModel)castResult.Model;
        Assert.AreEqual(expected: id, actual: castResultModel.Id);
        Assert.AreEqual(expected: $"{connectionPerson.FirstName} {connectionPerson.LastName}", actual: castResultModel.InviteeFullname);
    }

    [TestMethod]
    public async Task DetailsSendInvite_WhenInvalidConnectedPersonAndInvalidModelState_ThenRedirectToManageAccount()
    {
        // Arrange
        var id = Guid.NewGuid();

        var journeySession = new JourneySession
        {
            PermissionManagementSession = new PermissionManagementSession
            {
                Items = new List<PermissionManagementSessionItem>
                {
                    new()
                    {
                        Id = id,
                        RelationshipWithOrganisation = RelationshipWithOrganisation.Employee
                    }
                }
            }
        };

        var checkDetailsSendInviteViewModel = new CheckDetailsSendInviteViewModel();

        var userData = new UserData
        {
            Id = Guid.NewGuid(),
            FirstName = "FName",
            LastName = "LName",
            Organisations = new List<Organisation>()
            {
                new()
                {
                    Id = id
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

        _sessionManagerMock.Setup(sessionManager => sessionManager.GetSessionAsync(It.IsAny<ISession>()))
           .Returns(Task.FromResult<JourneySession?>(journeySession));

        _facadeServiceMock.Setup(connectionPerson => connectionPerson.GetPersonDetailsFromConnectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(Task.FromResult<ConnectionPerson>(null));

        _systemUnderTest.ModelState.AddModelError("key", "error message");
        // Act
        var result = await _systemUnderTest.CheckDetailsSendInvite(checkDetailsSendInviteViewModel, id);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));
    }

    [TestMethod]
    public async Task DetailsSendInvite_WhenInvalidUserOrganisation_ThenRedirectToManageAccount()
    {
        // Arrange
        var id = Guid.NewGuid();

        var journeySession = new JourneySession
        {
            PermissionManagementSession = new PermissionManagementSession
            {
                Items = new List<PermissionManagementSessionItem>
                {
                    new()
                    {
                        Id = id,
                        RelationshipWithOrganisation = RelationshipWithOrganisation.Employee
                    }
                }
            }
        };

        var checkDetailsSendInviteViewModel = new CheckDetailsSendInviteViewModel();

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

        _sessionManagerMock.Setup(sessionManager => sessionManager.GetSessionAsync(It.IsAny<ISession>()))
           .Returns(Task.FromResult<JourneySession?>(journeySession));
        
        // Act
        var result = await _systemUnderTest.CheckDetailsSendInvite(checkDetailsSendInviteViewModel, id);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));
    }

    [TestMethod]
    public async Task DetailsSendInvite_WhenInvalidSession_ThenRedirectToManageAccount()
    {       
        // Arrange
        var id = Guid.NewGuid();

        var journeySession = new JourneySession
        {
            PermissionManagementSession = new PermissionManagementSession()
        };

        var checkDetailsSendInviteViewModel = new CheckDetailsSendInviteViewModel();

        _sessionManagerMock.Setup(sessionManager => sessionManager.GetSessionAsync(It.IsAny<ISession>()))
           .Returns(Task.FromResult<JourneySession?>(journeySession));

        // Act
        var result = await _systemUnderTest.CheckDetailsSendInvite(checkDetailsSendInviteViewModel,id);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));
    }

    [TestMethod]
    public async Task InvitationToChangeSent_WhenValidRequestAndInvalidSession_ThenReturnActionResult()
    {
        // Arrange
        var id = Guid.NewGuid();

        var userData = new UserData
        {
            Id = Guid.NewGuid(),
            FirstName = "FName",
            LastName = "LName",
            Organisations = new List<Organisation>()
            {
                new()
                {
                    Id = id
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

        var connectionPerson = new ConnectionPerson
        {
            FirstName = "An",
            LastName = "Other"
        };

        _sessionManagerMock.Setup(sessionManager => sessionManager.GetSessionAsync(It.IsAny<ISession>()))
           .Returns(Task.FromResult<JourneySession?>(null));

        _facadeServiceMock.Setup(connectionPerson => connectionPerson.GetPersonDetailsFromConnectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(Task.FromResult<ConnectionPerson>(connectionPerson));

        // Act
        var result = await _systemUnderTest.CheckDetailsSendInvite(id);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));
    }

    [TestMethod]
    public async Task InvitationToChangeSent_WhenValidRequest_ThenReturnViewResult()
    {
        // Arrange
        var id = Guid.NewGuid();

        var journeySession = new JourneySession
        {
            PermissionManagementSession = new PermissionManagementSession
            {
                Items = new List<PermissionManagementSessionItem>
                {
                    new()
                    {
                        Id = id,
                        RelationshipWithOrganisation = RelationshipWithOrganisation.Employee
                    }
                }
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
                    Id = id
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

        var connectionPerson = new ConnectionPerson
        {
            FirstName = "An",
            LastName = "Other"            
        };

        _sessionManagerMock.Setup(sessionManager => sessionManager.GetSessionAsync(It.IsAny<ISession>()))
           .Returns(Task.FromResult<JourneySession?>(journeySession));

        _facadeServiceMock.Setup(connectionPerson => connectionPerson.GetPersonDetailsFromConnectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(Task.FromResult<ConnectionPerson>(connectionPerson));

        // Act
        var result = await _systemUnderTest.InvitationToChangeSent(id);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<ViewResult>();
        var castResult = (ViewResult)result;
        var castResultModel = (InvitationToChangeSentViewModel)castResult.Model;
        Assert.AreEqual(expected: $"{connectionPerson.FirstName} {connectionPerson.LastName}", actual: castResultModel.UserDisplayName);
    }

    [TestMethod]
    public async Task InvitationToChangeSent_WhenInvalidUserOrganisation_ThenReturnViewResult()
    {
        // Arrange
        var id = Guid.NewGuid();

        var journeySession = new JourneySession
        {
            PermissionManagementSession = new PermissionManagementSession
            {
                Items = new List<PermissionManagementSessionItem>
                {
                    new()
                    {
                        Id = id,
                        RelationshipWithOrganisation = RelationshipWithOrganisation.Employee
                    }
                }
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

        var connectionPerson = new ConnectionPerson
        {
            FirstName = "An",
            LastName = "Other"
        };

        _sessionManagerMock.Setup(sessionManager => sessionManager.GetSessionAsync(It.IsAny<ISession>()))
           .Returns(Task.FromResult<JourneySession?>(journeySession));

        _facadeServiceMock.Setup(connectionPerson => connectionPerson.GetPersonDetailsFromConnectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(Task.FromResult<ConnectionPerson>(connectionPerson));

        // Act
        var result = await _systemUnderTest.InvitationToChangeSent(id);

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));
    }
}