using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Core.Models;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Controllers.AccountManagement;
using FrontendAccountManagement.Web.Controllers.PermissionManagement;
using FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement;
using FrontendAccountManagement.Web.ViewModels.PermissionManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Security.Claims;
using System.Text.Json;
using Organisation = EPR.Common.Authorization.Models.Organisation;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.PermissionManagement
{

    [TestClass]
    public class ChangeAccountPermissionsTests : PermissionManagementTestBase
    {
        [TestInitialize]
        public void Setup()
        {
            SetupBase();
        }

        [TestMethod]
        [DataRow(PermissionType.Basic)]
        [DataRow(PermissionType.Admin)]
        [DataRow(PermissionType.Delegated)]
        [DataRow(PermissionType.Basic)]
        [DataRow(PermissionType.Admin)]
        public async Task ChangeAccountPermissions_PermissionDoesNotChange_RedirectsToManageAccount(PermissionType permissionType)
        {
            // Arrange
            var id = new Guid("aa5129bc-f86c-40fd-b595-328122c7e67d");
            var request = new ChangeAccountPermissionViewModel { PermissionType = permissionType, Id = id };

            _facadeServiceMock.Setup(x => x.GetPermissionTypeFromConnectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(Task.FromResult<(PermissionType?, Guid?)>((PermissionType: permissionType, UserId: Guid.NewGuid())));

            // Act
            var result = await _systemUnderTest.ChangeAccountPermissions(request, id);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));
        }

        [TestMethod]
        [DataRow(PermissionType.Admin, PermissionType.Basic)]
        [DataRow(PermissionType.Basic, PermissionType.Admin)]
        public async Task ChangeAccountPermissions_NotApprovedPersonPermissionChanges_RedirectsToManageAccount(PermissionType fromPermissionType, PermissionType toPermissionType)
        {
            // Arrange
            var id = new Guid("aa5129bc-f86c-40fd-b595-328122c7e67d");
            var request = new ChangeAccountPermissionViewModel { PermissionType = toPermissionType, Id = id };

            _facadeServiceMock.Setup(x => x.GetPermissionTypeFromConnectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(Task.FromResult<(PermissionType?, Guid?)>((PermissionType: fromPermissionType, UserId: Guid.NewGuid())));

            var connectionPerson = new ConnectionPerson { FirstName = "Johnny", LastName = "Ramone" };
            _facadeServiceMock.Setup(x => x.GetPersonDetailsFromConnectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>())).Returns(Task.FromResult(connectionPerson));

            _systemUnderTest.TempData = new TempDataDictionary(_httpContextMock.Object, Mock.Of<ITempDataProvider>());

            // Act
            var result = await _systemUnderTest.ChangeAccountPermissions(request, id);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));
            _systemUnderTest.TempData.Should().NotBeNull();
            _systemUnderTest.TempData["personUpdated"].Should().NotBeNull();
            _systemUnderTest.TempData["personUpdated"].ToString().Should().Be("Johnny Ramone");

        }

        [TestMethod]
        [DataRow(PermissionType.Admin)]
        [DataRow(PermissionType.Basic)]
        public async Task ChangeAccountPermissions_ApprovedPersonPermissionChangesFromDelegated_RedirectsToConfirmChangePermission(PermissionType toPermissionType)
        {
            // Arrange
            var id = new Guid("aa5129bc-f86c-40fd-b595-328122c7e67d");
            var request = new ChangeAccountPermissionViewModel { PermissionType = toPermissionType, Id = id };

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

            _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).Returns(Task.FromResult<JourneySession?>(journeySession));

            _facadeServiceMock.Setup(x => x.GetPermissionTypeFromConnectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(Task.FromResult<(PermissionType?, Guid?)>((PermissionType: PermissionType.Delegated, UserId: Guid.NewGuid())));

            // Act
            var result = await _systemUnderTest.ChangeAccountPermissions(request, id);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            ((RedirectToActionResult)result).ActionName.Should().Be(nameof(PermissionManagementController.ConfirmChangePermission));
        }

        [TestMethod]
        public async Task ChangeAccountPermissions_ApprovedPersonPermissionChangesFromDelegated_RedirectsToConfirmChangePermission()
        {
            // Arrange
            var id = new Guid("aa5129bc-f86c-40fd-b595-328122c7e67d");
            var request = new ChangeAccountPermissionViewModel { PermissionType = PermissionType.Admin, Id = id };

            _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).Returns(Task.FromResult<JourneySession?>(null));

            _facadeServiceMock.Setup(x => x.GetPermissionTypeFromConnectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(Task.FromResult<(PermissionType?, Guid?)>((PermissionType: PermissionType.Delegated, UserId: Guid.NewGuid())));

            // Act
            var result = await _systemUnderTest.ChangeAccountPermissions(request, id);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            ((RedirectToActionResult)result).ActionName.Should().Be(nameof(PermissionManagementController.ConfirmChangePermission));
        }

        [TestMethod]
        [DataRow(PermissionType.Admin)]
        [DataRow(PermissionType.Basic)]
        public async Task ChangeAccountPermissions_PermissionChangesToDelegated_RedirectsToRelationshipWithOrganisation(PermissionType fromPermissionType)
        {
            // Arrange
            var id = new Guid("aa5129bc-f86c-40fd-b595-328122c7e67d");
            var request = new ChangeAccountPermissionViewModel { PermissionType = PermissionType.Delegated, Id = id };

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

            _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).Returns(Task.FromResult<JourneySession?>(journeySession));

            _facadeServiceMock.Setup(x => x.GetPermissionTypeFromConnectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(Task.FromResult<(PermissionType?, Guid?)>((PermissionType: fromPermissionType, UserId: Guid.NewGuid())));

            // Act
            var result = await _systemUnderTest.ChangeAccountPermissions(request, id);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            ((RedirectToActionResult)result).ActionName.Should().Be(nameof(PermissionManagementController.RelationshipWithOrganisation));
        }

        [TestMethod]
        [DataRow(PermissionType.Basic)]
        public async Task ChangeAccountPermissions_WithInvalidSession_RedirectsToManageAccount(PermissionType fromPermissionType)
        {
            // Arrange
            var id = new Guid("aa5129bc-f86c-40fd-b595-328122c7e67d");
            var request = new ChangeAccountPermissionViewModel { PermissionType = PermissionType.Delegated, Id = id };

            _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).Returns(Task.FromResult<JourneySession?>(null));

            _facadeServiceMock.Setup(x => x.GetPermissionTypeFromConnectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(Task.FromResult<(PermissionType?, Guid?)>((PermissionType: fromPermissionType, UserId: Guid.NewGuid())));

            // Act
            var result = await _systemUnderTest.ChangeAccountPermissions(request, id);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));            
        }
        
        [TestMethod]
        [DataRow(PermissionType.Basic)]
        public async Task ChangeAccountPermissions_WithEmptySession_RedirectsToRelationshipWithOrganisation(PermissionType fromPermissionType)
        {
            // Arrange
            var id = new Guid("aa5129bc-f86c-40fd-b595-328122c7e67d");
            var request = new ChangeAccountPermissionViewModel { PermissionType = PermissionType.Delegated, Id = id };

            var journeySession = new JourneySession();

            _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).Returns(Task.FromResult<JourneySession?>(journeySession));

            _facadeServiceMock.Setup(x => x.GetPermissionTypeFromConnectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(Task.FromResult<(PermissionType?, Guid?)>((PermissionType: fromPermissionType, UserId: Guid.NewGuid())));

            // Act
            var result = await _systemUnderTest.ChangeAccountPermissions(request, id);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            ((RedirectToActionResult)result).ActionName.Should().Be(nameof(PermissionManagementController.RelationshipWithOrganisation));
        }

        [TestMethod]
        [DataRow(PermissionType.Admin)]
        public async Task ChangeAccountPermissions_WithEmptyOrganisation_RedirectsToRelationshipWithOrganisation(PermissionType fromPermissionType)
        {
            // Arrange
            var id = new Guid("aa5129bc-f86c-40fd-b595-328122c7e67d");
            var request = new ChangeAccountPermissionViewModel { PermissionType = PermissionType.Delegated, Id = id };

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

            var userData = new UserData
            {
                Id = Guid.NewGuid(),
                Organisations = new List<Organisation>
                {
                    new Organisation()
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

            _facadeServiceMock.Setup(x => x.GetPermissionTypeFromConnectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(Task.FromResult<(PermissionType?, Guid?)>((PermissionType: fromPermissionType, UserId: Guid.NewGuid())));

            // Act
            var result = await _systemUnderTest.ChangeAccountPermissions(request, id);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));
        }

        [TestMethod]
        [DataRow(PermissionType.Admin)]
        public async Task ChangeAccountPermissions_WithInvalidModelState_ReturnsViewResult(PermissionType fromPermissionType)
        {
            // Arrange
            var id = new Guid("aa5129bc-f86c-40fd-b595-328122c7e67d");
            var request = new ChangeAccountPermissionViewModel { PermissionType = PermissionType.Delegated, Id = id };

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

            _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).Returns(Task.FromResult<JourneySession?>(journeySession));

            _facadeServiceMock.Setup(x => x.GetPermissionTypeFromConnectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(Task.FromResult<(PermissionType?, Guid?)>((PermissionType: fromPermissionType, UserId: Guid.NewGuid())));

            _systemUnderTest.ModelState.AddModelError("key", "error message");

            // Act
            var result = await _systemUnderTest.ChangeAccountPermissions(request, id) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<ViewResult>();
            ChangeAccountPermissionViewModel model = (ChangeAccountPermissionViewModel)result.Model;            
            Assert.AreEqual(id, model.Id);
        }

        [TestMethod]
        [DataRow(PermissionType.Admin)]
        public async Task ChangeAccountPermissions_WithInvalidCurrentPermission_ReturnsViewResult(PermissionType fromPermissionType)
        {
            // Arrange
            var id = new Guid("aa5129bc-f86c-40fd-b595-328122c7e67d");
            var request = new ChangeAccountPermissionViewModel { PermissionType = PermissionType.Delegated, Id = id };

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

            _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).Returns(Task.FromResult<JourneySession?>(journeySession));

            _facadeServiceMock.Setup(x => x.GetPermissionTypeFromConnectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(Task.FromResult<(PermissionType?, Guid?)>((PermissionType: null, UserId: Guid.NewGuid())));

            // Act
            var result = await _systemUnderTest.ChangeAccountPermissions(request, id);

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<RedirectToActionResult>();
            ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));
        }

        [TestMethod]
        [DataRow(PermissionType.Admin)]
        public async Task ChangeAccountPermissions_WithApprovedCurrentPermission_ReturnsViewResult(PermissionType fromPermissionType)
        {
            // Arrange
            var id = new Guid("aa5129bc-f86c-40fd-b595-328122c7e67d");
            var request = new ChangeAccountPermissionViewModel { PermissionType = PermissionType.Delegated, Id = id };

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

            _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).Returns(Task.FromResult<JourneySession?>(journeySession));

            _facadeServiceMock.Setup(x => x.GetPermissionTypeFromConnectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                .Returns(Task.FromResult<(PermissionType?, Guid?)>((PermissionType: PermissionType.Approved, UserId: Guid.NewGuid())));

            // Act
            var result = await _systemUnderTest.ChangeAccountPermissions(request, id);

            // Assert
            Assert.IsNotNull(result);
            result.Should().BeOfType<RedirectToActionResult>();
            ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));
        }

        [TestMethod]
        public async Task RelationshipWithOrganisationId_ReturnsValidResult_RedirectsToManageAccount()
        {
            // Arrange
            var id = new Guid("aa5129bc-f86c-40fd-b595-328122c7e67d");
            var journeySession = new JourneySession
            {
                PermissionManagementSession = new PermissionManagementSession
                {
                    Items = new List<PermissionManagementSessionItem>
                {
                    new PermissionManagementSessionItem
                    {
                        Id = Guid.NewGuid()
                    }
                }
                }
            };

            _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).Returns(Task.FromResult<JourneySession?>(journeySession));

            _facadeServiceMock.Setup(x => x.GetPermissionTypeFromConnectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                    .Returns(Task.FromResult<(PermissionType?, Guid?)>((PermissionType: PermissionType.Admin, UserId: Guid.NewGuid())));

            // Act
            var result = await _systemUnderTest.RelationshipWithOrganisation(id) as RedirectToActionResult;

            // Assert    
            result.Should().BeOfType<RedirectToActionResult>();
            result!.ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));
        }
        
        [TestMethod]
        public async Task RelationshipWithOrganisationId_ReturnsValidOrganisation_RedirectsToManageAccount()
        {
            // Arrange
            var id = new Guid("aa5129bc-f86c-40fd-b595-328122c7e67d");
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

            _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).Returns(Task.FromResult<JourneySession?>(journeySession));

            _facadeServiceMock.Setup(x => x.GetPermissionTypeFromConnectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                    .Returns(Task.FromResult<(PermissionType?, Guid?)>((PermissionType: PermissionType.Admin, UserId: Guid.NewGuid())));

            // Act
            var result = await _systemUnderTest.RelationshipWithOrganisation(id);

            // Assert
            result.Should().BeOfType<ViewResult>();
            var resultCast = (ViewResult)result;
            RelationshipWithOrganisationViewModel model = (RelationshipWithOrganisationViewModel)resultCast.Model;
            Assert.AreEqual(id, model.Id);  
        }

        [TestMethod]
        public async Task RelationshipWithOrganisationId_ReturnsEmptySessionItem_RedirectsToManageAccount()
        {
            // Arrange
            var id = new Guid("aa5129bc-f86c-40fd-b595-328122c7e67d");
            var journeySession = new JourneySession
            {
                PermissionManagementSession = new PermissionManagementSession
                {
                    Items = new List<PermissionManagementSessionItem>
                    {
                        new PermissionManagementSessionItem()
                    }
                }
            };

            _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).Returns(Task.FromResult<JourneySession?>(journeySession));

            _facadeServiceMock.Setup(x => x.GetPermissionTypeFromConnectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                    .Returns(Task.FromResult<(PermissionType?, Guid?)>((PermissionType: PermissionType.Admin, UserId: Guid.NewGuid())));

            // Act
            var result = await _systemUnderTest.RelationshipWithOrganisation(id);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));
        }

        [TestMethod]
        public async Task RelationshipWithOrganisationId_WithInvalidUserData_ReturnsValidResultAndRedirectsToManageAccount()
        {
            // Arrange
            var id = new Guid("aa5129bc-f86c-40fd-b595-328122c7e67d");
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

            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);

            _httpContextMock.Setup(x => x.User).Returns(user);
            _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).Returns(Task.FromResult<JourneySession?>(journeySession));

            _facadeServiceMock.Setup(x => x.GetPermissionTypeFromConnectionAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>()))
                    .Returns(Task.FromResult<(PermissionType?, Guid?)>((PermissionType: PermissionType.Admin, UserId: Guid.NewGuid())));

            // Act
            var result = await _systemUnderTest.RelationshipWithOrganisation(id);

            // Assert    
            result.Should().BeOfType<RedirectToActionResult>();
            ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));
        }

    }
}

      