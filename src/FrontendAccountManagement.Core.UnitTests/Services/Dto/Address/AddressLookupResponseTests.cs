using FrontendAccountManagement.Core.Services.Dto.Address;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FrontendAccountManagement.Core.UnitTests.Services.Dto.Address
{
    [TestClass]
    public class AddressLookupResponseTests
    {
        [TestMethod]
        public void CanCreateAddressLookupResponse_WithHeaderAndResults()
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
            var address = new AddressLookupResponseAddress
            {
                AddressLine = "1 Test Street",
                Town = "TestTown",
                Postcode = "TT1 1TT",
                Country = "UK"
            };
            var result = new AddressLookupResponseResult { Address = address };
            var response = new AddressLookupResponse
            {
                Header = header,
                Results = new[] { result }
            };

            // Assert
            Assert.AreEqual(header, response.Header);
            Assert.AreEqual(1, response.Results.Length);
            Assert.AreEqual(address, response.Results[0].Address);
        }
    }
}
