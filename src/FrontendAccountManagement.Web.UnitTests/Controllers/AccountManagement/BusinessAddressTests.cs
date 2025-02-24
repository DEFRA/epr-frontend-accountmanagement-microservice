using FluentAssertions;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Controllers.AccountManagement;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using EPR.Common.Authorization.Models;
using AutoFixture;
using FrontendAccountManagement.Web.Controllers.Errors;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using FluentAssertions.Execution;
using System.Net;
using FrontendAccountManagement.Web.Configs;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement;

[TestClass]
public class BusinessAddressTests : AccountManagementTestBase
{
    private UserData _userData;
    private AccountManagementSession _session = null!;
    private JourneySession _journeySession;
    private Fixture _fixture = new Fixture();

    [TestInitialize]
    public void Setup()
    {
        _userData = _fixture.Create<UserData>();
        _userData.Organisations[0].CompaniesHouseNumber = null;

        SetupBase(_userData);

        _journeySession = new JourneySession
        {
            UserData = _userData,
            AccountManagementSession = new AccountManagementSession()
            {
                Journey = new List<string> {
                PagePath.BusinessAddressPostcode, PagePath.SelectBusinessAddress,PagePath.BusinessAddress
            },
                BusinessAddress = new FrontendAccountManagement.Core.Addresses.Address()
            }            
        };

        SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_journeySession);
    }

    [TestMethod]
    public async Task GivenValidBusinessAddress_WhenBusinessAddressCalled_ThenRedirectToUkNationPage_AndUpdateSession()
    {
        // Arrange
        var request = new BusinessAddressViewModel { BuildingNumber = "10", SubBuildingName = "Dummy House", Postcode = "AB01 BB3", Town = "Nowhere" };

        // Act
        var result = await SystemUnderTest.BusinessAddress(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(SystemUnderTest.NonCompaniesHouseUkNation));

        SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
    }

    [TestMethod]
    public async Task GivenIncompleteBusinessAddress_WhenBusinessAddressCalled_ThenReturnViewWithError()
    {
        // Arrange
        _journeySession.AccountManagementSession.Journey.RemoveAll(x => x == PagePath.SelectBusinessAddress);

        SystemUnderTest.ModelState.AddModelError(nameof(BusinessAddressViewModel.BuildingNumber), "Field is required");

        var request = new BusinessAddressViewModel { Postcode = "AB01 BB3", Town = "Nowhere" };

        // Act
        var result = await SystemUnderTest.BusinessAddress(request);

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<BusinessAddressViewModel>();

        SessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<JourneySession>>()), Times.Never);

        AssertBackLink(viewResult, PagePath.BusinessAddressPostcode);
    }

    [TestMethod]
    public async Task GivenIncompleteBusinessAddressWithTempData_WhenBusinessAddressCalled_ThenReturnViewWithError()
    {
        // Arrange
        _journeySession.AccountManagementSession.Journey.RemoveAll(x => x == PagePath.SelectBusinessAddress);

        TempDataDictionaryMock = new Mock<ITempDataDictionary>();
        TempDataDictionaryMock.Setup(x => x.ContainsKey(It.IsAny<string>())).Returns(true);
        
        SystemUnderTest.ModelState.AddModelError(nameof(BusinessAddressViewModel.BuildingNumber), "Field is required");
        SystemUnderTest.TempData = TempDataDictionaryMock.Object;
        var request = new BusinessAddressViewModel { Postcode = "AB01 BB3", Town = "Nowhere" };

        // Act
        var result = await SystemUnderTest.BusinessAddress(request);

        // Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<BusinessAddressViewModel>();

        var viewModel = (BusinessAddressViewModel)viewResult.Model!;

        viewModel.ShowWarning.Should().BeTrue();

        SessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<JourneySession>>()), Times.Never);

        AssertBackLink(viewResult, PagePath.BusinessAddressPostcode);
    }

    [TestMethod]
    public async Task GivenFinishedPreviousPage_WhenBusinessAddressCalled_ThenBusinessAddressPageReturned_WithSelectBusinessAddresseAsTheBackLink()
    {
        //Arrange
        TempDataDictionaryMock = new Mock<ITempDataDictionary>();
        TempDataDictionaryMock.Setup(tempData => tempData.ContainsKey("PostcodeLookupFailed")).Returns(true);
        TempDataDictionaryMock.SetupSet(tempData => tempData["PostcodeLookupFailed"] = true);
        SystemUnderTest.TempData = TempDataDictionaryMock.Object;

        //Act
        var result = await SystemUnderTest.BusinessAddress();

        //Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<BusinessAddressViewModel>();

        ((BusinessAddressViewModel)viewResult.Model).ShowWarning.Should().BeTrue();
        TempDataDictionaryMock.VerifySet(tempData => tempData["PostcodeLookupFailed"] = true, Times.Once);

        AssertBackLink(viewResult, PagePath.BusinessAddressPostcode);

        SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
    }

    [TestMethod]
    public async Task GivenNoPreviousPage_WhenBusinessAddressCalled_ThenBusinessAddressPageReturned_WithSelectBusinessAddresseAsTheBackLink()
    {
        //Arrange
        TempDataDictionaryMock = new Mock<ITempDataDictionary>();
        TempDataDictionaryMock.Setup(tempData => tempData.ContainsKey("PostcodeLookupFailed")).Returns(true);
        TempDataDictionaryMock.SetupSet(tempData => tempData["PostcodeLookupFailed"] = true);
        SystemUnderTest.TempData = TempDataDictionaryMock.Object;

        _journeySession.AccountManagementSession.Journey = new List<string>();

        //Act
        var result = await SystemUnderTest.BusinessAddress();

        //Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<BusinessAddressViewModel>();

        ((BusinessAddressViewModel)viewResult.Model).ShowWarning.Should().BeTrue();
        TempDataDictionaryMock.VerifySet(tempData => tempData["PostcodeLookupFailed"] = true, Times.Once);

        AssertBackLink(viewResult, PagePath.BusinessAddressPostcode);

        SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
    }


    [TestMethod]
    public async Task GivenBusinessAddress_WhenBusinessAddressIsManualAddress_ThenUpdatesModelWIthCorrectValue()
    {
        //Arrange
        _journeySession.AccountManagementSession.BusinessAddress.IsManualAddress = true;
        _journeySession.AccountManagementSession.BusinessAddress.SubBuildingName = "sub building name";
        _journeySession.AccountManagementSession.BusinessAddress.BuildingName = "building name";
        _journeySession.AccountManagementSession.BusinessAddress.BuildingNumber = "20";
        _journeySession.AccountManagementSession.BusinessAddress.Street = "street";
        _journeySession.AccountManagementSession.BusinessAddress.Town = "town";
        _journeySession.AccountManagementSession.BusinessAddress.County = "Yorkshire";

        //Act
        var result = await SystemUnderTest.BusinessAddress();

        //Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<BusinessAddressViewModel>();
        var model = (BusinessAddressViewModel)viewResult.Model!;

        AssertBackLink(viewResult, PagePath.BusinessAddressPostcode);

        SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
        Assert.AreEqual("sub building name", model.SubBuildingName);
        Assert.AreEqual("building name", model.BuildingName);
        Assert.AreEqual("20", model.BuildingNumber);
        Assert.AreEqual("street", model.Street);
        Assert.AreEqual("town", model.Town);
        Assert.AreEqual("Yorkshire", model.County);
    }

    [TestMethod]
    public async Task GivenNullBusinessAddress_WhenBusinessAddressCalled_ThenUpdatesModelWIthCorrectValue()
    {
        //Arrange
        _journeySession.AccountManagementSession.BusinessAddress = null;

        //Act
        var result = await SystemUnderTest.BusinessAddress();

        //Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<BusinessAddressViewModel>();
        var model = (BusinessAddressViewModel)viewResult.Model!;

        AssertBackLink(viewResult, PagePath.BusinessAddressPostcode);

        SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
        Assert.AreEqual(null, model.SubBuildingName);
        Assert.AreEqual(null, model.BuildingName);
        Assert.AreEqual(null, model.BuildingNumber);
        Assert.AreEqual(null, model.Street);
        Assert.AreEqual(null, model.Town);
        Assert.AreEqual(null, model.County);
    }

    [TestMethod]
    public async Task UserNavigatesToBusinessAddressPage_FromCheckYourDetailsPage_BackLinkShouldBeBusinessAddressPostcode()
    {
        //Arrange
        var journeySession = new JourneySession
        {
            AccountManagementSession = new AccountManagementSession{
                Journey = new List<string>
                {
                    PagePath.BusinessAddressPostcode, PagePath.SelectBusinessAddress,PagePath.BusinessAddress, PagePath.UkNation, PagePath.CheckYourDetails
                },
                BusinessAddress = new FrontendAccountManagement.Core.Addresses.Address()
            }
        };

        SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

        //Act
        var result = await SystemUnderTest.BusinessAddress();

        //Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().BeOfType<BusinessAddressViewModel>();
        AssertBackLink(viewResult, PagePath.BusinessAddressPostcode);

    }

    [TestMethod]
    public async Task GivenCompaniesHouseUser_WhenGetBusinessAddressCalled_ThenRedirectToErrorPage_AndUpdateSession()
    {
        // Arrange
        _userData.Organisations[0].CompaniesHouseNumber = "123";
        SetupBase(_userData);

        // Act
        var result = await SystemUnderTest.BusinessAddress();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(ErrorController.Error));
    }

    [TestMethod]
    public async Task GivenCompaniesHouseUser_WhenBusinessAddressCalled_ThenRedirectToErrorPage_AndUpdateSession()
    {
        // Arrange
        _userData.Organisations[0].CompaniesHouseNumber = "123";
        SetupBase(_userData);

        var request = new BusinessAddressViewModel { BuildingNumber = "10", SubBuildingName = "Dummy House", Postcode = "AB01 BB3", Town = "Nowhere" };

        // Act
        var result = await SystemUnderTest.BusinessAddress(request);

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(ErrorController.Error));
    }

    [TestMethod]
    public async Task GivenUserWithNoOrganisation_WhenBusinessAddressCalled_ThrowInvalidOperationException()
    {
        // Arrange
        Exception result = null;

        _userData.Organisations[0] = null;
        
        SetupBase(_userData);

        // Act
        try
        {
            await SystemUnderTest.BusinessAddress();
        }
        catch (Exception ex)
        {
            result = ex;
        }

        // Assert
        result.Should().BeOfType<InvalidOperationException>();
    }

    [TestMethod]
    public async Task GivenFeatureIsDisabled_WhenBusinessAddressCalled_ThenReturnsToErrorPage_WithNotFoundStatusCode()
    {
        // Arrange
        FeatureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.ManageCompanyDetailChanges))
        .ReturnsAsync(false);

        // Act
        var result = await SystemUnderTest.BusinessAddress();

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
    public async Task GivenFeatureIsDisabled_WhenBusinessAddressSubmitted_ThenReturnsToErrorPage_WithNotFoundStatusCode()
    {
        // Arrange
        FeatureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.ManageCompanyDetailChanges))
        .ReturnsAsync(false);

        var model = new BusinessAddressViewModel { BuildingNumber = "10", SubBuildingName = "Dummy House", Postcode = "AB01 BB3", Town = "Nowhere" };

        // Act
        var result = await SystemUnderTest.BusinessAddress(model);

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
