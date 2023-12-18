using System.ComponentModel.DataAnnotations;
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
public class JobTitleTests : PermissionManagementTestBase
{
    [TestInitialize]
    public void Setup()
    {
        SetupBase();
    }

    [TestMethod]
    public async Task WhenJobTitleIsSetForAnEmployee_ThenNextPageIsCheckDetailsSendInvite()
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

        var request = new JobTitleViewModel
        {
            JobTitle = "Owner"
        };

        // Act
        var result = await _systemUnderTest.JobTitle(request, id) as RedirectToActionResult;

        // Assert   
        result.Should().NotBeNull();
        result!.ActionName.Should().Be(nameof(PermissionManagementController.CheckDetailsSendInvite));
    }

    [TestMethod]
    [DataRow(RelationshipWithOrganisation.NotSet)]
    [DataRow(RelationshipWithOrganisation.Consultant)]
    [DataRow(RelationshipWithOrganisation.ConsultantFromComplianceScheme)]
    [DataRow(RelationshipWithOrganisation.SomethingElse)]
    public async Task OtherThanEmployeeRequestsJobTitlePage_ThenRedirectsToHomePage(RelationshipWithOrganisation nonEmployeeRelationship)
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
                        RelationshipWithOrganisation = nonEmployeeRelationship
                    }
                }
            }
        };

        _sessionManagerMock.Setup(sessionManager => sessionManager.GetSessionAsync(It.IsAny<ISession>()))
            .Returns(Task.FromResult<JourneySession?>(journeySession));

        var request = new JobTitleViewModel
        {
            JobTitle = "Owner"
        };

        // Act
        var result = await _systemUnderTest.JobTitle(request, id) as RedirectToActionResult;

        // Assert   
        result.Should().NotBeNull();
        result!.ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));
    }

    [TestMethod]
    public void WhenJobTitleIsNotEmpty_ThenValidationPasses()
    {
        // Arrange
        var model = new JobTitleViewModel
        {
            JobTitle = "Owner"
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
    public void WhenJobTitleIsEmpty_ThenValidationFailsWithRequiredErrorMessage()
    {
        // Arrange
        var model = new JobTitleViewModel
        {
            JobTitle = null!
        };

        var context = new ValidationContext(model, null, null);
        var validationResults = new List<ValidationResult>();

        // Act
        var isModelStateValid = Validator.TryValidateObject(model, context, validationResults, true);

        // Assert   
        isModelStateValid.Should().BeFalse();
        validationResults.Should().ContainSingle();
        validationResults.First().ErrorMessage.Should().Be("JobTitle.RequiredErrorMessage");
    }

    [TestMethod]
    public void WhenJobTitleHasMoreThan450Characters_ThenValidationFailsWithLengthErrorMessage()
    {
        // Arrange
        var model = new JobTitleViewModel
        {
            JobTitle = new string('*', 451)
        };

        var context = new ValidationContext(model, null, null);
        var validationResults = new List<ValidationResult>();

        // Act
        var isModelStateValid = Validator.TryValidateObject(model, context, validationResults, true);

        // Assert   
        isModelStateValid.Should().BeFalse();
        validationResults.Should().ContainSingle();
        validationResults.First().ErrorMessage.Should().Be("JobTitle.LengthErrorMessage");
    }

    [TestMethod]
    [DataRow(RelationshipWithOrganisation.NotSet)]
    [DataRow(RelationshipWithOrganisation.Consultant)]
    [DataRow(RelationshipWithOrganisation.ConsultantFromComplianceScheme)]
    [DataRow(RelationshipWithOrganisation.SomethingElse)]
    public async Task OtherThanEmployeeRequestsJobTitlePage_WithInvalidSession_ThenRedirectsToHomePage(RelationshipWithOrganisation nonEmployeeRelationship)
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
                        Id = Guid.NewGuid(),
                        RelationshipWithOrganisation = nonEmployeeRelationship
                    }
                }
            }
        };

        _sessionManagerMock.Setup(sessionManager => sessionManager.GetSessionAsync(It.IsAny<ISession>()))
            .Returns(Task.FromResult<JourneySession?>(journeySession));

        var request = new JobTitleViewModel
        {
            JobTitle = "Owner"
        };

        // Act
        var result = await _systemUnderTest.JobTitle(request, id) as RedirectToActionResult;

        // Assert   
        result.Should().NotBeNull();
        result!.ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));
    }

    [TestMethod]
    [DataRow(RelationshipWithOrganisation.Employee)]
    public async Task OtherThanEmployeeRequestsJobTitlePage_WithInvalidModelState_ThenRedirectsToHomePage(RelationshipWithOrganisation relationship)
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
                        RelationshipWithOrganisation = relationship
                    }
                }
            }
        };

        _sessionManagerMock.Setup(sessionManager => sessionManager.GetSessionAsync(It.IsAny<ISession>()))
            .Returns(Task.FromResult<JourneySession?>(journeySession));

        var request = new JobTitleViewModel
        {
            JobTitle = "Owner"
        };

        _systemUnderTest.ModelState.AddModelError("key", "error message");

        // Act
        var result = await _systemUnderTest.JobTitle(request, id) as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        result!.Should().BeOfType<ViewResult>();
        JobTitleViewModel model = (JobTitleViewModel)result.Model;
        Assert.AreEqual(expected: request.JobTitle, actual: model.JobTitle);
    }

    [TestMethod]
    public async Task JobTitleIsSetForAnEmployee_WithInvalidModelState_ThenNextPageIsCheckDetailsSendInvite()
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

        var request = new JobTitleViewModel
        {
            JobTitle = "Owner"
        };

        _systemUnderTest.ModelState.AddModelError("TestKey", "test error message");

        // Act
        var result = await _systemUnderTest.JobTitle(request, id);

        // Assert
        Assert.IsNotNull(result);
        result!.Should().BeOfType<ViewResult>();
        var modelcastResult = (ViewResult)result;
        JobTitleViewModel model = (JobTitleViewModel)modelcastResult.Model;
        Assert.AreEqual(expected: request.JobTitle, actual: model.JobTitle);
    }
}
