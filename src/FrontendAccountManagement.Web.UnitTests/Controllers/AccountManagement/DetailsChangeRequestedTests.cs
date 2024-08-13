using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement
{
    [TestClass]
    public class DetailsChangeRequestedTests : AccountManagementTestBase
    {
        private UserData _userData;

        [TestInitialize]
        public void Setup()
        {
            _userData = new UserData
            {
                FirstName = "Dwight",
                LastName = "Schrute",
            };

            SetupBase(_userData);
           
            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
                .Returns(Task.FromResult(new JourneySession { UserData = _userData }));
        }

        [TestMethod]
        public async Task DetailsChangeRequested_ShouldReturnViewWithExpectedModel()
        {
            // Act
            var result = await SystemUnderTest.DetailsChangeRequested();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.ViewName.Should().Be(nameof(SystemUnderTest.DetailsChangeRequested));
            var model = viewResult.Model as DetailsChangeRequestedViewModel;
            model.Should().NotBeNull();
            model.Username.Should().Be("Dwight Schrute");
            model.UpdatedDatetime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }
    }
}
