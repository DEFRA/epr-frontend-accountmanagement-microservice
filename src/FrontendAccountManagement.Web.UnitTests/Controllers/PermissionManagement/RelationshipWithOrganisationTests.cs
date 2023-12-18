using System.Security.Claims;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Controllers.AccountManagement;
using FrontendAccountManagement.Web.Controllers.PermissionManagement;
using FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement;
using FrontendAccountManagement.Web.ViewModels.PermissionManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.PermissionManagement;

[TestClass]
public class RelationshipWithOrganisationTests : PermissionManagementTestBase
{
    [TestInitialize]
    public void Setup()
    {
        SetupBase();
        _httpContextMock.Setup(x=> x.User.Claims).Returns(new List<Claim>
        {
            new ("IsComplianceScheme", "true")
        }.AsEnumerable());
    }

    [TestMethod]
    [DataRow(RelationshipWithOrganisation.Employee)]
    [DataRow(RelationshipWithOrganisation.Consultant)]
    [DataRow(RelationshipWithOrganisation.ConsultantFromComplianceScheme)]
    [DataRow(RelationshipWithOrganisation.SomethingElse)]
    public async Task RelationshipWithOrganisation_ModelStateIsValid_AndEmployeeIsChosen_RedirectsToJobTitle(RelationshipWithOrganisation relation)
    {
        // Arrange
        var shortString = new string('a',50);
        var id = new Guid("aa5129bc-f86c-40fd-b595-328122c7e67d");
        var request = new RelationshipWithOrganisationViewModel
        {
            IsComplianceScheme = true, 
            Id = id,
            SelectedRelationshipWithOrganisation = relation,
            AdditionalRelationshipInformation = shortString
        };
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

        // Act
        var result = await _systemUnderTest.RelationshipWithOrganisation(request, id);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();
        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(PermissionManagementController.JobTitle));
    }

    [TestMethod]
    public async Task RelationshipWithOrganisation_WhenNothingIsSelected_PageReturnsAnError()
    {
        // Arrange
        var id = new Guid("aa5129bc-f86c-40fd-b595-328122c7e67d");
        var request = new RelationshipWithOrganisationViewModel
        {
            IsComplianceScheme = true, Id = id,
            SelectedRelationshipWithOrganisation = RelationshipWithOrganisation.NotSet
        };
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

        // Act
        var result = await _systemUnderTest.RelationshipWithOrganisation(request, id) as ViewResult;

        // Assert
        Assert.AreEqual(expected: false, actual: result.ViewData.ModelState.IsValid);
    }
       
    [TestMethod]
    public async Task RelationshipWithOrganisation_WhenSomethingElseIsSelectedButNoTextAdded_PageReturnsAnError()
    {
        // Arrange
        var id = new Guid("aa5129bc-f86c-40fd-b595-328122c7e67d");
        var request = new RelationshipWithOrganisationViewModel
        {
            IsComplianceScheme = true, Id = id,
            SelectedRelationshipWithOrganisation = RelationshipWithOrganisation.SomethingElse,
            AdditionalRelationshipInformation = String.Empty
        };
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

        // Act
        var result = await _systemUnderTest.RelationshipWithOrganisation(request, id) as ViewResult;

        // Assert
        Assert.AreEqual(expected: false, actual: result.ViewData.ModelState.IsValid);
    }
    
    [TestMethod]
    public async Task RelationshipWithOrganisation_WhenSomethingElseIsSelectedButCharacterLimitedExceeded_PageReturnsAnError()
    {
        // Arrange
        var longString = new string('a', 451);
        var id = new Guid("aa5129bc-f86c-40fd-b595-328122c7e67d");
        var request = new RelationshipWithOrganisationViewModel
        {
            IsComplianceScheme = true, Id = id,
            SelectedRelationshipWithOrganisation = RelationshipWithOrganisation.SomethingElse,
            AdditionalRelationshipInformation = longString
        };
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

        // Act
        var result = await _systemUnderTest.RelationshipWithOrganisation(request, id) as ViewResult;

        // Assert
        Assert.AreEqual(expected: false, actual: result.ViewData.ModelState.IsValid);
    }

    [TestMethod]
    public async Task RelationshipWithOrganisation_WhenNotSetIsSelectedWithInvalidSessionItem_PageReturnsANull()
    {
        // Arrange
        var id = new Guid("aa5129bc-f86c-40fd-b595-328122c7e67d");
        var request = new RelationshipWithOrganisationViewModel()
        {
            IsComplianceScheme = true,
            Id = id,
            SelectedRelationshipWithOrganisation = RelationshipWithOrganisation.NotSet,
            AdditionalRelationshipInformation = String.Empty
        };
        var journeySession = new JourneySession
        {
            PermissionManagementSession = new PermissionManagementSession
            {
                Items = new List<PermissionManagementSessionItem>
                {
                    new PermissionManagementSessionItem
                    {
                        Id = Guid.NewGuid(),
                    }
                }
            }
        };

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).Returns(Task.FromResult<JourneySession?>(journeySession));

        // Act
        var result = await _systemUnderTest.RelationshipWithOrganisation(request, id) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        result.Should().BeOfType<RedirectToActionResult>();
        result.ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));
    }

    [TestMethod]
    public async Task JobTitle_ValidEmployeeIdProvided_PageReturnsView()
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
                        Id = id,
                        RelationshipWithOrganisation = RelationshipWithOrganisation.Employee
                    }
                }
            }
        };

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).Returns(Task.FromResult<JourneySession?>(journeySession));

        // Act
        var result = await _systemUnderTest.JobTitle(id) as ViewResult;

        // Assert
        result.Should().BeOfType<ViewResult>();
        Assert.AreEqual(expected: true, actual: result.ViewData.ModelState.IsValid);
        var model = (JobTitleViewModel)result.Model;
        Assert.AreEqual(expected: id, actual: model.Id);
    }

    [TestMethod]
    public async Task JobTitle_ValidConsultantIdProvided_PageReturnsView()
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
                        Id = id,
                        RelationshipWithOrganisation = RelationshipWithOrganisation.Consultant
                    }
                }
            }
        };

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).Returns(Task.FromResult<JourneySession?>(journeySession));

        // Act
        var result = await _systemUnderTest.JobTitle(id) as ViewResult;

        // Assert
        Assert.IsNull(result);  
    }
}