using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Controllers.AccountManagement;
using FrontendAccountManagement.Web.Controllers.PermissionManagement;
using FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement;
using FrontendAccountManagement.Web.ViewModels.PermissionManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.PermissionManagement;

[TestClass]
public class NameOfConsultancyTests : PermissionManagementTestBase
{
    [TestInitialize]
    public void Setup()
    {
        SetupBase();
    }

    [TestMethod]
    public async Task NameOfConsultancy_Get_ReturnsViewWithViewModel()
    {
        // Arrange
        var id = new Guid("aa5129bc-f86c-40fd-b595-328122c7e67d");
        var expectedBackLink = $"/manage-account/{PagePath.RelationshipWithOrganisation}/{id}";

        var journeySession = new JourneySession
        {
            PermissionManagementSession = new PermissionManagementSession
            {
                Items = new List<PermissionManagementSessionItem>
                {
                    new PermissionManagementSessionItem
                    {
                        Id = id,
                        Journey = new List<string> { $"{PagePath.ChangeAccountPermissions}/{id}", $"{PagePath.RelationshipWithOrganisation}/{id}", $"{PagePath.NameOfConsultancy}/{id}" },
                        RelationshipWithOrganisation = RelationshipWithOrganisation.Consultant
                    }
                }
            }
        };

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).Returns(Task.FromResult<JourneySession?>(journeySession));

        // Act
        var result = await _systemUnderTest.NameOfConsultancy(id) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        result!.Should().BeOfType<ViewResult>();      
        AssertBackLink(result, expectedBackLink);
       
        var model = result.Model;
        model.Should().BeOfType(typeof(NameOfConsultancyViewModel));

        var nameOfConsultancyViewModel = (NameOfConsultancyViewModel?)model;
        nameOfConsultancyViewModel?.Id.Should().Be(id);
    }

    [TestMethod]
    public async Task NameOfConsultancy_PostWithValidModel_RedirectToCheckDetailsSendInviteAndSaveSession()
    {
        // Arrange
        const string TestConsultancyName = "Test organisation name";
        var id = new Guid("aa5129bc-f86c-40fd-b595-328122c7e67d");
        var request = new NameOfConsultancyViewModel { Id = id, Name = TestConsultancyName };
        var journeySession = new JourneySession
        {
            PermissionManagementSession = new PermissionManagementSession
            {
                Items = new List<PermissionManagementSessionItem>
                {
                    new PermissionManagementSessionItem
                    {
                        Id = id,
                        Journey = new List<string> { $"{PagePath.ChangeAccountPermissions}/{id}", $"{PagePath.RelationshipWithOrganisation}/{id}" },
                        RelationshipWithOrganisation = RelationshipWithOrganisation.Consultant
                    }
                }
            }
        };

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).Returns(Task.FromResult<JourneySession?>(journeySession));

        // Act
        var result = await _systemUnderTest.NameOfConsultancy(request, id) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        result!.Should().BeOfType<RedirectToActionResult>();
        result!.ActionName.Should().Be(nameof(PermissionManagementController.CheckDetailsSendInvite));
        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
    }

    [TestMethod]
    public async Task NameOfConsultancy_PostWithInvalidModel_ReturnViewWithViewModel()
    {
        // Arrange
        const string TestConsultancyName = "";
        var id = new Guid("aa5129bc-f86c-40fd-b595-328122c7e67d");
        var expectedBackLink = $"/manage-account/{PagePath.RelationshipWithOrganisation}/{id}";
        var request = new NameOfConsultancyViewModel { Id = id, Name = TestConsultancyName };

        var journeySession = new JourneySession
        {
            PermissionManagementSession = new PermissionManagementSession
            {
                Items = new List<PermissionManagementSessionItem>
                {
                    new PermissionManagementSessionItem
                    {
                        Id = id,
                        Journey = new List<string> { $"{PagePath.ChangeAccountPermissions}/{id}", $"{PagePath.RelationshipWithOrganisation}/{id}", $"{PagePath.NameOfConsultancy}/{id}" },
                        RelationshipWithOrganisation = RelationshipWithOrganisation.Consultant
                    }
                }
            }
        };

        _systemUnderTest.ModelState.AddModelError("TestKey", "test error message");

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).Returns(Task.FromResult<JourneySession?>(journeySession));

        // Act
        var result = await _systemUnderTest.NameOfConsultancy(request, id) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        result!.Should().BeOfType<ViewResult>();         

        AssertBackLink(result, expectedBackLink);

        var model = result!.Model;
        model.Should().BeOfType(typeof(NameOfConsultancyViewModel));

        var nameOfConsultancyViewModel = (NameOfConsultancyViewModel?)model;
        nameOfConsultancyViewModel?.Id.Should().Be(id);
    }

    [TestMethod]
    [DataRow(RelationshipWithOrganisation.ConsultantFromComplianceScheme)]
    [DataRow(RelationshipWithOrganisation.Employee)]
    [DataRow(RelationshipWithOrganisation.SomethingElse)]
    public async Task NameOfConsultancy_WhenNotConsultancy_RedirectHome(RelationshipWithOrganisation testRelationship)
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
                        RelationshipWithOrganisation = testRelationship
                    }
                }
            }
        };
        _sessionManagerMock.Setup(sessionManager => sessionManager.GetSessionAsync(It.IsAny<ISession>())).Returns(Task.FromResult<JourneySession?>(journeySession));

        var model = new NameOfConsultancyViewModel
        {
            Name = "Test name"
        };

        // Act
        var result = await _systemUnderTest.NameOfConsultancy(model, id) as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        result!.Should().BeOfType<RedirectToActionResult>();
        result!.ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));
    }

    [TestMethod]
    public void NameOfConsultancyViewModel_Validation__WhenNameIsNotEmpty_ThenValidationPasses()
    {
        // Arrange
        var model = new NameOfConsultancyViewModel
        {
            Name = "Test name"
        };
        var context = new ValidationContext(model, null, null);
        var validationResults = new List<ValidationResult>();

        // Act
        var isModelStateValid = Validator.TryValidateObject(model, context, validationResults, true);

        // Assert
        isModelStateValid.Should().BeTrue();
        validationResults.Should().BeEmpty();
    }

    [TestMethod]
    public void NameOfConsultancyViewModel_Validation__WhenNameIsEmpty_ValidationFailsWithRequiredErrorMessage()
    {
        // Arrange
        var model = new NameOfConsultancyViewModel
        {
            Name = null!
        };
        var context = new ValidationContext(model, null, null);
        var validationResults = new List<ValidationResult>();

        // Act
        var isModelStateValid = Validator.TryValidateObject(model, context, validationResults, true);

        // Assert
        isModelStateValid.Should().BeFalse();
        validationResults.Should().ContainSingle();
        validationResults.First().ErrorMessage.Should().Be("NameOfConsultancy.RequiredErrorMessage");
    }

    [TestMethod]
    public void NameOfConsultancyViewModel_Validation__WhenNameExceeds160Characters_ValidationFailsWithLengthErrorMessage()
    {
        // Arrange
        var model = new NameOfConsultancyViewModel
        {
            Name = new string('X', 170)
        };
        var context = new ValidationContext(model, null, null);
        var validationResults = new List<ValidationResult>();

        // Act
        var isModelStateValid = Validator.TryValidateObject(model, context, validationResults, true);

        // Assert
        isModelStateValid.Should().BeFalse();
        validationResults.Should().ContainSingle();
        validationResults.First().ErrorMessage.Should().Be("NameOfConsultancy.LengthErrorMessage");
    }

    [TestMethod]
    public async Task NameOfConsultancy_GetWithInvalidRelationshipWithOrganisation_ReturnsViewWithViewModel()
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
                        Journey = new List<string> { $"{PagePath.ChangeAccountPermissions}/{id}", $"{PagePath.RelationshipWithOrganisation}/{id}", $"{PagePath.NameOfConsultancy}/{id}" },
                        RelationshipWithOrganisation = RelationshipWithOrganisation.Employee
                    }
                }
            }
        };

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).Returns(Task.FromResult<JourneySession?>(journeySession));

        // Act
        var result = await _systemUnderTest.NameOfConsultancy(id) as ViewResult;

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task NameOfConsultancy_GetWithEmptyRelationshipWithOrganisation_ReturnsViewWithViewModel()
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

        // Act
        var result = await _systemUnderTest.NameOfConsultancy(id) as ViewResult;

        // Assert
        Assert.IsNull(result);
    }
}