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
using Microsoft.Extensions.Logging;
using FrontendAccountManagement.Core.Models;

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

        // /// <summary>
        // /// Check that the viewmodel returned when accessing the declaration page contains the user data extracted from tempdata..
        // /// </summary>
        [TestMethod]
        public async Task Declaration_ReturnsExistingUserDetailsWhenPresent()
        {
            var firstName = "Bob";
            var lastName = "McPlaceholder";

            // Arrange
            var userData = new UserData
            {
                ServiceRole = Core.Enums.ServiceRole.Approved.ToString(),
                ServiceRoleId = 1,
                RoleInOrganisation = PersonRole.Admin.ToString(),
            };
            
            SetupBase(userData);

            SystemUnderTest.TempData.Add("AmendedUserDetails", $"{{ \"FirstName\": \"{firstName}\", \"LastName\": \"{lastName}\" }}");

            // Act
            var result = await SystemUnderTest.Declaration();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult) result;
            var viewModelResult = (EditUserDetailsViewModel)viewResult.Model;
            Assert.AreEqual(firstName, viewModelResult.FirstName);
            Assert.AreEqual(lastName, viewModelResult.LastName);

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

        /// <summary>
        /// Check that when the user is a basic user, the UpdateUserDetailsRequest page is returned.
        /// </summary>
        [TestMethod]
        public async Task Declaration_Post_UserIsBasicUser()
        {
            // Arrange
            var userData = new UserData
            {
                ServiceRole = Core.Enums.ServiceRole.Basic.ToString(),
                ServiceRoleId = (int)Core.Enums.ServiceRole.Basic,
                RoleInOrganisation = PersonRole.Employee.ToString(),
            };

            SetupBase(userData);

            var viewModel = new Mock<EditUserDetailsViewModel>();

            // Act
            var result = await SystemUnderTest.DeclarationPost(viewModel.Object);


            //Assert
            Assert.IsInstanceOfType<ViewResult>(result);
            var resultViewModel = (EditUserDetailsViewModel)((ViewResult)result).Model;
            Assert.AreSame(viewModel.Object, resultViewModel);
        }

        /// <summary>
        /// Check that update user is an approved user, they're redirected to the details change requested page.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Declaration_Post_UserIsApprovedUser()
        {
            // Arrange
            var userData = new UserData
            {
                Id = Guid.NewGuid(),
                ServiceRole = Core.Enums.ServiceRole.Approved.ToString(),
                ServiceRoleId = (int)Core.Enums.ServiceRole.Approved,
                RoleInOrganisation = PersonRole.Employee.ToString(),
                Organisations = new List<Organisation>
                {
                    new Organisation()
                    {
                        Id = Guid.NewGuid(),
                    }
                }
            };

            this.FacadeServiceMock.Setup(mock => mock.UpdateUserDetailsAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<UpdateUserDetailsRequest>()))
                .Returns(Task.FromResult(new UpdateUserDetailsResponse 
                { 
                    HasApprovedOrDelegatedUserDetailsSentForApproval = true,
                }));

            this.FacadeServiceMock.Setup(mock => mock.GetUserAccount())
                .Returns(Task.FromResult(new UserAccountDto
                {

                }));

            this.SetupBase(userData);

            var viewModel = new Mock<EditUserDetailsViewModel>();

            // Act
            var result = await SystemUnderTest.DeclarationPost(viewModel.Object);

            //Assert
            Assert.IsInstanceOfType<RedirectToActionResult>(result);
            var resultAction = (RedirectToActionResult)result;
            Assert.AreSame("DetailsChangeRequested", resultAction.ActionName);
        }


    }
}