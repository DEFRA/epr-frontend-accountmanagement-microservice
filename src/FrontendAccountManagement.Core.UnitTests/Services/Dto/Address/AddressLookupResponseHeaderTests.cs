using FrontendAccountManagement.Core.Services.Dto.Address;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FrontendAccountManagement.Core.UnitTests.Services.Dto.Address
{
    [TestClass]
    public class AddressLookupResponseHeaderTests
    {
        [TestMethod]
        public void CanSetAndGetAllProperties()
        {
            // Arrange
            var header = new AddressLookupResponseHeader
            {
                Query = "postcode",
                Offset = "0",
                TotalResults = "1",
                Format = "json",
                Dataset = "test",
                Lr = "en",
                MaxResults = "10",
                MatchingTotalResults = "1"
            };

            // Assert
            Assert.AreEqual("postcode", header.Query);
            Assert.AreEqual("0", header.Offset);
            Assert.AreEqual("1", header.TotalResults);
            Assert.AreEqual("json", header.Format);
            Assert.AreEqual("test", header.Dataset);
            Assert.AreEqual("en", header.Lr);
            Assert.AreEqual("10", header.MaxResults);
            Assert.AreEqual("1", header.MatchingTotalResults);
        }

        [TestMethod]
        public void DefaultValues_AreNull()
        {
            // Act
            var header = new AddressLookupResponseHeader();

            // Assert
            Assert.IsNull(header.Query);
            Assert.IsNull(header.Offset);
            Assert.IsNull(header.TotalResults);
            Assert.IsNull(header.Format);
            Assert.IsNull(header.Dataset);
            Assert.IsNull(header.Lr);
            Assert.IsNull(header.MaxResults);
            Assert.IsNull(header.MatchingTotalResults);
        }
    }
}
