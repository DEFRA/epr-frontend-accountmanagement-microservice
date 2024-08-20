using System.Net;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using Microsoft.AspNetCore.Mvc;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Controllers.Errors;
using Microsoft.AspNetCore.Http;
using Moq;
using EPR.Common.Authorization.Models;
using System;
using FrontendAccountManagement.Core.Enums;
using Organisation = EPR.Common.Authorization.Models.Organisation;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement
{
    /// <summary>
    /// Tests the Declaration page fuctionality.
    /// </summary>
    [TestClass]
    public class DeclarationTests : AccountManagementTestBase
    {
        [TestInitialize]
        public void Setup()
        {
            SetupBase();
        }

        // /// <summary>
        // /// Check that the declaration page can be accessed when reaching it via the "Check your details" page.
        // /// </summary>
        [TestMethod]
        public async Task Declaration_ReturnsViewWithModel_ForAValidRequest()
        {
            // Arrange
            var userData = new UserData
            {
                ServiceRole = Core.Enums.ServiceRole.Approved.ToString(),
                ServiceRoleId = 1,
                RoleInOrganisation = PersonRole.Admin.ToString(),
            };

            SetupBase(userData);

            // Act
            var result = await SystemUnderTest.Declaration();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = result as ViewResult;
            Assert.IsInstanceOfType(viewResult.Model, typeof(EditUserDetailsViewModel));

            // Verify the session was saved and the back link was set
            SessionManagerMock.Verify(
                m =>
                    m.SaveSessionAsync(
                        It.IsAny<ISession>(),
                        It.IsAny<JourneySession>())
                    , Times.Once);
        }

        /// <summary>
        /// Checks that a bad request returned if the model is not valid.
        /// </summary>
        [TestMethod]
        public async Task Declaration_ReturnsBadRequest_WhenModelStateInvalid()
        {
            // Arrange
            var userData = new UserData
            {
                ServiceRole = Core.Enums.ServiceRole.Approved.ToString(),
                ServiceRoleId = 1,
                RoleInOrganisation = PersonRole.Admin.ToString(),
            };

            SetupBase(userData);

            SystemUnderTest.ModelState.AddModelError("Key", "Error");

            // Act
            var result = await SystemUnderTest.Declaration();

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task Declaration_ShouldReturnNotFound_WhenUserRoleIsBasicEmployee()
        {
            // Arrange
            var userData = new UserData
            {
                ServiceRole = Core.Enums.ServiceRole.Basic.ToString(),
                ServiceRoleId = 3,
                RoleInOrganisation = PersonRole.Employee.ToString(),
            };

            SetupBase(userData);

            // Act
            var result = await SystemUnderTest.Declaration();

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            SessionManagerMock.Verify(m => m.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
        }

        [TestMethod]
        public async Task Declaration_ShouldReturnNotFound_WhenUserRoleIsBasicAdmin()
        {
            // Arrange
            var userData = new UserData
            {
                ServiceRole = Core.Enums.ServiceRole.Basic.ToString(),
                ServiceRoleId = 3,
                RoleInOrganisation = PersonRole.Admin.ToString(),
            };

            SetupBase(userData);

            // Act
            var result = await SystemUnderTest.Declaration();

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            SessionManagerMock.Verify(m => m.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
        }

        [TestMethod]
        public async Task Declaration_ReturnsForbiddenError_WhenChangeRequestPending()
        {
            // Arrange
            var mockUserData = new UserData
            {
                IsChangeRequestPending = true,
                ServiceRole = Core.Enums.ServiceRole.Approved.ToString(),
                ServiceRoleId = 1,
                RoleInOrganisation = PersonRole.Admin.ToString(),
            };

            SetupBase(mockUserData);

            // Act
            var result = await SystemUnderTest.Declaration();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectToActionResult = result as RedirectToActionResult;
            Assert.AreEqual(PagePath.Error, redirectToActionResult.ControllerName.ToLower());
            Assert.AreEqual(nameof(ErrorController.Error).ToLower(), redirectToActionResult.ActionName);
            Assert.AreEqual((int)HttpStatusCode.Forbidden, redirectToActionResult.RouteValues["statusCode"]);
        }

        [TestMethod]
        public async Task Declaration_ShouldThrowInvalidOperationException_WhenRoleInOrganisationIsNullOrEmpty()
        {
            // Arrange
            var userData = new UserData
            {
                ServiceRole = Core.Enums.ServiceRole.Basic.ToString(),
                ServiceRoleId = 3,
                RoleInOrganisation = null,
            };

            Exception result = null;

            SetupBase(userData);

            // Act
            try
            {
                await SystemUnderTest.Declaration();
            }
            catch (Exception ex)
            {
                result = ex;
            }

            // Assert
            result.Should().BeOfType<InvalidOperationException>();
            SessionManagerMock.Verify(m => m.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
        }

        [TestMethod]
        public async Task Declaration_ShouldThrowInvalidOperationException_WhenServiceRoleIdIsDefault()
        {
            // Arrange
            var userData = new UserData
            {
                ServiceRole = Core.Enums.ServiceRole.Basic.ToString(),
                ServiceRoleId = default,
                RoleInOrganisation = Core.Enums.PersonRole.Admin.ToString(),
            };

            Exception result = null;

            SetupBase(userData);

            // Act
            try
            {
                await SystemUnderTest.Declaration();
            }
            catch (Exception ex)
            {
                result = ex;
            }

            // Assert
            result.Should().BeOfType<InvalidOperationException>();
            SessionManagerMock.Verify(m => m.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
        }

        [TestMethod]
        public async Task Declaration_ShouldThrowInvalidOperationException_WhenRoleInOrganisationIsEmpty()
        {
            // Arrange
            var userData = new UserData
            {
                ServiceRole = Core.Enums.ServiceRole.Basic.ToString(),
                ServiceRoleId = 3,
                RoleInOrganisation = string.Empty,
            };

            Exception result = null;

            SetupBase(userData);

            // Act
            try
            {
                await SystemUnderTest.Declaration();
            }
            catch (Exception ex)
            {
                result = ex;
            }

            // Assert
            result.Should().BeOfType<InvalidOperationException>();
            Assert.IsTrue(result.Message == "Unknown role in organisation.");
            SessionManagerMock.Verify(m => m.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
        }

        [TestMethod]
        public async Task Declaration_ShouldThrowInvalidOperationException_WhenRoleInOrganisationIsNull()
        {
            // Arrange
            var userData = new UserData
            {
                ServiceRole = Core.Enums.ServiceRole.Basic.ToString(),
                ServiceRoleId = 3,
                RoleInOrganisation = null,
            };

            Exception result = null;

            SetupBase(userData);

            // Act
            try
            {
                await SystemUnderTest.Declaration();
            }
            catch (Exception ex)
            {
                result = ex;
            }

            // Assert
            result.Should().BeOfType<InvalidOperationException>();
            Assert.IsTrue(result.Message == "Unknown role in organisation.");
            SessionManagerMock.Verify(m => m.GetSessionAsync(It.IsAny<ISession>()), Times.Once);
        }
    }
}