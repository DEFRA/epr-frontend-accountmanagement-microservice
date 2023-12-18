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
public class NameOfComplianceSchemeTests : PermissionManagementTestBase
{
    [TestInitialize]
    public void Setup()
    {
        SetupBase();
    }

    [TestMethod]
    public async Task NameOfComplianceScheme_Get_ReturnsViewWithViewModel()
    {
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
                        Journey = new List<string> { $"{PagePath.ChangeAccountPermissions}/{id}", $"{PagePath.RelationshipWithOrganisation}/{id}", $"{PagePath.NameOfComplianceScheme}/{id}" },
                        RelationshipWithOrganisation = RelationshipWithOrganisation.ConsultantFromComplianceScheme
                    }
                }
            }
        };

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).Returns(Task.FromResult<JourneySession?>(journeySession));

        var result = await _systemUnderTest.NameOfComplianceScheme(id);

        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;
        AssertBackLink(viewResult, expectedBackLink);
       
        var model = viewResult.Model;
        model.Should().BeOfType(typeof(NameOfComplianceSchemeViewModel));

        var nameOfComplianceSchemeViewModel = (NameOfComplianceSchemeViewModel?)model;
        nameOfComplianceSchemeViewModel?.Id.Should().Be(id);
    }

    [TestMethod]
    public async Task NameOfComplianceScheme_PostWithValidModel_RedirectToCheckDetailsSendInviteAndSaveSession()
    {
        const string TestComplianceSchemeName = "Test organisation name";
        var id = new Guid("aa5129bc-f86c-40fd-b595-328122c7e67d");
        var request = new NameOfComplianceSchemeViewModel { Id = id, Name = TestComplianceSchemeName };
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
                        RelationshipWithOrganisation = RelationshipWithOrganisation.ConsultantFromComplianceScheme
                    }
                }
            }
        };

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).Returns(Task.FromResult<JourneySession?>(journeySession));

        var result = await _systemUnderTest.NameOfComplianceScheme(request, id);

        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(PermissionManagementController.CheckDetailsSendInvite));

        _sessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
    }

    [TestMethod]
    public async Task NameOfComplianceScheme_PostWithInvalidModel_ValidationFailsWithRequiredErrorMessage()
    {
        const string TestComplianceSchemeName = "";
        var id = new Guid("aa5129bc-f86c-40fd-b595-328122c7e67d");
        var expectedBackLink = $"/manage-account/{PagePath.RelationshipWithOrganisation}/{id}";
        var request = new NameOfComplianceSchemeViewModel { Id = id, Name = TestComplianceSchemeName };

        var journeySession = new JourneySession
        {
            PermissionManagementSession = new PermissionManagementSession
            {
                Items = new List<PermissionManagementSessionItem>
                {
                    new PermissionManagementSessionItem
                    {
                        Id = id,
                        Journey = new List<string> { $"{PagePath.ChangeAccountPermissions}/{id}", $"{PagePath.RelationshipWithOrganisation}/{id}", $"{PagePath.NameOfComplianceScheme}/{id}" },
                        RelationshipWithOrganisation = RelationshipWithOrganisation.ConsultantFromComplianceScheme
                    }
                }
            }
        };

        _systemUnderTest.ModelState.AddModelError("TestKey", "test error message");

        _sessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).Returns(Task.FromResult<JourneySession?>(journeySession));

        var result = await _systemUnderTest.NameOfComplianceScheme(request, id);

        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        AssertBackLink(viewResult, expectedBackLink);

        var model = viewResult.Model;
        model.Should().BeOfType(typeof(NameOfComplianceSchemeViewModel));

        var nameOfComplianceSchemeViewModel = (NameOfComplianceSchemeViewModel?)model;
        nameOfComplianceSchemeViewModel?.Id.Should().Be(id);
    }

    [TestMethod]
    [DataRow(RelationshipWithOrganisation.Consultant)]
    [DataRow(RelationshipWithOrganisation.Employee)]
    [DataRow(RelationshipWithOrganisation.SomethingElse)]
    public async Task NameOfComplianceScheme_WhenNotComplianceScheme_RedirectHome(RelationshipWithOrganisation testRelationship)
    {
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

        var model = new NameOfComplianceSchemeViewModel
        {
            Name = "Test name"
        };
        var result = await _systemUnderTest.NameOfComplianceScheme(model, id) as RedirectToActionResult;
        result.Should().NotBeNull();
        result!.ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));
    }
    [TestMethod]
    public void NameOfComplianceSchemeViewModel_Validation__WhenNameIsNotEmpty_ThenValidationPasses()
    {
        var model = new NameOfComplianceSchemeViewModel
        {
            Name = "Test name"
        };
        var context = new ValidationContext(model, null, null);
        var validationResults = new List<ValidationResult>();
        var isModelStateValid = Validator.TryValidateObject(model, context, validationResults, true);
        isModelStateValid.Should().BeTrue();
        validationResults.Should().BeEmpty();
    }
    [TestMethod]
    public void NameOfComplianceSchemeViewModel_Validation__WhenNameIsEmpty_ValidationFailsWithRequiredErrorMessage()
    {
        var model = new NameOfComplianceSchemeViewModel
        {
            Name = null!
        };
        var context = new ValidationContext(model, null, null);
        var validationResults = new List<ValidationResult>();
        var isModelStateValid = Validator.TryValidateObject(model, context, validationResults, true);
        isModelStateValid.Should().BeFalse();
        validationResults.Should().ContainSingle();
        validationResults.First().ErrorMessage.Should().Be("NameOfComplianceScheme.RequiredErrorMessage");
    }
    [TestMethod]
    public void NameOfComplianceSchemeViewModel_Validation__WhenNameExceeds160Characters_ValidationFailsWithLengthErrorMessage()
    {
        var model = new NameOfComplianceSchemeViewModel
        {
            Name = new string('X', 170)
        };
        var context = new ValidationContext(model, null, null);
        var validationResults = new List<ValidationResult>();
        var isModelStateValid = Validator.TryValidateObject(model, context, validationResults, true);
        isModelStateValid.Should().BeFalse();
        validationResults.Should().ContainSingle();
        validationResults.First().ErrorMessage.Should().Be("NameOfComplianceScheme.LengthErrorMessage");
    }
}