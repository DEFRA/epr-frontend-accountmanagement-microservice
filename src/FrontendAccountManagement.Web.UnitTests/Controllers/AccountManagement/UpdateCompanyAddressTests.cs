using System.Net;
using AutoFixture;
using EPR.Common.Authorization.Models;
using FluentAssertions.Execution;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Controllers.AccountManagement;
using FrontendAccountManagement.Web.Controllers.Errors;
using FrontendAccountManagement.Web.ViewModels;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

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
        public async Task Get_UpdateCompanyAddress_ShouldRedirectIfOrganisationIsCompaniesHouseCompany()
        {
            // Arrange
            var session = new JourneySession
            {
                AccountManagementSession = new AccountManagementSession
                {
                    OrganisationType = OrganisationType.CompaniesHouseCompany
                }
            };
            SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

            // Act
            var result = await SystemUnderTest.UpdateCompanyAddress();

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>();
                var redirectResult = result as RedirectToActionResult;
                redirectResult.ControllerName.Should().Be("Error");
                redirectResult.RouteValues["statusCode"].Should().Be((int)HttpStatusCode.Forbidden);
            }
        }

        [TestMethod]
        public async Task Get_UpdateCompanyAddress_ShouldReturnViewWithModelWhenSessionIsNotNullAndIsUpdateCompanyAddressTrue()
        {
            // Arrange
            var session = new JourneySession
            {
                AccountManagementSession = new AccountManagementSession
                {
                    OrganisationType = OrganisationType.NonCompaniesHouseCompany,
                    IsUpdateCompanyAddress = true
                }
            };
            SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

            // Act
            var result = await SystemUnderTest.UpdateCompanyAddress();

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ViewResult>();
                var viewResult = result as ViewResult;
                var model = viewResult.Model as UpdateCompanyAddressViewModel;

                model.Should().NotBeNull();
                model.IsUpdateCompanyAddress.Should().Be(YesNoAnswer.Yes);
            }
        }

        [TestMethod]
        public async Task Get_UpdateCompanyAddress_ShouldReturnViewWithModelWhenSessionIsNotNullAndIsUpdateCompanyAddressFalse()
        {
            // Arrange
            var session = new JourneySession
            {
                AccountManagementSession = new AccountManagementSession
                {
                    OrganisationType = OrganisationType.NonCompaniesHouseCompany,
                    IsUpdateCompanyAddress = false,

                    Journey = new List<string> {
                        PagePath.UpdateCompanyName, PagePath.UpdateCompanyAddress
                    },
                }
            };
            SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

            // Act
            var result = await SystemUnderTest.UpdateCompanyAddress();

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ViewResult>();
                var viewResult = result as ViewResult;
                var model = viewResult.Model as UpdateCompanyAddressViewModel;

                model.Should().NotBeNull();
                model.IsUpdateCompanyAddress.Should().Be(YesNoAnswer.No);

                AssertBackLink(viewResult, PagePath.UpdateCompanyName);
            }
        }

        [TestMethod]
        public async Task Get_UpdateCompanyAddress_ShouldReturnViewWithModelWhenIsUpdateCompanyAddressIsNull()
        {
            // Arrange
            var session = new JourneySession
            {
                AccountManagementSession = new AccountManagementSession
                {
                    OrganisationType = OrganisationType.NonCompaniesHouseCompany,
                    IsUpdateCompanyAddress = null
                }
            };
            SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

            // Act
            var result = await SystemUnderTest.UpdateCompanyAddress();

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ViewResult>();
                var viewResult = result as ViewResult;
                var model = viewResult.Model as UpdateCompanyAddressViewModel;

                model.Should().NotBeNull();
                model.IsUpdateCompanyAddress.Should().BeNull();
            }
        }

        [TestMethod]
        public async Task Post_UpdateCompanyAddress_When_ModelStateIsInvalid_ReturnsViewWithModel()
        {
            // Arrange
            var session = new JourneySession
            {
                AccountManagementSession = new AccountManagementSession
                {
                    OrganisationType = OrganisationType.NonCompaniesHouseCompany,
                    IsUpdateCompanyAddress = false,

                    Journey = new List<string> {
                        PagePath.UpdateCompanyName, PagePath.UpdateCompanyAddress
                    },
                }
            };
            SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

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
                AssertBackLink(viewResult, PagePath.UpdateCompanyName);
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
                redirectResult.ActionName.Should().Be(nameof(SystemUnderTest.BusinessAddressPostcode));
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
                redirectResult.ActionName.Should().Be(nameof(SystemUnderTest.BusinessAddressPostcode));
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
                redirectResult.ActionName.Should().Be(nameof(SystemUnderTest.CheckCompanyDetails));
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

        [TestMethod]
        public async Task GivenFeatureIsDisabled_WhenUpdateCompanyAddressCalled_ThenReturnsToErrorPage_WithNotFoundStatusCode()
        {
            // Arrange
            FeatureManagerMock.Setup(x => x.IsEnabledAsync(FeatureName.ManageCompanyDetailChanges))
            .ReturnsAsync(false);

            // Act
            var result = await SystemUnderTest.UpdateCompanyAddress();

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>();

                var actionResult = result as RedirectToActionResult;
                actionResult.Should().NotBeNull();
                actionResult.ActionName.Should().Be(PagePath.Error);
                actionResult.ControllerName.Should().Be(nameof(ErrorController.Error));
                actionResult.RouteValues["statusCode"].Should().Be((int)HttpStatusCode.NotFound);
            }
        }
    }
}
