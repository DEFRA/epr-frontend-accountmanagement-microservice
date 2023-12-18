namespace FrontendAccountManagement.Web.UnitTests.Utilities
{
    [TestClass]
    public class RegexUtilitiesTests
    {        
        protected readonly string validEmail = "an.other@test.com";
        protected readonly string inValidEmail = "an.othertest.com";
        protected readonly string emptyEmail = "";
        protected readonly string nullEmail = null;

        [TestMethod]
        public async Task GivenOnAnyPage_WhenIsValidEmailIsCalledWithaValidEmail_ThenTureIsReturned()
        {
            // Act
            var result = RegexUtilities.IsValidEmail(validEmail);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public async Task GivenOnAnyPage_WhenIsValidEmailIsCalledWithanInvalidEmail_ThenFalseIsReturned()
        {
            // Act
            var result = RegexUtilities.IsValidEmail(inValidEmail);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GivenOnAnyPage_WhenIsValidEmailIsCalledWithaNullEmail_ThenFalseIsReturned()
        {
            // Act
            var result = RegexUtilities.IsValidEmail(nullEmail);

            // Assert
            result.Should().BeFalse();
        }

        [TestMethod]
        public async Task GivenOnAnyPage_WhenIsValidEmailIsCalledWithAnEmptyEmail_ThenFalseIsReturned()
        {
            // Act
            var result = RegexUtilities.IsValidEmail(emptyEmail);

            // Assert
            result.Should().BeFalse();
        }
    }
}