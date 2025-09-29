using System.Net;
using AutoFixture;
using EPR.Common.Authorization.Models;
using FluentAssertions.Execution;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.Constants;
using FrontendAccountManagement.Web.Controllers.Errors;
using FrontendAccountManagement.Web.ViewModels;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement
{
    [TestClass]
    public class UpdateCompanyNameTests : AccountManagementTestBase
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
        public async Task Get_UpdateCompanyName_ShouldRedirectIfOrganisationIsCompaniesHouseCompany()
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
            var result = await SystemUnderTest.UpdateCompanyName();

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>();
                var redirectResult = result as RedirectToActionResult;
                redirectResult.ControllerName.Should().Be("Error");
                redirectResult.RouteValues["statusCode"].Should().Be((int)HttpStatusCode.NotFound);
            }
        }

        [TestMethod]
        public async Task Get_UpdateCompanyName_ShouldReturnViewWithModelWhenSessionIsNotNull()
        {
            // Arrange
            var session = new JourneySession
            {
                AccountManagementSession = new AccountManagementSession
                {
                    OrganisationType = OrganisationType.NonCompaniesHouseCompany,
                    IsUpdateCompanyName = true  
                }
            };
            SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

            // Act
            var result = await SystemUnderTest.UpdateCompanyName();

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ViewResult>();
                var viewResult = result as ViewResult;
                var model = viewResult.Model as UpdateCompanyNameViewModel;

                model.Should().NotBeNull();
                model.IsUpdateCompanyName.Should().Be(YesNoAnswer.Yes);

                AssertBackLink(viewResult, PagePath.ManageAccount);
            }
        }

        [TestMethod]
        public async Task Get_UpdateCompanyName_ShouldReturnViewWithModelWhenIsUpdateCompanyNameIsFalse()
        {
            // Arrange
            var session = new JourneySession
            {
                AccountManagementSession = new AccountManagementSession
                {
                    OrganisationType = OrganisationType.NonCompaniesHouseCompany,
                    IsUpdateCompanyName = false  
                }
            };
            SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

            // Act
            var result = await SystemUnderTest.UpdateCompanyName();

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ViewResult>();
                var viewResult = result as ViewResult;
                var model = viewResult.Model as UpdateCompanyNameViewModel;

                model.Should().NotBeNull();
                model.IsUpdateCompanyName.Should().Be(YesNoAnswer.No);  
            }
        }

        [TestMethod]
        public async Task Get_UpdateCompanyName_ShouldReturnViewWithModelWhenIsUpdateCompanyNameIsNull()
        {
            // Arrange
            var session = new JourneySession
            {
                AccountManagementSession = new AccountManagementSession
                {
                    OrganisationType = OrganisationType.NonCompaniesHouseCompany,
                    IsUpdateCompanyName = null  
                }
            };
            SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

            // Act
            var result = await SystemUnderTest.UpdateCompanyName();

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ViewResult>();
                var viewResult = result as ViewResult;
                var model = viewResult.Model as UpdateCompanyNameViewModel;

                model.Should().NotBeNull();
                model.IsUpdateCompanyName.Should().BeNull();  
            }
        }


        [TestMethod]
        public async Task Post_UpdateCompanyName_ShouldReturnRedirectWhenModelIsValid()
        {
            // Arrange
            var model = new UpdateCompanyNameViewModel { IsUpdateCompanyName = YesNoAnswer.Yes };
            var session = new JourneySession
            {
                AccountManagementSession = new AccountManagementSession()
            };
            SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

            // Act
            var result = await SystemUnderTest.UpdateCompanyName(model);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>();
                var redirectResult = result as RedirectToActionResult;
                redirectResult.ActionName.Should().Be(nameof(SystemUnderTest.CompanyName));
            }
        }

        [TestMethod]
        public async Task Post_UpdateCompanyName_ShouldReturnRedirectUpdateIsFalseAndWhenModelIsValid()
        {
            // Arrange
            var model = new UpdateCompanyNameViewModel { IsUpdateCompanyName = YesNoAnswer.No };
            var session = new JourneySession
            {
                AccountManagementSession = new AccountManagementSession()
            };
            SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

            // Act
            var result = await SystemUnderTest.UpdateCompanyName(model);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>();
                var redirectResult = result as RedirectToActionResult;
                redirectResult.ActionName.Should().Be(nameof(SystemUnderTest.UpdateCompanyAddress));
            }
        }

        [TestMethod]
        public async Task Post_UpdateCompanyName_ShouldReturnRedirectWhenModelIsNotValid()
        {
            // Arrange
            var session = new JourneySession
            {
                AccountManagementSession = new AccountManagementSession
                {
                    OrganisationType = OrganisationType.NonCompaniesHouseCompany,
                    IsUpdateCompanyName = true,
                    Journey = new List<string> { PagePath.ManageAccount, PagePath.UpdateCompanyName }
                }
            };
            SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

            var model = new UpdateCompanyNameViewModel();
            SystemUnderTest.ModelState.AddModelError("Error", "Some error message");

            // Act
            var result = await SystemUnderTest.UpdateCompanyName(model);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ViewResult>();
                var viewResult = result as ViewResult;
                viewResult.Model.Should().Be(model);

                AssertBackLink(viewResult, PagePath.ManageAccount);
            }
        }

        [TestMethod]
        public async Task GivenFeatureIsDisabled_WhenUpdateCompanyNameCalled_ThenReturnsToErrorPage_WithNotFoundStatusCode()
        {
            // Arrange
            FeatureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.ManageCompanyDetailChanges))
            .ReturnsAsync(false);

            // Act
            var result = await SystemUnderTest.UpdateCompanyName();

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
        public async Task GivenFeatureIsDisabled_WhenUpdateCompanyNameSubmitted_ThenReturnsToErrorPage_WithNotFoundStatusCode()
        {
            // Arrange
            FeatureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.ManageCompanyDetailChanges))
            .ReturnsAsync(false);

            var model = new UpdateCompanyNameViewModel { IsUpdateCompanyName = YesNoAnswer.Yes };

            // Act
            var result = await SystemUnderTest.UpdateCompanyName(model);

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
