using AutoFixture;
using EPR.Common.Authorization.Models;
using FluentAssertions.Execution;
using FrontendAccountManagement.Core.Addresses;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Controllers.Errors;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Net;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement
{
    [TestClass]
    public class BusinessAddressPostcodeTests : AccountManagementTestBase
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

            TempDataDictionaryMock = new Mock<ITempDataDictionary>();
            SystemUnderTest.TempData = TempDataDictionaryMock.Object;

            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
                .Returns(Task.FromResult(_journeySession));
        }

        [TestMethod]
        public async Task Get_BusinessAddressPostcode_When_SessionIsNull_ReturnsViewWithModel()
        {
            // Arrange
            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync((JourneySession)null);

            // Act
            var result = await SystemUnderTest.BusinessAddressPostcode();

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ViewResult>();
                var viewResult = result as ViewResult;
                viewResult.Model.Should().BeOfType<BusinessAddressPostcodeViewModel>();
            }
        }

        [TestMethod]
        public async Task Get_BusinessAddressPostcode_When_OrganisationTypeIsCompaniesHouseCompany_ReturnsRedirect()
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
            var result = await SystemUnderTest.BusinessAddressPostcode();

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>();
                var redirectResult = result as RedirectToActionResult;
                redirectResult.ControllerName.Should().Be(nameof(ErrorController.Error));
                redirectResult.RouteValues["statusCode"].Should().Be((int)HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task Get_BusinessAddressPostcode_When_SessionIsValid_ReturnsViewWithCorrectBackLink()
        {
            // Arrange
            var session = new JourneySession
            {
                AccountManagementSession = new AccountManagementSession
                {
                    BusinessAddress = new Address { Postcode = "AB12 3CD" }
                }
            };
            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

            // Act
            var result = await SystemUnderTest.BusinessAddressPostcode();

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ViewResult>();
                var viewResult = result as ViewResult;
                var model = viewResult.Model.Should().BeOfType<BusinessAddressPostcodeViewModel>().Which;
                model.Postcode.Should().Be("AB12 3CD");
            }
        }

        [TestMethod]
        public async Task Post_BusinessAddressPostcode_When_ModelStateIsInvalid_ReturnsViewWithModel()
        {
            // Arrange
            SystemUnderTest.ModelState.AddModelError("Postcode", "Postcode is required");

            var model = new BusinessAddressPostcodeViewModel
            {
                Postcode = null // Invalid value to trigger the error
            };

            // Act
            var result = await SystemUnderTest.BusinessAddressPostcode(model);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ViewResult>();
                var viewResult = result as ViewResult;
                viewResult.Model.Should().Be(model);
            }
        }

        [TestMethod]
        public async Task Post_BusinessAddressPostcode_When_PostcodeIsValid_ReturnsRedirectToSelectBusinessAddress()
        {
            // Arrange
            var model = new BusinessAddressPostcodeViewModel
            {
                Postcode = "AB12 3CD"
            };
            var session = new JourneySession
            {
                AccountManagementSession = new AccountManagementSession
                {
                    BusinessAddress = new Address { Postcode = "AB12 3CD" }
                }
            };
            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

            var addressList = new AddressList(); // Simulating address found
            FacadeServiceMock.Setup(f => f.GetAddressListByPostcodeAsync(It.IsAny<string>())).ReturnsAsync(addressList);

            // Act
            var result = await SystemUnderTest.BusinessAddressPostcode(model);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>();
                var redirectResult = result as RedirectToActionResult;
                redirectResult.ActionName.Should().Be("SelectBusinessAddress");
            }
        }

        [TestMethod]
        public async Task Post_BusinessAddressPostcode_When_AddressListIsNull_ReturnsRedirectForBusinessAddress()
        {
            // Arrange
            var model = new BusinessAddressPostcodeViewModel
            {
                Postcode = "AB12 3CD"
            };
            var session = new JourneySession
            {
                AccountManagementSession = new AccountManagementSession
                {
                    BusinessAddress = new Address { Postcode = "AB12 3CD" }
                }
            };
            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

            FacadeServiceMock.Setup(f => f.GetAddressListByPostcodeAsync(It.IsAny<string>())).ReturnsAsync((AddressList)null);

            // Act
            var result = await SystemUnderTest.BusinessAddressPostcode(model);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>();
                var redirectResult = result as RedirectToActionResult;
                redirectResult.ActionName.Should().Be("BusinessAddress");

                TempDataDictionaryMock.Invocations.SelectMany(x => x.Arguments).Should().Contain(PostcodeLookupFailedKey);
            }
        }

        [TestMethod]
        public async Task GivenFeatureIsDisabled_WhenBusinessAddressPostcodeCalled_ThenReturnsToErrorPage_WithNotFoundStatusCode()
        {
            // Arrange
            FeatureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.ManageCompanyDetailChanges))
            .ReturnsAsync(false);

            // Act
            var result = await SystemUnderTest.BusinessAddressPostcode();

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

        [TestMethod]
        public async Task GivenFeatureIsDisabled_WhenBusinessAddressPostcodeSubmitted_ThenReturnsToErrorPage_WithNotFoundStatusCode()
        {
            // Arrange
            FeatureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.ManageCompanyDetailChanges))
            .ReturnsAsync(false);

            var model = new BusinessAddressPostcodeViewModel
            {
                Postcode = "AB12 3CD"
            };

            // Act
            var result = await SystemUnderTest.BusinessAddressPostcode(model);

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
