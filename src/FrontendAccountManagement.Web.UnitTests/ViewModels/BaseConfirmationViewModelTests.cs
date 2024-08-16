using FrontendAccountManagement.Web.Extensions;
using FrontendAccountManagement.Web.ViewModels.AccountManagement;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using static System.Net.Mime.MediaTypeNames;
using System.Globalization;

namespace FrontendAccountManagement.Web.UnitTests.ViewModels
{
    [TestClass]
    public class BaseConfirmationViewModelTests
    {

        [DataTestMethod]
        [DataRow("Changed")]
        [DataRow("Requested")]
        public void GetFormattedChangeMessage_ShouldReturnCorrectMessage_ForValidInput(string action)
        {
            // Arrange
            var username = "Dwight Schrute";
            var updatedDateTime = new DateTime(2024, 6, 25, 12, 6, 0, DateTimeKind.Utc);
            var viewModel = new BaseConfirmationViewModel
            {
                Username = username,
                UpdatedDatetime = updatedDateTime
            };

            // Act
            var result = viewModel.GetFormattedChangeMessage(action);

            // Assert
            var expectedMessage = $"{action} by Dwight Schrute at 01:06pm on 25th June 2024";
            result.Should().Be(expectedMessage);
        }

        [DataTestMethod]
        [DataRow("Test User", "2024-06-01 00:00:00 UTC", "Changed by Test User at 01:00am on 01st June 2024")]
        [DataRow("Test User", "2024-06-02 00:00:00 UTC", "Changed by Test User at 01:00am on 02nd June 2024")]
        [DataRow("Test User", "2024-06-03 00:00:00 UTC", "Changed by Test User at 01:00am on 03rd June 2024")]
        [DataRow("Test User", "2024-06-04 00:00:00 UTC", "Changed by Test User at 01:00am on 04th June 2024")]
        [DataRow("Test User", "2024-06-21 00:00:00 UTC", "Changed by Test User at 01:00am on 21st June 2024")]
        [DataRow("Test User", "2024-06-22 00:00:00 UTC", "Changed by Test User at 01:00am on 22nd June 2024")]
        [DataRow("Test User", "2024-06-23 00:00:00 UTC", "Changed by Test User at 01:00am on 23rd June 2024")]
        public void GetFormattedChangeMessage_ShouldReturnCorrectOrdinal_ForDaysWithSpecialSuffixes(
            string username, string dateString, string expectedMessage)
        {
            // Arrange
            string pattern = "yyyy-MM-dd HH:mm:ss 'UTC'";
            var date = DateTime.ParseExact(dateString, pattern, CultureInfo.InvariantCulture);
            var viewModel = new BaseConfirmationViewModel
            {
                Username = username,
                UpdatedDatetime = date
            };

            // Act
            var result = viewModel.GetFormattedChangeMessage("Changed");

            // Assert
            Assert.AreEqual(expectedMessage, result);
        }
    }
}