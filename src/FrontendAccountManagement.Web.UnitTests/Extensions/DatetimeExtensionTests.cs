using System;
using System.Globalization;
using FluentAssertions.Extensions;
using FrontendAccountManagement.Web.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FrontendAccountManagement.Web.UnitTests.Extensions
{
    [TestClass]
    public class DatetimeExtensionTests
    {
        [TestMethod]
        //[DataRow("2001-09-24T02:14:19Z", "03:14")]
        [DataRow("1998-11-24T03:33:08Z", "03:33")]
        [DataRow("2017-12-24T06:06:41Z", "06:06")]
        [DataRow("2047-01-08T16:45:12Z", "16:45")]
        //[DataRow("2049-06-07T19:27:20Z", "20:27")]
        public void CanCallToUkTime(string input, string expected)
        {
            // Arrange
            var utcTime = DateTime.Parse(input, CultureInfo.InvariantCulture).AsUtc();

            // Act
            var result = utcTime.UtcToGmt();

            // Assert
            Assert.AreEqual(expected, result.ToString("HH:mm"));
        }

        [TestMethod]
        public void ToReadableDate_ValidDateTime_ReturnsCorrectFormat()
        {
            // Arrange
            var dateTime = new DateTime(2023, 8, 23, 12, 0, 0, DateTimeKind.Utc); // 23rd August 2023

            // Act
            var result = dateTime.ToReadableDate();

            // Assert
            Assert.AreEqual("23 August 2023", result);
        }

        [TestMethod]
        public void ToShortReadableDate_ValidDateTime_ReturnsCorrectFormat()
        {
            // Arrange
            var dateTime = new DateTime(2023, 8, 23, 14, 30, 0, DateTimeKind.Utc); // 23rd August 2023

            // Act
            var result = dateTime.ToShortReadableDate();

            // Assert
            Assert.AreEqual("23 Aug 2023", result);
        }

        [TestMethod]
        public void ToShortReadableWithShortYearDate_ValidDateTime_ReturnsCorrectFormat()
        {
            // Arrange
            var dateTime = new DateTime(2024, 8, 23, 14, 30, 0, DateTimeKind.Utc); // 23rd August 2024

            // Act
            var result = dateTime.ToShortReadableWithShortYearDate();

            // Assert
            Assert.AreEqual("23 Aug 24", result);
        }

        [TestMethod]
        public void ToReadableDateTime_ValidDateTime_ReturnsCorrectFormat()
        {
            // Arrange
            var dateTime = new DateTime(2023, 8, 23, 09, 0, 0, DateTimeKind.Utc); // 23rd August 2023

            // Act
            var result = dateTime.ToReadableDateTime();

            // Assert
            Assert.AreEqual("23 August 2023, 10:00am", result);
        }

        [TestMethod]
        public void ToTimeHoursMinutes_ValidDateTime_ReturnsCorrectFormat()
        {
            // Arrange
            var dateTime = new DateTime(2023, 8, 23, 09, 0, 0, DateTimeKind.Utc); // 23rd August 2023

            // Act
            var result = dateTime.ToTimeHoursMinutes();

            // Assert
            Assert.AreEqual("10:00am", result);
        }
    }
}