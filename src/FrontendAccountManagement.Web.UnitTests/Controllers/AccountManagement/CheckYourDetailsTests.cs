using AutoFixture;
using EPR.Common.Authorization.Constants;
using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Core.Enums;
using FrontendAccountManagement.Core.Models;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Controllers.AccountManagement;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using static FrontendAccountManagement.Core.Constants.ServiceRoles;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement
{
    /// <summary>
    /// Test the CheckYourDetails functionality
    /// </summary>
    [TestClass]
    public class CheckYourDetailsTests : AccountManagementTestBase
    {
        private UserData _userData;
        private JourneySession _journeySession;
        private Fixture _fixture = new Fixture();
        private EditUserDetailsViewModel _viewModel;
        private UpdateUserDetailsRequest _userDetailsDto;
        private UpdateUserDetailsResponse _updateUserDetailsResponse;

        [TestInitialize]
        public void Setup()
        {
            _userData = _fixture.Create<UserData>();
            _userData.IsChangeRequestPending = false;

            _viewModel = _fixture.Create<EditUserDetailsViewModel>();
            _userDetailsDto = _fixture.Create<UpdateUserDetailsRequest>();
            _updateUserDetailsResponse = _fixture.Create<UpdateUserDetailsResponse>();

            SetupBase(_userData);

            _journeySession = new JourneySession
            {
                UserData = _userData,
                AccountManagementSession = new AccountManagementSession() { Journey = new List<string>() }
            };

            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
                .Returns(Task.FromResult(_journeySession));
        }

        [TestMethod]
        public async Task CheckYourDetails_Logs_Error_WhenNewUserDetailsViewModelJsonInvalid()
        {
            // Arrange
            var userData = new UserData
            {
                ServiceRole = Core.Enums.ServiceRole.Approved.ToString(),
                ServiceRoleId = 1,
                RoleInOrganisation = PersonRole.Admin.ToString(),
            };
            SetupBase(userData);
            SystemUnderTest.TempData.Add("NewUserDetails", "{ invalid json }");
            // Act
            var result = await SystemUnderTest.CheckYourDetails();
            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            var viewModelResult = (EditUserDetailsViewModel)viewResult.Model;
            Assert.IsNull(viewModelResult.FirstName);
            Assert.IsNull(viewModelResult.LastName);
            LoggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deserialising NewUserDetails Failed.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
            // Verify the session was saved and the back link was set
            SessionManagerMock.Verify(
                m =>
                    m.SaveSessionAsync(
                        It.IsAny<ISession>(),
                        It.IsAny<JourneySession>())
                    , Times.Once);
        }

        [TestMethod]
        public async Task CheckYourDetails_ShouldReturnViewWithExpectedModel()
        {
            // Act
            var result = await SystemUnderTest.CheckYourDetails();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            var model = viewResult.Model as EditUserDetailsViewModel;
            model.Should().NotBeNull();
            model.FirstName.Should().StartWith("FirstName");
            model.LastName.Should().StartWith("LastName");
        }

        [TestMethod]
        public async Task CheckYourDetails_Throw_403_Error_When_IsChangeRequestPending_IsTrue()
        {
            //Arrange
            _userData.IsChangeRequestPending = true;
            SetupBase(_userData);

            // Act
            var result = await SystemUnderTest.CheckYourDetails();

            // Assert
            ((RedirectToActionResult)result).ActionName.Should().Be("error");
            ((object[])((RedirectToActionResult)result).RouteValues.Values)[0].Should().Be(403);
        }

        [TestMethod]
        public async Task CheckYourDetailsPost_ShouldReturnViewData()
        {
            // Act
            var result = await SystemUnderTest.CheckYourDetails(_viewModel);

            // Assert
            result.Should().NotBeNull();
            ((ViewResult)result).ViewData.Should().NotBeNull();
        }

        [TestMethod]
        public async Task CheckYourDetailsPost_ShouldRedirectToAction_Declaration_For_Approved_Or_Delegate_User()
        {
            //Arrange
            _userData.ServiceRoleId = 1;
            _userData.RoleInOrganisation = "Admin";
            _userData.ServiceRole = "Approved";
            SetupBase(_userData);
            // Act        
            var result = await SystemUnderTest.CheckYourDetails(_viewModel) as RedirectToActionResult;
            // Assert
            result.Should().NotBeNull();
            result.ActionName.Should().Be("Declaration");
        }

        [TestMethod]
        public async Task CheckYourDetailsPost_Call_UpdateUserDetails_When_Condition_Met()
        {
            //Arrange
            _userData.ServiceRoleId = 3;
            _userData.RoleInOrganisation = "Admin";
            _userData.ServiceRole = "Basic";
            SetupBase(_userData);

            AutoMapperMock.Setup(x => x.Map<UpdateUserDetailsRequest>(_viewModel)).Returns(_userDetailsDto);

            FacadeServiceMock.Setup(x => x.GetUserAccount()).ReturnsAsync(new UserAccountDto());

            FacadeServiceMock.Setup(x => x.UpdateUserDetailsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), "Packaging", _userDetailsDto))
                 .ReturnsAsync(_updateUserDetailsResponse);

            // Act        
            var result = await SystemUnderTest.CheckYourDetails(_viewModel) as RedirectToActionResult;

            // Assert
            result.Should().NotBeNull();
            result.ActionName.Should().Be("UpdateDetailsConfirmation");
        }

        [TestMethod]
        public async Task CheckYourDetailsPost_Call_UpdateUserDetails_Update_Telephone_Only_When_Condition_Met()
        {
            //Arrange
            var editUserDetailsViewModel = new EditUserDetailsViewModel
            {
                FirstName = "TestFirst",
                LastName = "TestLast",
                JobTitle = "TestJob",
                Telephone = "07545812345",
                OriginalFirstName = "TestFirst",
                OriginalLastName = "TestLast",
                OriginalJobTitle = "TestJob",
                OriginalTelephone = "07545812346"
            };

            AutoMapperMock.Setup(x => x.Map<UpdateUserDetailsRequest>(editUserDetailsViewModel)).Returns(_userDetailsDto);
            FacadeServiceMock.Setup(x => x.GetUserAccount()).ReturnsAsync(new UserAccountDto());

            FacadeServiceMock.Setup(x => x.UpdateUserDetailsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), "Packaging", _userDetailsDto))
                 .ReturnsAsync(new UpdateUserDetailsResponse { HasTelephoneOnlyUpdated = true });

            // Act        
            var result = await SystemUnderTest.CheckYourDetails(editUserDetailsViewModel) as RedirectToActionResult;

            // Assert
            result.Should().NotBeNull();
            result.ActionName.Should().Be("UpdateDetailsConfirmation");
        }

        [TestMethod]
        public async Task CheckYourDetails_AddsAmendedUserDetailsToTempData_WhenNotPresent()
        {
            // Arrange
            var userData = new UserData
            {
                FirstName = "Test",
                LastName = "User",
                ServiceRoleId = (int)Core.Enums.ServiceRole.Approved,
                RoleInOrganisation = PersonRole.Admin.ToString(),
                Organisations = new List<Organisation> { new Organisation() }
            };
            SetupBase(userData);

            SystemUnderTest.TempData.Remove(AccountManagementController.AmendedUserDetailsKey); // Ensure it's not present

            // Act
            await SystemUnderTest.CheckYourDetails();

            // Assert
            Assert.IsTrue(SystemUnderTest.TempData.ContainsKey(AccountManagementController.AmendedUserDetailsKey));
        }

        [TestMethod]
        public async Task CheckYourDetails_DoesNotAddAmendedUserDetailsToTempData_WhenAlreadyPresent()
        {
            // Arrange
            var userData = new UserData
            {
                FirstName = "Test",
                LastName = "User",
                ServiceRoleId = (int)Core.Enums.ServiceRole.Approved,
                RoleInOrganisation = PersonRole.Admin.ToString(),
                Organisations = new List<Organisation> { new Organisation() }
            };
            SetupBase(userData);

            var expectedValue = "existing";
            SystemUnderTest.TempData[AccountManagementController.AmendedUserDetailsKey] = expectedValue;

            // Act
            await SystemUnderTest.CheckYourDetails();

            // Assert
            Assert.AreEqual(expectedValue, SystemUnderTest.TempData[AccountManagementController.AmendedUserDetailsKey]);
        }

        #region Private

        private static IEnumerable<object[]> CheckYourDetailsData
        {
            get
            {
                return new[]
                {
                    // Check journey is added to correctly when navigating from the management page.
                    new[]
                    {
                        new List<string> { "manage" },
                        new List<string> { "manage", "check-your-details" },
                    },

                    // Check the journey is added to correctly when navigating from the "what-are-your-details" page.
                    new[]
                    {
                        new List<string> { "manage", "check-your-details", "what-are-your-details" },
                        new List<string> { "manage", "check-your-details", "what-are-your-details", "check-your-details" },
                    },

                    // Check the journey isn't changed when the user refreshes the page.
                    new[]
                    {
                        new List<string> { "manage", "check-your-details" },
                        new List<string> { "manage", "check-your-details" },
                    }
                };
            }
        }

        #endregion
    }
}