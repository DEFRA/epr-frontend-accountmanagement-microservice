using System.ComponentModel.DataAnnotations;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Controllers.PermissionManagement;
using FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement;
using FrontendAccountManagement.Web.ViewModels.PermissionManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.PermissionManagement;

[TestClass]
public class CheckDetailsSendInviteTests : PermissionManagementTestBase
{
    [TestInitialize]
    public void Setup()
    {
        SetupBase();
    }

    [TestMethod]
    public async Task WhenFullNameIsEntered_ThenNextPageIsInvitationToChangeSent()
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
                        RelationshipWithOrganisation = RelationshipWithOrganisation.Employee,
                        JobTitle = "Job Title"
                    }
                }
            }
        };

        _sessionManagerMock.Setup(sessionManager => sessionManager.GetSessionAsync(It.IsAny<ISession>()))
            .Returns(Task.FromResult<JourneySession?>(journeySession));

        var request = new CheckDetailsSendInviteViewModel
        {
            Fullname = "John Smith"
        };

        var result = await _systemUnderTest.CheckDetailsSendInvite(request, id) as RedirectToActionResult;

        result.Should().NotBeNull();

        result!.ActionName.Should().Be(nameof(PermissionManagementController.InvitationToChangeSent));
    }

    [TestMethod]
    public void WhenFullNameIsEmpty_ThenValidationFailsWithRequiredErrorMessage()
    {
        var model = new CheckDetailsSendInviteViewModel
        {
            Fullname = null!
        };

        var context = new ValidationContext(model, null, null);
        var validationResults = new List<ValidationResult>();
        var isModelStateValid = Validator.TryValidateObject(model, context, validationResults, true);

        isModelStateValid.Should().BeFalse();
        validationResults.Should().ContainSingle();
        validationResults.First().ErrorMessage.Should().Be("CheckDetailsSendInvite.FullNameError");
    }

    [TestMethod]
    public void WhenFullNameHasMoreThan200Characters_ThenValidationFailsWithLengthErrorMessage()
    {
        var model = new CheckDetailsSendInviteViewModel
        {
            Fullname = new string('*', 201)
        };

        var context = new ValidationContext(model, null, null);
        var validationResults = new List<ValidationResult>();
        var isModelStateValid = Validator.TryValidateObject(model, context, validationResults, true);

        isModelStateValid.Should().BeFalse();
        validationResults.Should().ContainSingle();
        validationResults.First().ErrorMessage.Should().Be("CheckDetailsSendInvite.FullNameMaxLengthError");
    }
}
