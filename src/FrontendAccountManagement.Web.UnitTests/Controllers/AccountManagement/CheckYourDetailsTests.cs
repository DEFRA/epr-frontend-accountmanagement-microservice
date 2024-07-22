using EPR.Common.Authorization.Models;
using EPR.Common.Authorization.Models;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Core.Sessions;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;

namespace FrontendAccountManagement.Web.UnitTests.Controllers.AccountManagement
{
    /// <summary>
    /// Test the CheckYourDetails functionality
    /// </summary>
    [TestClass]
    public class CheckYourDetailsTests : AccountManagementTestBase
    {
        private UserData _userData;

        [TestInitialize]
        public void Setup()
        {
            _userData = new UserData
            {
                FirstName = "TestFirst",
                LastName = "TestLast",
            };

            SetupBase(_userData);

            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
                .Returns(Task.FromResult(new JourneySession { UserData = _userData }));
        }

        [TestMethod]
        public async Task CheckYourDetails_ShouldReturnViewWithExpectedModel()  
        {
            // Act
            var result = await SystemUnderTest.CheckYourDetails();

            // Assert
            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult.ViewName.Should().Be(nameof(SystemUnderTest.CheckYourDetails));
            var model = viewResult.Model as EditUserDetailsViewModel;
            model.Should().NotBeNull();
            model.FirstName.Should().Be("TestFirst");
            model.LastName.Should().Be("TestLast");
        }
    }
}
