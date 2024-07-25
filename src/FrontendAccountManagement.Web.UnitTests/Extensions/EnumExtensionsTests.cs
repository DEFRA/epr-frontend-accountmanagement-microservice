using FrontendAccountManagement.Web.Extensions;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountManagement.Web.UnitTests.Extensions
{
    [TestClass]
    public class EnumExtensionsTests
    {
        public enum TestEnum
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
            var enumValue = TestEnum.FirstValue;

            // Act
            var displayName = enumValue.GetDisplayName();

            // Assert
            Assert.AreEqual("First Value", displayName);
        }

        [TestMethod]
        public void GetDisplayName_ReturnsEnumName_WhenDisplayAttributeIsNotPresent()
        {
            // Arrange
            var enumValue = TestEnum.SecondValue;

            // Act
            var displayName = enumValue.GetDisplayName();

            // Assert
            Assert.AreEqual("SecondValue", displayName);
        }

        [TestMethod]
        public void GetDisplayName_ReturnsDisplayName_ForAnotherEnumValueWithDisplayAttribute()
        {
            // Arrange
            var enumValue = TestEnum.ThirdValue;

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
            var invalidEnum = (TestEnum)testVal;

            // Act
            var displayName = invalidEnum.GetDisplayName();

            // Assert
            Assert.AreEqual(testVal.ToString(), displayName);
        }
    }
}
