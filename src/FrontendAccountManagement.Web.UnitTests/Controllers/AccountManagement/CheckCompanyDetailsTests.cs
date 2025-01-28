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

    //TODO: Mark - Revisit during Integration Testing

    //[TestMethod]
    //public async Task GivenFinishedPreviousPage_WhenCheckCompanyDetailsCalled_ThenCheckCompanyDetailsPageReturned_WithUpdateCompanyAddressAsTheBackLink()
    //{
    //    //Arrange


    //    //Act
    //    var result = await SystemUnderTest.CheckCompanyDetails();

    //    //Assert
    //    result.Should().BeOfType<ViewResult>();

    //    var viewResult = (ViewResult)result;

    //    viewResult.Model.Should().BeOfType<CheckCompanyDetailsViewModel>();

    //    AssertBackLink(viewResult, PagePath.UpdateCompanyAddress);
    //}

    //[TestMethod]
    //public async Task GivenNoPreviousPage_WhenCheckCompanyDetailsCalled_ThenCheckCompanyDetailsPageReturned_WithUpdateCompanyAddressAsTheBackLink()
    //{
    //    //Arrange
    //    _journeySession.AccountManagementSession.Journey = new List<string>();

    //    //Act
    //    var result = await SystemUnderTest.CheckCompanyDetails();

    //    //Assert
    //    result.Should().BeOfType<ViewResult>();

    //    var viewResult = (ViewResult)result;

    //    viewResult.Model.Should().BeOfType<CheckCompanyDetailsViewModel>();

    //    AssertBackLink(viewResult, PagePath.UpdateCompanyAddress);
    //}
}
