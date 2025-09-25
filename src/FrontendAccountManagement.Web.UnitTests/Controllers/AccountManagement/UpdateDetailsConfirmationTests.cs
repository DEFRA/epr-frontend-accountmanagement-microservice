using EPR.Common.Authorization.Constants;
using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Controllers.Errors;
using FrontendAccountManagement.Web.Extensions;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Net;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement
{
    [TestClass]
    public class UpdateDetailsConfirmationTests : AccountManagementTestBase
    {
        private UserData _userData;

        [TestInitialize]
        public void Setup()
        {
            _userData = new UserData
            {
                FirstName = "Dwight",
                LastName = "Schrute",
            };

            SetupBase(_userData);
           
            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
                .Returns(Task.FromResult(new JourneySession { UserData = _userData }));
        }

        [TestMethod]
        public async Task UpdateDetailsConfirmatio_WhenIsChangeRequestPending_RedirectsToActionForbidden()
        {
            // Arrange
            var userData = new UserData
            {
                IsChangeRequestPending = true,
            };
            SetupBase(userData: userData);
            // Act
            var result = await SystemUnderTest.UpdateDetailsConfirmation();
            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual(nameof(ErrorController.Error).ToLower(), redirectResult.ActionName);
            Assert.AreEqual("Error", redirectResult.ControllerName);
            Assert.AreEqual((int)HttpStatusCode.Forbidden, redirectResult.RouteValues["statusCode"]);
        }



        [TestMethod]
        public async Task UpdateDetailsConfirmation_ShouldReturnViewWithExpectedModel()
        {
            // Act
            var result = await SystemUnderTest.UpdateDetailsConfirmation();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.ViewName.Should().Be(null);
            var model = viewResult.Model as UpdateDetailsConfirmationViewModel;
            model.Should().NotBeNull();
            model.Username.Should().Be("Dwight Schrute");
            var changedDateAt = DateTime.UtcNow;
            model.UpdatedDatetime.Should().BeCloseTo(changedDateAt.UtcToGmt(), TimeSpan.FromSeconds(1));
        }

    }
}
