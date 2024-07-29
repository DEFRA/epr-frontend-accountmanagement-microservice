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
            var result = utcTime.ToUkTime();

            // Assert
            Assert.AreEqual(expected, result.ToString("HH:mm"));
        }
    }
}