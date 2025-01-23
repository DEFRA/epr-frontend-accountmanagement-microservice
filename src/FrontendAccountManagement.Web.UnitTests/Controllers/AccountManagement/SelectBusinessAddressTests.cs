using FrontendAccountManagement.Core.Addresses;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Controllers.AccountManagement;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        private AccountManagementSession _accountManagementSessionMock = null!;
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

            JourneySessionMock = new JourneySession
            {
                AccountManagementSession = new AccountManagementSession
                {
                    Journey = new List<string> { PagePath.BusinessAddressPostcode, PagePath.SelectBusinessAddress, PagePath.BusinessAddress },
                    BusinessAddress = new Address { Postcode = "NW2 3TB" }
                }
            };

            SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync(JourneySessionMock);


            var addressListMock = new AddressList
            {
                Addresses = _addressesMock
            };
            FacadeServiceMock.Setup(x => x.GetAddressListByPostcodeAsync(It.IsAny<string>())).ReturnsAsync(addressListMock);
        }

        // PAUL - this test needs addressing after the full merge
        //[TestMethod]
        //public async Task GivenFailedPostcodeLookup_WhenSelectBusinessAddressCalled_TheUserHasBeenRedirectedToBusinessAddressPage()
        //{
        //    //Arrange
        //    FacadeServiceMock
        //        .Setup(x => x.GetAddressListByPostcodeAsync(It.IsAny<string>()))
        //        .Throws(new Exception());

        //    //Act
        //    var result = await SystemUnderTest.SelectBusinessAddress();

        //    //Assert
        //    result.Should().BeOfType<RedirectToActionResult>();

        //    var redirection = (RedirectToActionResult)result;

        //    redirection.ActionName.Should().Be(nameof(AccountManagementController.BusinessAddress));

        //    TempDataDictionaryMock.Invocations.SelectMany(x => x.Arguments).Should().Contain(PostcodeLookupFailedKey);
        //}

        // PAUL - this test needs addressing after the full merge
        //[TestMethod]
        //public async Task GivenFailedPostcodeLookup_GetAddressList_ReturnsNull_WhenSelectBusinessAddressCalled_TheUserHasBeenRedirectedToBusinessAddressPage()
        //{
        //    //Arrange
        //    FacadeServiceMock
        //        .Setup(x => x.GetAddressListByPostcodeAsync(It.IsAny<string>()))
        //        .Returns((Task<AddressList?>)null);

        //    //Act
        //    var result = await SystemUnderTest.SelectBusinessAddress();

        //    //Assert
        //    result.Should().BeOfType<RedirectToActionResult>();

        //    var redirection = (RedirectToActionResult)result;

        //    redirection.ActionName.Should().Be(nameof(AccountManagementController.BusinessAddress));

        //    TempDataDictionaryMock.Invocations.SelectMany(x => x.Arguments).Should().Contain(PostcodeLookupFailedKey);
        //}

        [TestMethod]
        public async Task GivenFinishedPreviousPage_WhenSelectBusinessAddressCalled_ThenSelectBusinessAddressPageReturned_WithBusinessAddressPostcodeAsTheBackLink()
        {
            //Act
            var result = await SystemUnderTest.SelectBusinessAddress();

            //Assert
            result.Should().BeOfType<ViewResult>();
            var viewResult = (ViewResult)result;
            viewResult.Model.Should().BeOfType<SelectBusinessAddressViewModel>();
            AssertBackLink(viewResult, PagePath.BusinessAddressPostcode);
        }

        [TestMethod]
        public async Task SelectBusinessAddress_WhenAddressIndexIsCorrect_ItShouldUpdateSessionAndRedirectToNextScreen()
        {
            JourneySessionMock.AccountManagementSession!.AddressesForPostcode = new List<Address?> { new() };

            var result = await SystemUnderTest.SelectBusinessAddress(
                new SelectBusinessAddressViewModel { SelectedListIndex = "0" });

            result.Should().BeOfType<RedirectToActionResult>();

            ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountManagementController.UkNation));

            SessionManagerMock.Verify(sessionManager => sessionManager
                .SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
        }

        [TestMethod]
        public async Task SelectBusinessAddress_WhenAddressIndexIsOutOfBounds_ItShouldReturnViewWithError()
        {
            JourneySessionMock.AccountManagementSession!.AddressesForPostcode = new List<Address?> { new() };

            var result = await SystemUnderTest.SelectBusinessAddress(
                new SelectBusinessAddressViewModel { SelectedListIndex = "1" });

            result.Should().BeOfType<ViewResult>();

            ((ViewResult)result).ViewData.ModelState.IsValid.Should().BeFalse();

            SessionManagerMock.Verify(sessionManager => sessionManager
                .SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Never);
        }

        [TestMethod]
        public async Task SelectBusinessAddress_WhenAddressIndexIsNotNumber_ItShouldReturnViewWithError()
        {
            var result = await SystemUnderTest.SelectBusinessAddress(
                new SelectBusinessAddressViewModel { SelectedListIndex = "a" });

            result.Should().BeOfType<ViewResult>();

            ((ViewResult)result).ViewData.ModelState.IsValid.Should().BeFalse();

            SessionManagerMock.Verify(sessionManager => sessionManager
                .SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Never);
        }

        // PAUL - this test needs addressing after the full merge
        //[TestMethod]
        //public async Task GivenNullPostcodeLookup_WhenSelectBusinessAddressCalled_TheUserHasBeenRedirectedToBusinessAddressPage()
        //{
        //    //Arrange
        //    FacadeServiceMock
        //        .Setup(x => x.GetAddressListByPostcodeAsync(It.IsAny<string>()));

        //    //Act
        //    var result = await SystemUnderTest.SelectBusinessAddress();

        //    //Assert
        //    result.Should().BeOfType<RedirectToActionResult>();

        //    var redirection = (RedirectToActionResult)result;

        //    redirection.ActionName.Should().Be(nameof(AccountManagementController.BusinessAddress));

        //    TempDataDictionaryMock.Invocations.SelectMany(x => x.Arguments).Should().Contain(PostcodeLookupFailedKey);
        //}
    }
}
