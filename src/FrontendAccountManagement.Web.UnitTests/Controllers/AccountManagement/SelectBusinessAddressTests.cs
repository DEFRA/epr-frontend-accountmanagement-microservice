using FluentAssertions.Execution;
using FrontendAccountManagement.Core.Addresses;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Controllers.AccountManagement;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement
{
    [TestClass]
    public class SelectBusinessAddressTests : AccountManagementTestBase
    {
        private static readonly IList<Address> _addressesMock = new List<Address>
        {
            new() { BuildingNumber = "10", Street = "Gracefield Gardens", Town = "London" },
            new() { BuildingNumber = "11", Street = "Gracefield Gardens", Town = "London" },
            new() { BuildingNumber = "12", Street = "Gracefield Gardens", Town = "London" }
        };


        [TestInitialize]
        public void Setup()
        {
            SetupBase();

            TempDataDictionaryMock = new Mock<ITempDataDictionary>();
            SystemUnderTest.TempData = TempDataDictionaryMock.Object;

            JourneySessionMock = new JourneySession
            {
                AccountManagementSession = new AccountManagementSession
                {
                    Journey = new List<string> { PagePath.BusinessAddressPostcode, PagePath.SelectBusinessAddress },
                    BusinessAddress = new Address { Postcode = "NW2 3TB" },
                    AddressesForPostcode = _addressesMock.ToList()
                }
            };

            SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);

            var addressListMock = new AddressList { Addresses = _addressesMock };
            FacadeServiceMock.Setup(x => x.GetAddressListByPostcodeAsync(It.IsAny<string>()))
                .ReturnsAsync(addressListMock);
        }

        [TestMethod]
        public async Task GivenFailedPostcodeLookup_WhenSelectBusinessAddressCalled_TheUserHasBeenRedirectedToBusinessAddressPage()
        {
            //Arrange
            FacadeServiceMock
                .Setup(x => x.GetAddressListByPostcodeAsync(It.IsAny<string>()))
                .Throws(new Exception());

            //Act
            var result = await SystemUnderTest.SelectBusinessAddress();

            //Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>();
                var redirection = (RedirectToActionResult)result;
                redirection.ActionName.Should().Be(nameof(AccountManagementController.BusinessAddress));
                TempDataDictionaryMock.Invocations.SelectMany(x => x.Arguments).Should().Contain(PostcodeLookupFailedKey);
            }
        }

        [TestMethod]
        public async Task GivenFailedPostcodeLookup_GetAddressList_ReturnsNull_WhenSelectBusinessAddressCalled_TheUserHasBeenRedirectedToBusinessAddressPage()
        {
            //Arrange
            FacadeServiceMock
                .Setup(x => x.GetAddressListByPostcodeAsync(It.IsAny<string>()))
                .Returns((Task<AddressList?>)null);

            //Act
            var result = await SystemUnderTest.SelectBusinessAddress();

            //Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>();
                var redirection = (RedirectToActionResult)result;
                redirection.ActionName.Should().Be(nameof(AccountManagementController.BusinessAddress));
                TempDataDictionaryMock.Invocations.SelectMany(x => x.Arguments).Should().Contain(PostcodeLookupFailedKey);
            }
        }

        [TestMethod]
        public async Task GivenFinishedPreviousPage_WhenSelectBusinessAddressCalled_ThenSelectBusinessAddressPageReturned_WithBusinessAddressPostcodeAsTheBackLink()
        {
            //Act
            var result = await SystemUnderTest.SelectBusinessAddress();

            //Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ViewResult>();
                var viewResult = (ViewResult)result;
                viewResult.Model.Should().BeOfType<SelectBusinessAddressViewModel>();
                AssertBackLink(viewResult, PagePath.BusinessAddressPostcode);
            }
        }

        [TestMethod]
        public async Task SelectBusinessAddress_WhenAddressIndexIsCorrect_ItShouldUpdateSessionAndRedirectToNextScreen()
        {
            //Arrange
            JourneySessionMock.AccountManagementSession!.AddressesForPostcode = new List<Address?> { new() };

            //Act
            var result = await SystemUnderTest.SelectBusinessAddress(
                new SelectBusinessAddressViewModel { SelectedListIndex = "0" });

            //Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>();
                ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountManagementController.NonCompaniesHouseUkNation));
                SessionManagerMock.Verify(sessionManager => sessionManager
                    .SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
            }
        }

        [TestMethod]
        public async Task SelectBusinessAddress_WhenAddressIndexIsOutOfBounds_ItShouldReturnViewWithError()
        {
            //Arrange
            JourneySessionMock.AccountManagementSession!.AddressesForPostcode = new List<Address?> { new() };

            //Act
            var result = await SystemUnderTest.SelectBusinessAddress(
                new SelectBusinessAddressViewModel { SelectedListIndex = "1" });

            //Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ViewResult>();
                ((ViewResult)result).ViewData.ModelState.IsValid.Should().BeFalse();
                SessionManagerMock.Verify(sessionManager => sessionManager
                    .SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Never);
            }
        }

        [TestMethod]
        public async Task SelectBusinessAddress_WhenAddressIndexIsNotNumber_ItShouldReturnViewWithError()
        {
            //Act
            var result = await SystemUnderTest.SelectBusinessAddress(
                new SelectBusinessAddressViewModel { SelectedListIndex = "a" });

            //Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ViewResult>();
                ((ViewResult)result).ViewData.ModelState.IsValid.Should().BeFalse();
                SessionManagerMock.Verify(sessionManager => sessionManager
                    .SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Never);
            }
        }

        [TestMethod]
        public async Task GivenNullPostcodeLookup_WhenSelectBusinessAddressCalled_TheUserHasBeenRedirectedToBusinessAddressPage()
        {
            //Arrange
            FacadeServiceMock
                .Setup(x => x.GetAddressListByPostcodeAsync(It.IsAny<string>()));

            //Act
            var result = await SystemUnderTest.SelectBusinessAddress();

            //Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>();
                var redirection = (RedirectToActionResult)result;
                redirection.ActionName.Should().Be(nameof(AccountManagementController.BusinessAddress));
                TempDataDictionaryMock.Invocations.SelectMany(x => x.Arguments).Should().Contain(PostcodeLookupFailedKey);
            }
        }

        [TestMethod]
        public async Task GivenEmptyAddressList_WhenSelectBusinessAddressCalled_ShouldRedirectToBusinessAddress()
        {
            // Arrange
            FacadeServiceMock.Setup(x => x.GetAddressListByPostcodeAsync(It.IsAny<string>()))
                .ReturnsAsync(new AddressList { Addresses = new List<Address>() });

            // Act
            var result = await SystemUnderTest.SelectBusinessAddress();

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>()
                .Which.ActionName.Should().Be(nameof(AccountManagementController.BusinessAddress));
                TempDataDictionaryMock.Invocations.SelectMany(x => x.Arguments).Should().Contain(PostcodeLookupFailedKey);
            }
        }

        [TestMethod]
        public async Task GivenValidSession_WhenSelectBusinessAddressCalled_ShouldReturnCorrectViewModel()
        {
            // Act
            var result = await SystemUnderTest.SelectBusinessAddress();

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ViewResult>()
                .Which.Model.Should().BeOfType<SelectBusinessAddressViewModel>();

                var viewResult = (ViewResult)result;
                AssertBackLink(viewResult, PagePath.BusinessAddressPostcode);
            }
        }

        [TestMethod]
        public async Task GivenValidSelection_WhenSelectBusinessAddressPostCalled_ShouldRedirectToUkNation()
        {
            // Act
            var result = await SystemUnderTest.SelectBusinessAddress(new SelectBusinessAddressViewModel { SelectedListIndex = "1" });

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>()
                .Which.ActionName.Should().Be(nameof(AccountManagementController.NonCompaniesHouseUkNation));
            }
        }

        [TestMethod]
        public async Task GivenInvalidSelectionIndex_WhenSelectBusinessAddressPostCalled_ShouldReturnViewWithError()
        {
            // Act
            var result = await SystemUnderTest.SelectBusinessAddress(new SelectBusinessAddressViewModel { SelectedListIndex = "10" });

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ViewResult>();
                ((ViewResult)result).ViewData.ModelState.IsValid.Should().BeFalse();
            }
        }

        [TestMethod]
        public async Task GivenInvalidIndexFormat_WhenSelectBusinessAddressPostCalled_ShouldReturnViewWithError()
        {
            // Act
            var result = await SystemUnderTest.SelectBusinessAddress(new SelectBusinessAddressViewModel { SelectedListIndex = "abc" });

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ViewResult>();
                ((ViewResult)result).ViewData.ModelState.IsValid.Should().BeFalse();
            }
        }

        [TestMethod]
        public async Task GivenErrorInFacadeService_WhenSelectBusinessAddressPostCalled_ShouldRedirectToBusinessAddress()
        {
            // Arrange
            FacadeServiceMock.Setup(x => x.GetAddressListByPostcodeAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Service failure"));

            SystemUnderTest.ModelState.AddModelError("SelectedListIndex", "Index is required");

            // Act
            var result = await SystemUnderTest.SelectBusinessAddress(new SelectBusinessAddressViewModel { SelectedListIndex = "0" });

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>()
                .Which.ActionName.Should().Be(nameof(AccountManagementController.BusinessAddress));
            }
        }

        [TestMethod]
        public async Task GivenValidInput_WhenSelectBusinessAddressPostCalled_ShouldUpdateSessionCorrectly()
        {
            // Act
            await SystemUnderTest.SelectBusinessAddress(new SelectBusinessAddressViewModel { SelectedListIndex = "1" });

            // Assert
            using (new AssertionScope())
            {
                Assert.AreEqual(_addressesMock[1], JourneySessionMock.AccountManagementSession.BusinessAddress);
                SessionManagerMock.Verify(session => session.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
            }
        }

        [TestMethod]
        public async Task GivenNullSession_WhenSelectBusinessAddressCalled_ShouldRedirectToBusinessAddress()
        {
            // Arrange
            SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync((JourneySession)null);

            // Act
            var result = await SystemUnderTest.SelectBusinessAddress();

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>()
                .Which.ActionName.Should().Be(nameof(AccountManagementController.BusinessAddress));
                TempDataDictionaryMock.Invocations.SelectMany(x => x.Arguments).Should().Contain(PostcodeLookupFailedKey);
            }
        }

        [TestMethod]
        public async Task GivenEmptyPostcode_WhenSelectBusinessAddressCalled_ShouldRedirectToBusinessAddress()
        {
            // Arrange
            JourneySessionMock.AccountManagementSession.BusinessAddress.Postcode = string.Empty;

            SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);

            // Act
            var result = await SystemUnderTest.SelectBusinessAddress();

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>()
                .Which.ActionName.Should().Be(nameof(AccountManagementController.BusinessAddress));

                TempDataDictionaryMock.Invocations.SelectMany(x => x.Arguments).Should().Contain(PostcodeLookupFailedKey);

                LoggerMock.Verify(
                    x => x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Postcode is missing in session, redirecting to BusinessAddress page.")),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
            }
        }

        [TestMethod]
        public async Task GivenNullSession_WhenSelectBusinessAddressPostCalled_ShouldRedirectToBusinessAddress()
        {
            // Arrange
            SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync((JourneySession)null); // Mock session as null

            var model = new SelectBusinessAddressViewModel
            {
                SelectedListIndex = "1"
            };

            // Act
            var result = await SystemUnderTest.SelectBusinessAddress(model);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>()
                .Which.ActionName.Should().Be(nameof(AccountManagementController.BusinessAddress));
                TempDataDictionaryMock.Invocations.SelectMany(x => x.Arguments).Should().Contain(PostcodeLookupFailedKey);
            }
        }


    }
}
