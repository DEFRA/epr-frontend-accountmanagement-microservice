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
using FrontendAccountManagement.Web.Configs;


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
        public async Task NextPage_ShouldNotRedirectTo_UpdateCompanyAddress_WhenOrganisationNameBlank()
        {
            // Arrange
            var viewModel = new OrganisationNameViewModel
            {
                OrganisationName = ""
            };
            var session = new JourneySession
            {
                AccountManagementSession = new AccountManagementSession
                {
                    Journey = new List<string> { PagePath.UpdateCompanyName, PagePath.CompanyName },

                }
            };
            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

            SystemUnderTest.ModelState.AddModelError(nameof(OrganisationNameViewModel.OrganisationName), "Enter an organisation name");


            // Act
            var result = await SystemUnderTest.CompanyName(viewModel);

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ViewResult>();
                var viewResult = (ViewResult)result;
                viewResult.Model.Should().BeOfType<OrganisationNameViewModel>();

                SessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<JourneySession>>()), Times.Never);

                // Check that the back link redirects to UpdateCompanyName page
                AssertBackLink(viewResult, PagePath.UpdateCompanyName);
            }
                


        }
        [TestMethod]
        public async Task NextPage_ShouldNotRedirectTo_UpdateCompanyAddress_WhenOrganisationNameMoreThen170character()
        {
            // Arrange
            var viewModel = new OrganisationNameViewModel
            {
                OrganisationName = new string('A', 171)
            };
            var session = new JourneySession
            {
                AccountManagementSession = new AccountManagementSession
                {
                    Journey = new List<string> { PagePath.UpdateCompanyName, PagePath.UpdateCompanyAddress },

                }
            };
            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

            SystemUnderTest.ModelState.AddModelError(nameof(OrganisationNameViewModel.OrganisationName), "Organisation name must be 170 characters or less");

            // Act
            var result = await SystemUnderTest.CompanyName(viewModel);

            // Assert

            result.Should().BeOfType<ViewResult>();
            var viewResult = (ViewResult)result;
            viewResult.Model.Should().BeOfType<OrganisationNameViewModel>();



            SessionManagerMock.Verify(x => x.UpdateSessionAsync(It.IsAny<ISession>(), It.IsAny<Action<JourneySession>>()), Times.Never);


        }


        [TestMethod]
        public async Task UserNavigatesToCompanyNamePage_FromUpdateCompanyNamePage_BackLinkShouldBeUpdateCompanyName()
        {
            // Arrange
            var session = new JourneySession
            {
                UserData = new UserData
                {
                    Organisations = new List<Organisation>
                {
                    new Organisation { OrganisationType = OrganisationType.NonCompaniesHouseCompany,Name="Company Name"}
                }
                },
                AccountManagementSession = new AccountManagementSession
                {
                    Journey = new List<string> { PagePath.UpdateCompanyName, PagePath.UpdateCompanyAddress }
                }
            };

            SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

            // Act
            var result = await SystemUnderTest.CompanyName();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ViewResult>();
            var viewResult = (ViewResult)result;
            viewResult.Model.Should().BeOfType<OrganisationNameViewModel>();

            // Check that the back link redirects to UpdateCompanyName page
            AssertBackLink(viewResult, PagePath.UpdateCompanyName);
        }

        [TestMethod]
        public async Task UserNavigatesToCompanyNamePage_GivenNullSession_BackLinkShouldBeUpdateCompanyName()
        {
            // Arrange
            SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>()))
                .ReturnsAsync((JourneySession)null);

            // Act
            var result = await SystemUnderTest.CompanyName();

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>();
                var redirectResult = result as RedirectToActionResult;
                redirectResult.ControllerName.Should().Be(nameof(ErrorController.Error));
                redirectResult.RouteValues["statusCode"].Should().Be((int)HttpStatusCode.InternalServerError);
            }
        }

        [TestMethod]
        public async Task UserNavigatesToCompanyNamePage_GivenNullAccountManagementSession_BackLinkShouldBeUpdateCompanyName()
        {
            // Arrange
            SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(new JourneySession { AccountManagementSession = null });

            // Act
            var result = await SystemUnderTest.CompanyName();

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>();
                var redirectResult = result as RedirectToActionResult;
                redirectResult.ControllerName.Should().Be(nameof(ErrorController.Error));
                redirectResult.RouteValues["statusCode"].Should().Be((int)HttpStatusCode.InternalServerError);
            }
        }

        [TestMethod]
        public async Task UserNavigatesToCompanyNamePage_GivenNullJourney_BackLinkShouldBeUpdateCompanyName()
        {
            // Arrange
            SessionManagerMock.Setup(x => x.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(new JourneySession { AccountManagementSession = new AccountManagementSession { Journey = null } });

            // Act
            var result = await SystemUnderTest.CompanyName();

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>();
                var redirectResult = result as RedirectToActionResult;
                redirectResult.ControllerName.Should().Be(nameof(ErrorController.Error));
                redirectResult.RouteValues["statusCode"].Should().Be((int)HttpStatusCode.InternalServerError);
            }
        }


        [TestMethod]
        public async Task ShouldRedirectToError_AccessingOutside_WhenUserCompanieshouse()
        {
            // Arrange
            var session = new JourneySession
            {
                UserData = new UserData
                {
                    Organisations = new List<Organisation>
            {
                new Organisation { OrganisationType = OrganisationType.CompaniesHouseCompany }
            }
                },
                AccountManagementSession = new AccountManagementSession()
            };

            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

            // Act
            var result = await SystemUnderTest.CompanyName();

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<RedirectToActionResult>();
                var redirectResult = result.As<RedirectToActionResult>();

                redirectResult.ControllerName.Should().Be(nameof(ErrorController.Error));
                redirectResult.ActionName.Should().Be(PagePath.Error);
                redirectResult.RouteValues.Should().ContainKey("statusCode");
                redirectResult.RouteValues["statusCode"].Should().Be((int)HttpStatusCode.NotFound);
            }
                
        }


        [TestMethod]
        public async Task ShouldRedirectToCompanyPage_AccessingOutside_WhenUserNonCompanieshouse()
        {
            // Arrange
            var session = new JourneySession
            {
                AccountManagementSession = new AccountManagementSession
                {
                    OrganisationName = "Company Name"
                },
                UserData = new UserData
                {
                    Organisations = new List<Organisation>
            {
                new Organisation
                {
                    OrganisationType = OrganisationType.NonCompaniesHouseCompany,
                    Name = "Company Name"
                }
            }
                }
            };

            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);

            // Act
            var result = await SystemUnderTest.CompanyName();

            // Assert
            using (new AssertionScope())
            {
                result.Should().BeOfType<ViewResult>()
                  .Which.Model.Should().BeOfType<OrganisationNameViewModel>()
                  .Which.OrganisationName.Should().Be("Company Name");

                var viewResult = result.As<ViewResult>();
                viewResult.ViewName.Should().BeNull();
            }
        }


        [TestMethod]
        public async Task RedirectToNextPage_UpdatedOrganisationName_AssigninBackToSession()
        {
            var initialOrganisationName = "Initial Company Name";
            var updatedOrganisationName = "Updated Company Name";

            // Setting up the initial session with a dummy organisation name
            var session = new JourneySession
            {
                AccountManagementSession = new AccountManagementSession
                {
                    OrganisationName = initialOrganisationName
                },
                UserData = new UserData
                {
                    Organisations = new List<Organisation>
                     {
                         new Organisation { OrganisationType = OrganisationType.NonCompaniesHouseCompany, Name = initialOrganisationName }
                    }
                }
            };

            // Create a view model with the updated organisation name
            var viewModel = new OrganisationNameViewModel
            {
                OrganisationName = updatedOrganisationName
            };

            // Mock the session manager to return the initial session
            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>())).ReturnsAsync(session);



            // Act
            var result = await SystemUnderTest.CompanyName(viewModel);
            var redirectToActionResult = (RedirectToActionResult)result;

            // Assert
            // Verify that the session's OrganisationName has been updated
            using (new AssertionScope())
            {
                session.AccountManagementSession.OrganisationName.Should().Be(updatedOrganisationName);
                redirectToActionResult.ActionName.Should().Be("UpdateCompanyAddress");
            }


        }

        [TestMethod]
        public async Task GivenFeatureIsDisabled_WhenCompanyNameCalled_ThenReturnsToErrorPage_WithNotFoundStatusCode()
        {
            // Arrange
            FeatureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.ManageCompanyDetailChanges))
            .ReturnsAsync(false);

            // Act
            var result = await SystemUnderTest.CompanyName();

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
        public async Task GivenFeatureIsDisabled_WhenCompanyNameSubmitted_ThenReturnsToErrorPage_WithNotFoundStatusCode()
        {
            // Arrange
            FeatureManagerMock.Setup(x => x.IsEnabledAsync(FeatureFlags.ManageCompanyDetailChanges))
            .ReturnsAsync(false);

            var updatedOrganisationName = "Updated Company Name";
            var model = new OrganisationNameViewModel
            {
                OrganisationName = updatedOrganisationName
            };

            // Act
            var result = await SystemUnderTest.CompanyName(model);

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