using FrontendAccountManagement.Core.Services.Dto.Address;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FrontendAccountManagement.Core.UnitTests.Services.Dto.Address
{
    [TestClass]
    public class AddressLookupResponseResultTests
    {
        [TestMethod]
        public void CanSetAndGetAddressProperty()
        {
            // Arrange
            var address = new AddressLookupResponseAddress
            {
                AddressLine = "1 Test Street",
                Town = "TestTown",
                Postcode = "TT1 1TT",
                Country = "UK"
            };
            var result = new AddressLookupResponseResult
            {
                Address = address
            };

            // Assert
            Assert.AreEqual(address, result.Address);
            Assert.AreEqual("1 Test Street", result.Address.AddressLine);
            Assert.AreEqual("TestTown", result.Address.Town);
            Assert.AreEqual("TT1 1TT", result.Address.Postcode);
            Assert.AreEqual("UK", result.Address.Country);
        }

        [TestMethod]
        public void DefaultValue_Address_IsNull()
        {
            // Act
            var result = new AddressLookupResponseResult();

            // Assert
            Assert.IsNull(result.Address);
        }
    }
}
