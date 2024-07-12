using FrontendAccountManagement.Web.Extensions;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;

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
            var updatedDateTime = new DateTime(2024, 6, 25, 12, 6, 0);
            var viewModel = new DetailsChangeRequestedViewModel()
            {
                Username = username,
                UpdatedDatetime = updatedDateTime
            };

            // Act
            var result = viewModel.GetFormattedChangeMessage("Requested");

            // Assert
            var expectedMessage = "Requested by Dwight Schrute at 12:06pm on 25th June 2024";
            result.Should().Be(expectedMessage);
        }

        [DataTestMethod]
        [DataRow("Test User", "2024-06-01", "Requested by Test User at 12:00am on 01st June 2024")]
        [DataRow("Test User", "2024-06-02", "Requested by Test User at 12:00am on 02nd June 2024")]
        [DataRow("Test User", "2024-06-03", "Requested by Test User at 12:00am on 03rd June 2024")]
        [DataRow("Test User", "2024-06-04", "Requested by Test User at 12:00am on 04th June 2024")]
        [DataRow("Test User", "2024-06-21", "Requested by Test User at 12:00am on 21st June 2024")]
        [DataRow("Test User", "2024-06-22", "Requested by Test User at 12:00am on 22nd June 2024")]
        [DataRow("Test User", "2024-06-23", "Requested by Test User at 12:00am on 23rd June 2024")]
        public void GetFormattedChangeMessage_ShouldReturnCorrectOrdinal_ForDaysWithSpecialSuffixes(
            string username, string dateString, string expectedMessage)
        {
            // Arrange
            var date = DateTime.Parse(dateString);
            var viewModel = new DetailsChangeRequestedViewModel
            {
                Username = username,
                UpdatedDatetime = date
            };

            // Act
            var result = viewModel.GetFormattedChangeMessage("Requested");

            // Assert
            Assert.AreEqual(expectedMessage, result);
        }
    }
}