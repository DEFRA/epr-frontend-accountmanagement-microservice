using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.Extensions;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement
{
    [TestClass]
    public class UpdateDetailsConfirmationTests : AccountManagementTestBase
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
        public async Task UpdateDetailsConfirmation_ShouldReturnViewWithExpectedModel()
        {
            // Act
            var result = await SystemUnderTest.UpdateDetailsConfirmation();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.ViewName.Should().Be(null);
            var model = viewResult.Model as UpdateDetailsConfirmationViewModel;
            model.Should().NotBeNull();
            model.Username.Should().Be("Dwight Schrute");
            var changedDateAt = DateTime.UtcNow;
            model.UpdatedDatetime.Should().BeCloseTo(changedDateAt.UtcToGmt(), TimeSpan.FromSeconds(1));
        }

    }
}
