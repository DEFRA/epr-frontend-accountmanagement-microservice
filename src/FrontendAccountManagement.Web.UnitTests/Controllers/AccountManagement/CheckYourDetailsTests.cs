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
        private JourneySession _journeySession;

        [TestInitialize]
        public void Setup()
        {
            _userData = new UserData
            {
                FirstName = "TestFirst",
                LastName = "TestLast",
            };

            SetupBase(_userData);

            _journeySession = new JourneySession
            {
                UserData = _userData,
                AccountManagementSession = new AccountManagementSession() { Journey = new List<string>() }
            };

            SessionManagerMock.Setup(sm => sm.GetSessionAsync(It.IsAny<ISession>()))
                .Returns(Task.FromResult(_journeySession)
                   );
        }

        //[TestMethod]
        //public async Task CheckYourDetails_ShouldReturnViewWithExpectedModel()  
        //{
        //    // Act
        //    var result = await SystemUnderTest.CheckYourDetails();

        //    // Assert
        //    var viewResult = result as ViewResult;
        //    viewResult.Should().NotBeNull();
        //    var model = viewResult.Model as EditUserDetailsViewModel;
        //    model.Should().NotBeNull();
        //    model.FirstName.Should().Be("TestFirst");
        //    model.LastName.Should().Be("TestLast");
        //}

        //[TestMethod]
        //public async Task CheckYourDetailsPost_ShouldReturnActionName() 
        //{
        //    // Act        
        //    var editUserDetailsViewModel = new EditUserDetailsViewModel
        //    {
        //        FirstName = "TestFirst",
        //        LastName = "TestLast",
        //    };

        //    var result = await SystemUnderTest.CheckYourDetails(editUserDetailsViewModel);

        //    // Assert
        //    result.Should().NotBeNull();
        //    ((RedirectToActionResult)result).ActionName.Should().NotBeNull();
        //}

        #region Private
        
        private static IEnumerable<object[]> CheckYourDetailsData
        {
            get
            {
                return new[]
                {
                    // Check journey is added to correctly when navigating from the management page.
                    new[]
                    {
                        new List<string> { "manage" },
                        new List<string> { "manage", "check-your-details" },
                    },

                    // Check the journey is added to correctly when navigating from the "what-are-your-details" page.
                    new[]
                    {
                        new List<string> { "manage", "check-your-details", "what-are-your-details" },
                        new List<string> { "manage", "check-your-details", "what-are-your-details", "check-your-details" },
                    },

                    // Check the journey isn't changed when the user refreshes the page.
                    new[] 
                    {
                        new List<string> { "manage", "check-your-details" }, 
                        new List<string> { "manage", "check-your-details" },
                    }
                };
            }
        }

        #endregion
    }
}
