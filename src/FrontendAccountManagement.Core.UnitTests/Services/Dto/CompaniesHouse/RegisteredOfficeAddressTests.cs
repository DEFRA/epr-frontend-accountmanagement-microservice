using FrontendAccountManagement.Core.Services.Dto.CompaniesHouse;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FrontendAccountManagement.Core.UnitTests.Services.Dto.CompaniesHouse
{
    [TestClass]
    public class RegisteredOfficeAddressTests
    {
        [TestMethod]
        public void CanSetAndGetAllProperties()
        {
            // Arrange
            var country = new Country { Name = "UK" };
            var address = new RegisteredOfficeAddress
            {
                SubBuildingName = "Flat 2",
                BuildingName = "Test Building",
                BuildingNumber = "1",
                Street = "Test Street",
                Town = "Test Town",
                County = "Test County",
                Postcode = "TT1 1TT",
                Locality = "Test Locality",
                DependentLocality = "Dependent Locality",
                Country = country,
                IsUkAddress = true
            };

            // Assert
            Assert.AreEqual("Flat 2", address.SubBuildingName);
            Assert.AreEqual("Test Building", address.BuildingName);
            Assert.AreEqual("1", address.BuildingNumber);
            Assert.AreEqual("Test Street", address.Street);
            Assert.AreEqual("Test Town", address.Town);
            Assert.AreEqual("Test County", address.County);
            Assert.AreEqual("TT1 1TT", address.Postcode);
            Assert.AreEqual("Test Locality", address.Locality);
            Assert.AreEqual("Dependent Locality", address.DependentLocality);
            Assert.AreEqual(country, address.Country);
            Assert.IsTrue(address.IsUkAddress.Value);
        }

        [TestMethod]
        public void DefaultValues_AreNull()
        {
            // Act
            var address = new RegisteredOfficeAddress();

            // Assert
            Assert.IsNull(address.SubBuildingName);
            Assert.IsNull(address.BuildingName);
            Assert.IsNull(address.BuildingNumber);
            Assert.IsNull(address.Street);
            Assert.IsNull(address.Town);
            Assert.IsNull(address.County);
            Assert.IsNull(address.Postcode);
            Assert.IsNull(address.Locality);
            Assert.IsNull(address.DependentLocality);
            Assert.IsNull(address.Country);
            Assert.IsNull(address.IsUkAddress);
        }
    }
}
