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
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement
{
    [TestClass]
    public class CompanyNameTests : AccountManagementTestBase
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
        public async Task GivenCompanyName_WhenCompanyNameHttpPostCalled_ThenRedirectToUpdateBusinessAddress_AndUpdateSession()
        {
            // Arrange
            var request = new OrganisationNameViewModel { OrganisationName = "Test For company name change" };

            var Sessions = new JourneySession
            {
                AccountManagementSession = new AccountManagementSession
                {
                    OrganisationName = request.OrganisationName
                }
            };
            SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(Sessions);

            // Act
            var result = await SystemUnderTest.CompanyName(request);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();

            ((RedirectToActionResult)result).ActionName.Should().Be(nameof(AccountManagementController.UpdateCompanyAddress));
           
        }

        [TestMethod]
        public async Task GivenNoCompanyName_WhenCompanyNameHttpPostCalled_ThenReturnViewWithErrorAndCorrectBackLink()
        {
            // Arrange
            SystemUnderTest.ModelState.AddModelError(nameof(OrganisationNameViewModel.OrganisationName), "company name field is required");

            // Act
            var result = await SystemUnderTest.CompanyName(new OrganisationNameViewModel());

            // Assert
            result.Should().BeOfType<ViewResult>();

            var viewResult = (ViewResult)result;

            viewResult.Model.Should().BeOfType<OrganisationNameViewModel>();

            
            AssertBackLink(viewResult, PagePath.CompanyName);
        }

        // This test case not executed currently due to back page not available "updatecompanyName" 

        [TestMethod]
        public async Task UserNavigatesToCompanyNamePage_FromUpdateCompanyNamePage_BackLinkShouldBeUpdateCompanyName()
        {
            //Arrange
            var session = new JourneySession
            {
                AccountManagementSession = new AccountManagementSession
                {
                    OrganisationName = "Company Name"
                }
            };

            SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);
            //Act
            var result = await SystemUnderTest.CompanyName();

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            var viewResult = (ViewResult)result;
            viewResult.Model.Should().BeOfType<OrganisationNameViewModel>();
            AssertBackLink(viewResult, PagePath.UpdateCompanyName);

        }
    }
}