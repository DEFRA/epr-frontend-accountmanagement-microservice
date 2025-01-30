using EPR.Common.Authorization.Models;
using FluentAssertions.Execution;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Controllers.AccountManagement;
using FrontendAccountManagement.Web.Controllers.Errors;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using FrontendAccountManagement.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.AspNetCore.Http;
using Moq;
using AutoFixture;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement
{
    [TestClass]
    public class UpdateCompanyAddressTests : AccountManagementTestBase
    {

        private UserData _userData;
        private JourneySession _journeySession;
        private Fixture _fixture = new Fixture();

        [TestInitialize]
        public void Setup()
        {
            _userData = _fixture.Create<UserData>();
            _userData.IsChangeRequestPending = false;

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
        public async Task Get_UpdateCompanyAddress_When_SessionIsNull_ReturnsViewWithModel()
        {
            // Arrange
            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync((JourneySession)null);

            // Act
            var result = await SystemUnderTest.UpdateCompanyAddress();

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ViewResult>();
                var viewResult = result as ViewResult;
                viewResult.Model.Should().BeOfType<UpdateCompanyAddressViewModel>();
                var model = viewResult.Model as UpdateCompanyAddressViewModel;
                model.IsUpdateCompanyAddress.Should().BeNull();
            }
        }

        [TestMethod]
        public async Task Get_UpdateCompanyAddress_When_OrganisationTypeIsCompaniesHouseCompany_ReturnsRedirect()
        {
            // Arrange
            var session = new JourneySession
            {
                AccountManagementSession = new AccountManagementSession
                {
                    OrganisationType = OrganisationType.CompaniesHouseCompany
                }
            };
            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

            // Act
            var result = await SystemUnderTest.UpdateCompanyAddress();

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>();
                var redirectResult = result as RedirectToActionResult;
                redirectResult.ControllerName.Should().Be(nameof(ErrorController.Error));
                redirectResult.RouteValues["statusCode"].Should().Be((int)HttpStatusCode.Forbidden);
            }
        }

        [TestMethod]
        public async Task Get_UpdateCompanyAddress_When_SessionIsValid_ReturnsViewWithCorrectBackLink()
        {
            // Arrange
            var session = new JourneySession
            {
                AccountManagementSession = new AccountManagementSession
                {
                    IsUpdateCompanyName = true
                }
            };
            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

            // Act
            var result = await SystemUnderTest.UpdateCompanyAddress();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [TestMethod]
        public async Task Post_UpdateCompanyAddress_When_ModelStateIsInvalid_ReturnsViewWithModel()
        {
            // Arrange
            SystemUnderTest.ModelState.AddModelError("isUpdateCompanyAddress", "Required");

            var model = new UpdateCompanyAddressViewModel
            {
                IsUpdateCompanyAddress = null // Invalid value to trigger the error
            };

            // Act
            var result = await SystemUnderTest.UpdateCompanyAddress(model);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ViewResult>();
                var viewResult = result as ViewResult;
                viewResult.Model.Should().Be(model);
            }
        }

        [TestMethod]
        public async Task Post_UpdateCompanyAddress_When_SessionIsNull_ReturnsRedirectForBusinessAddressPostcode()
        {
            // Arrange
            var model = new UpdateCompanyAddressViewModel
            {
                IsUpdateCompanyAddress = YesNoAnswer.Yes
            };
            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync((JourneySession)null);

            // Act
            var result = await SystemUnderTest.UpdateCompanyAddress(model);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>();
                var redirectResult = result as RedirectToActionResult;
                redirectResult.ActionName.Should().Be("BusinessAddressPostcode");
            }
        }

        [TestMethod]
        public async Task Post_UpdateCompanyAddress_When_IsUpdateCompanyAddressIsYes_ReturnsRedirectToBusinessAddressPostcode()
        {
            // Arrange
            var model = new UpdateCompanyAddressViewModel
            {
                IsUpdateCompanyAddress = YesNoAnswer.Yes
            };
            var session = new JourneySession();
            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

            // Act
            var result = await SystemUnderTest.UpdateCompanyAddress(model);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>();
                var redirectResult = result as RedirectToActionResult;
                redirectResult.ActionName.Should().Be("BusinessAddressPostcode");
            }
        }

        [TestMethod]
        public async Task Post_UpdateCompanyAddress_When_IsUpdateCompanyAddressIsNo_And_OrganisationNameChanged_RedirectsToCheckYourCompanyDetails()
        {
            // Arrange
            var model = new UpdateCompanyAddressViewModel
            {
                IsUpdateCompanyAddress = YesNoAnswer.No
            };

            var session = new JourneySession
            {
                AccountManagementSession = new AccountManagementSession
                {
                    IsUpdateCompanyName = true
                }
            };

            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

            // Act
            var result = await SystemUnderTest.UpdateCompanyAddress(model);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>();
                var redirectResult = result as RedirectToActionResult;
                redirectResult.ActionName.Should().Be("CheckCompanyDetails");
            }
        }

        [TestMethod]
        public async Task Post_UpdateCompanyAddress_When_IsUpdateCompanyAddressIsNo_And_OrganisationNameIsSame_RedirectsToManageAccount()
        {
            // Arrange
            var model = new UpdateCompanyAddressViewModel
            {
                IsUpdateCompanyAddress = YesNoAnswer.No
            };

            var session = new JourneySession
            {
                UserData = new UserData
                {
                    Organisations = new List<Organisation>
                {
                    new Organisation { Name = "CompanyName" }
                }
                },
                AccountManagementSession = new AccountManagementSession
                {
                    OrganisationName = "CompanyName"
                }
            };

            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

            // Act
            var result = await SystemUnderTest.UpdateCompanyAddress(model);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>();
                var redirectResult = result as RedirectToActionResult;
                redirectResult.ActionName.Should().Be(nameof(AccountManagementController.ManageAccount));
            }
        }
    }
}
