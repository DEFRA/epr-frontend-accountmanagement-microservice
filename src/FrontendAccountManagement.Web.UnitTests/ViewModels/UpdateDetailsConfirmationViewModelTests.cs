using FrontendAccountManagement.Web.ViewModels.AccountManagement;

namespace FrontendAccountManagement.Web.UnitTests.ViewModels
{
    [TestClass]
    public class UpdateDetailsConfirmationViewModelTests
    {

        [TestMethod]
        public void GetFormattedChangeMessage_ShouldReturnCorrectMessage_ForValidInput()
        {
            // Arrange
            var username = "Dwight Schrute";
            var updatedDateTime = new DateTime(2024, 6, 25, 12, 6, 0);
            var viewModel = new UpdateDetailsConfirmationViewModel
            {
                Username = username,
                UpdatedDatetime = updatedDateTime
            };

            // Act
            var result = viewModel.GetFormattedChangeMessage();

            // Assert
            var expectedMessage = "Changed by Dwight Schrute at 12:06pm on 25th June 2024";
            result.Should().Be(expectedMessage);
        }

        [TestMethod]
        public void GetFormattedChangeMessage_ShouldReturnCorrectOrdinal_ForDaysWithSpecialSuffixes()
        {
            // Arrange
            var username = "Test User";
            var testCases = new[]
            {
                new
                {
                    Date = new DateTime(2024, 6, 1),
                    ExpectedMessage = "Changed by Test User at 12:00am on 01st June 2024"
                },
                new
                {
                    Date = new DateTime(2024, 6, 2),
                    ExpectedMessage = "Changed by Test User at 12:00am on 02nd June 2024"
                },
                new
                {
                    Date = new DateTime(2024, 6, 3),
                    ExpectedMessage = "Changed by Test User at 12:00am on 03rd June 2024"
                },
                new
                {
                    Date = new DateTime(2024, 6, 4),
                    ExpectedMessage = "Changed by Test User at 12:00am on 04th June 2024"
                },
                new
                {
                    Date = new DateTime(2024, 6, 21),
                    ExpectedMessage = "Changed by Test User at 12:00am on 21st June 2024"
                },
                new
                {
                    Date = new DateTime(2024, 6, 22),
                    ExpectedMessage = "Changed by Test User at 12:00am on 22nd June 2024"
                },
                new
                {
                    Date = new DateTime(2024, 6, 23),
                    ExpectedMessage = "Changed by Test User at 12:00am on 23rd June 2024"
                }
            };

            foreach (var testCase in testCases)
            {
                var viewModel = new UpdateDetailsConfirmationViewModel
                {
                    Username = username,
                    UpdatedDatetime = testCase.Date
                };

                // Act
                var result = viewModel.GetFormattedChangeMessage();

                // Assert
                result.Should().Be(testCase.ExpectedMessage);
            }
        }
    }
}