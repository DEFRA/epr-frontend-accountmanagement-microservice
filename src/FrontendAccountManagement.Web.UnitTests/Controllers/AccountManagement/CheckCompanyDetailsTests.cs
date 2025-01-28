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
using FrontendAccountManagement.Core.Enums;
using FrontendAccountManagement.Core.Models;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement;

[TestClass]
public class CheckCompanyDetailsTests : AccountManagementTestBase
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
                PagePath.UpdateCompanyAddress, PagePath.CheckCompanyDetails
            },
                BusinessAddress = new FrontendAccountManagement.Core.Addresses.Address()
            }            
        };

        SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(_journeySession);
    }

    [TestMethod]
    public async Task GivenOrganisationNameModified_WhenCheckCompanyDetailsCalled_ThenRedirectToCompanyDetailsUpdatedPage_FacadeUpdate_AndClearSession()
    {
        // Arrange
        _journeySession.AccountManagementSession.OrganisationName = "New Name";

        FacadeServiceMock.Setup(mock => mock.GetUserAccount())
         .Returns(Task.FromResult(new UserAccountDto
         {

         }));

        // Act
        var result = await SystemUnderTest.CheckCompanyDetailsPost();

        // Assert
        result.Should().BeOfType<RedirectToActionResult>();

        ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountManagementController.CompanyDetailsUpdated));

        FacadeServiceMock.Verify(
             x => x.UpdateOrganisationDetails(
                 It.IsAny<Guid>(),
                 It.Is<OrganisationUpdateDto>(dto => dto.Name == "New Name")
             ),
             Times.Once
         );

        SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
    }

    //[TestMethod]
    //public async Task GivenIncompleteBusinessAddress_WhenBusinessAddressCalled_ThenReturnViewWithError()
    //{
    //    // Arrange
    //    _journeySession.AccountManagementSession.Journey.RemoveAll(x => x == PagePath.SelectBusinessAddress);

    //    SystemUnderTest.ModelState.AddModelError(nameof(BusinessAddressViewModel.BuildingNumber), "Field is required");

    //    var request = new BusinessAddressViewModel { Postcode = "AB01 BB3", Town = "Nowhere" };

    //    // Act
    //    var result = await SystemUnderTest.BusinessAddress(request);

    //    // Assert
    //    result.Should().BeOfType<ViewResult>();

    //    var viewResult = (ViewResult)result;

    //    viewResult.Model.Should().BeOfType<BusinessAddressViewModel>();

    //    SessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<JourneySession>>()), Times.Never);

    //    AssertBackLink(viewResult, PagePath.BusinessAddressPostcode);
    //}

    //[TestMethod]
    //public async Task GivenIncompleteBusinessAddressWithTempData_WhenBusinessAddressCalled_ThenReturnViewWithError()
    //{
    //    // Arrange
    //    _journeySession.AccountManagementSession.Journey.RemoveAll(x => x == PagePath.SelectBusinessAddress);

    //    TempDataDictionaryMock = new Mock<ITempDataDictionary>();
    //    TempDataDictionaryMock.Setup(x => x.ContainsKey(It.IsAny<string>())).Returns(true);

    //    SystemUnderTest.ModelState.AddModelError(nameof(BusinessAddressViewModel.BuildingNumber), "Field is required");
    //    SystemUnderTest.TempData = TempDataDictionaryMock.Object;
    //    var request = new BusinessAddressViewModel { Postcode = "AB01 BB3", Town = "Nowhere" };

    //    // Act
    //    var result = await SystemUnderTest.BusinessAddress(request);

    //    // Assert
    //    result.Should().BeOfType<ViewResult>();

    //    var viewResult = (ViewResult)result;

    //    viewResult.Model.Should().BeOfType<BusinessAddressViewModel>();

    //    var viewModel = (BusinessAddressViewModel)viewResult.Model!;

    //    viewModel.ShowWarning.Should().BeTrue();

    //    SessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<JourneySession>>()), Times.Never);

    //    AssertBackLink(viewResult, PagePath.BusinessAddressPostcode);
    //}

    [TestMethod]
    public async Task GivenFinishedPreviousPage_WhenCheckCompanyDetailsCalled_ThenCheckCompanyDetailsPageReturned_WithUpdateCompanyAddressAsTheBackLink()
    {
        //Arrange


        //Act
        var result = await SystemUnderTest.CheckCompanyDetails();

        //Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<CheckCompanyDetailsViewModel>();

        AssertBackLink(viewResult, PagePath.UpdateCompanyAddress);
    }

    [TestMethod]
    public async Task GivenNoPreviousPage_WhenCheckCompanyDetailsCalled_ThenCheckCompanyDetailsPageReturned_WithUpdateCompanyAddressAsTheBackLink()
    {
        //Arrange
        _journeySession.AccountManagementSession.Journey = new List<string>();

        //Act
        var result = await SystemUnderTest.CheckCompanyDetails();

        //Assert
        result.Should().BeOfType<ViewResult>();

        var viewResult = (ViewResult)result;

        viewResult.Model.Should().BeOfType<CheckCompanyDetailsViewModel>();

        AssertBackLink(viewResult, PagePath.UpdateCompanyAddress);
    }


    //[TestMethod]
    //public async Task GivenBusinessAddress_WhenBusinessAddressIsManualAddress_ThenUpdatesModelWIthCorrectValue()
    //{
    //    //Arrange
    //    _journeySession.AccountManagementSession.BusinessAddress.IsManualAddress = true;
    //    _journeySession.AccountManagementSession.BusinessAddress.SubBuildingName = "sub building name";
    //    _journeySession.AccountManagementSession.BusinessAddress.BuildingName = "building name";
    //    _journeySession.AccountManagementSession.BusinessAddress.BuildingNumber = "20";
    //    _journeySession.AccountManagementSession.BusinessAddress.Street = "street";
    //    _journeySession.AccountManagementSession.BusinessAddress.Town = "town";
    //    _journeySession.AccountManagementSession.BusinessAddress.County = "Yorkshire";

    //    //Act
    //    var result = await SystemUnderTest.BusinessAddress();

    //    //Assert
    //    result.Should().BeOfType<ViewResult>();

    //    var viewResult = (ViewResult)result;

    //    viewResult.Model.Should().BeOfType<BusinessAddressViewModel>();
    //    var model = (BusinessAddressViewModel)viewResult.Model!;

    //    AssertBackLink(viewResult, PagePath.BusinessAddressPostcode);

    //    SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
    //    Assert.AreEqual("sub building name", model.SubBuildingName);
    //    Assert.AreEqual("building name", model.BuildingName);
    //    Assert.AreEqual("20", model.BuildingNumber);
    //    Assert.AreEqual("street", model.Street);
    //    Assert.AreEqual("town", model.Town);
    //    Assert.AreEqual("Yorkshire", model.County);
    //}

    //[TestMethod]
    //public async Task GivenNullBusinessAddress_WhenBusinessAddressCalled_ThenUpdatesModelWIthCorrectValue()
    //{
    //    //Arrange
    //    _journeySession.AccountManagementSession.BusinessAddress = null;

    //    //Act
    //    var result = await SystemUnderTest.BusinessAddress();

    //    //Assert
    //    result.Should().BeOfType<ViewResult>();

    //    var viewResult = (ViewResult)result;

    //    viewResult.Model.Should().BeOfType<BusinessAddressViewModel>();
    //    var model = (BusinessAddressViewModel)viewResult.Model!;

    //    AssertBackLink(viewResult, PagePath.BusinessAddressPostcode);

    //    SessionManagerMock.Verify(x => x.SaveSessionAsync(It.IsAny<ISession>(), It.IsAny<JourneySession>()), Times.Once);
    //    Assert.AreEqual(null, model.SubBuildingName);
    //    Assert.AreEqual(null, model.BuildingName);
    //    Assert.AreEqual(null, model.BuildingNumber);
    //    Assert.AreEqual(null, model.Street);
    //    Assert.AreEqual(null, model.Town);
    //    Assert.AreEqual(null, model.County);
    //}

    //[TestMethod]
    //public async Task UserNavigatesToBusinessAddressPage_FromCheckYourDetailsPage_BackLinkShouldBeBusinessAddressPostcode()
    //{
    //    //Arrange
    //    var journeySession = new JourneySession
    //    {
    //        AccountManagementSession = new AccountManagementSession{
    //            Journey = new List<string>
    //            {
    //                PagePath.BusinessAddressPostcode, PagePath.SelectBusinessAddress,PagePath.BusinessAddress, PagePath.UkNation, PagePath.CheckYourDetails
    //            },
    //            BusinessAddress = new FrontendAccountManagement.Core.Addresses.Address()
    //        }
    //    };

    //    SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(journeySession);

    //    //Act
    //    var result = await SystemUnderTest.BusinessAddress();

    //    //Assert
    //    result.Should().NotBeNull();
    //    result.Should().BeOfType<ViewResult>();
    //    var viewResult = (ViewResult)result;
    //    viewResult.Model.Should().BeOfType<BusinessAddressViewModel>();
    //    AssertBackLink(viewResult, PagePath.BusinessAddressPostcode);

    //}

    //[TestMethod]
    //public async Task GivenCompaniesHouseUser_WhenGetBusinessAddressCalled_ThenRedirectToErrorPage_AndUpdateSession()
    //{
    //    // Arrange
    //    _userData.Organisations[0].CompaniesHouseNumber = "123";
    //    SetupBase(_userData);

    //    // Act
    //    var result = await SystemUnderTest.BusinessAddress();

    //    // Assert
    //    result.Should().BeOfType<RedirectToActionResult>();

    //    ((RedirectToActionResult)result).ActionName.Should().Be(nameof(ErrorController.Error));
    //}

    //[TestMethod]
    //public async Task GivenCompaniesHouseUser_WhenBusinessAddressCalled_ThenRedirectToErrorPage_AndUpdateSession()
    //{
    //    // Arrange
    //    _userData.Organisations[0].CompaniesHouseNumber = "123";
    //    SetupBase(_userData);

    //    var request = new BusinessAddressViewModel { BuildingNumber = "10", SubBuildingName = "Dummy House", Postcode = "AB01 BB3", Town = "Nowhere" };

    //    // Act
    //    var result = await SystemUnderTest.BusinessAddress(request);

    //    // Assert
    //    result.Should().BeOfType<RedirectToActionResult>();

    //    ((RedirectToActionResult)result).ActionName.Should().Be(nameof(ErrorController.Error));
    //}

    //[TestMethod]
    //public async Task GivenUserWithNoOrganisation_WhenBusinessAddressCalled_ThrowInvalidOperationException()
    //{
    //    // Arrange
    //    Exception result = null;

    //    _userData.Organisations[0] = null;
        
    //    SetupBase(_userData);

    //    // Act
    //    try
    //    {
    //        await SystemUnderTest.BusinessAddress();
    //    }
    //    catch (Exception ex)
    //    {
    //        result = ex;
    //    }

    //    // Assert
    //    result.Should().BeOfType<InvalidOperationException>();
    //}
}
