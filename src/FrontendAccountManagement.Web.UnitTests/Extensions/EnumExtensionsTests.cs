using FrontendAccountManagement.Web.Extensions;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountManagement.Web.UnitTests.Extensions
{
    [TestClass]
    public class EnumExtensionsTests
    {
        private enum Test
        {
            [Display(Name = "First Value")]
            FirstValue,

            SecondValue,

            [Display(Name = "Third Value")]
            ThirdValue
        }

        [TestMethod]
        public void GetDisplayName_ReturnsDisplayName_WhenDisplayAttributeIsPresent()
        {
            // Arrange
            var enumValue = Test.FirstValue;

            // Act
            var displayName = enumValue.GetDisplayName();

            // Assert
            Assert.AreEqual("First Value", displayName);
        }

        [TestMethod]
        public void GetDisplayName_ReturnsEnumName_WhenDisplayAttributeIsNotPresent()
        {
            // Arrange
            var enumValue = Test.SecondValue;

            // Act
            var displayName = enumValue.GetDisplayName();

            // Assert
            Assert.AreEqual("SecondValue", displayName);
        }

        [TestMethod]
        public void GetDisplayName_ReturnsDisplayName_ForAnotherEnumValueWithDisplayAttribute()
        {
            // Arrange
            var enumValue = Test.ThirdValue;

            // Act
            var displayName = enumValue.GetDisplayName();

            // Assert
            Assert.AreEqual("Third Value", displayName);
        }

        [TestMethod]
        public void GetDisplayName_ReturnsStringOfValue_WhenInvalidEnum()
        {
            // Arrange
            var testVal = 8;
            var invalidEnum = (Test)testVal;

            // Act
            var displayName = invalidEnum.GetDisplayName();

            // Assert
            Assert.AreEqual(testVal.ToString(), displayName);
        }
    }
}
