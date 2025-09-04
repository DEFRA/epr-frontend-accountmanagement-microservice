using FrontendAccountManagement.Core.Services.Dto.Address;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FrontendAccountManagement.Core.UnitTests.Services.Dto.Address
{
    [TestClass]
    public class AddressLookupResponseAddressTests
    {
        [TestMethod]
        public void CanSetAndGetAllProperties()
        {
            // Arrange
            var address = new AddressLookupResponseAddress
            {
                AddressLine = "1 Test Street",
                SubBuildingName = "Flat 2",
                BuildingName = "Test Building",
                BuildingNumber = "1",
                Street = "Test Street",
                Locality = "Test Locality",
                DependentLocality = "Dependent Locality",
                Town = "Test Town",
                County = "Test County",
                Postcode = "TT1 1TT",
                Country = "UK",
                XCoordinate = 123,
                YCoordinate = 456,
                UPRN = "1234567890",
                Match = "Exact",
                MatchDescription = "Exact match found",
                Language = "en"
            };

            // Assert
            Assert.AreEqual("1 Test Street", address.AddressLine);
            Assert.AreEqual("Flat 2", address.SubBuildingName);
            Assert.AreEqual("Test Building", address.BuildingName);
            Assert.AreEqual("1", address.BuildingNumber);
            Assert.AreEqual("Test Street", address.Street);
            Assert.AreEqual("Test Locality", address.Locality);
            Assert.AreEqual("Dependent Locality", address.DependentLocality);
            Assert.AreEqual("Test Town", address.Town);
            Assert.AreEqual("Test County", address.County);
            Assert.AreEqual("TT1 1TT", address.Postcode);
            Assert.AreEqual("UK", address.Country);
            Assert.AreEqual(123, address.XCoordinate);
            Assert.AreEqual(456, address.YCoordinate);
            Assert.AreEqual("1234567890", address.UPRN);
            Assert.AreEqual("Exact", address.Match);
            Assert.AreEqual("Exact match found", address.MatchDescription);
            Assert.AreEqual("en", address.Language);
        }

        [TestMethod]
        public void DefaultValues_AreNull()
        {
            // Act
            var address = new AddressLookupResponseAddress();

            // Assert
            Assert.IsNull(address.AddressLine);
            Assert.IsNull(address.SubBuildingName);
            Assert.IsNull(address.BuildingName);
            Assert.IsNull(address.BuildingNumber);
            Assert.IsNull(address.Street);
            Assert.IsNull(address.Locality);
            Assert.IsNull(address.DependentLocality);
            Assert.IsNull(address.Town);
            Assert.IsNull(address.County);
            Assert.IsNull(address.Postcode);
            Assert.IsNull(address.Country);
            Assert.IsNull(address.XCoordinate);
            Assert.IsNull(address.YCoordinate);
            Assert.IsNull(address.UPRN);
            Assert.IsNull(address.Match);
            Assert.IsNull(address.MatchDescription);
            Assert.IsNull(address.Language);
        }
    }
}
