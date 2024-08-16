using FrontendAccountManagement.Web.Extensions;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using System.Globalization;

namespace FrontendAccountManagement.Web.UnitTests.ViewModels
{
    [TestClass]
    public class DetailsChangeRequestedViewModelTests
    {

        [TestMethod]
        public void GetFormattedChangeMessage_ShouldReturnCorrectMessage_ForValidInput()
        {
            // Arrange
            var username = "Dwight Schrute";
            var updatedDateTime = new DateTime(2024, 6, 25, 12, 6, 0, DateTimeKind.Utc);
            var viewModel = new DetailsChangeRequestedViewModel()
            {
                Username = username,
                UpdatedDatetime = updatedDateTime
            };

            // Act
            var result = viewModel.GetFormattedChangeMessage("Requested");

            // Assert
            var expectedMessage = "Requested by Dwight Schrute at 01:06pm on 25th June 2024";
            result.Should().Be(expectedMessage);
        }
    }
}