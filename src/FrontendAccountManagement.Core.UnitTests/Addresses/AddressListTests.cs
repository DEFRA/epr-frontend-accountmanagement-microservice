using System;
using System.Collections.Generic;
using System.Linq;
using FrontendAccountManagement.Core.Addresses;
using FrontendAccountManagement.Core.Services.Dto.Address;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FrontendAccountManagement.Core.UnitTests.Addresses
{
    [TestClass]
    public class AddressListTests
    {
        [TestMethod]
        public void DefaultConstructor_AddressesIsNull()
        {
            // Act
            var addressList = new AddressList();

            // Assert
            Assert.IsNull(addressList.Addresses);
        }

        [TestMethod]
        public void Constructor_WithValidAddressLookupResponse_MapsAddressesCorrectly()
        {
            // Arrange
            var response = new AddressLookupResponse
            {
                Results = new[]
                {
                    new AddressLookupResponseResult
                    {
                        Address = new AddressLookupResponseAddress
                        {
                            AddressLine = "1 Test Street",
                            SubBuildingName = "Flat 2",
                            BuildingName = "Test Building",
                            BuildingNumber = "1",
                            Street = "Test Street",
                            Town = "Test Town",
                            County = "Test County",
                            Postcode = "TT1 1TT",
                            Locality = "Test Locality",
                            DependentLocality = "Dependent Locality"
                        }
                    }
                }
            };

            // Act
            var addressList = new AddressList(response);

            // Assert
            Assert.IsNotNull(addressList.Addresses);
            Assert.AreEqual(1, addressList.Addresses.Count);
            var address = addressList.Addresses.First();
            Assert.AreEqual("1 Test Street", address.AddressSingleLine);
            Assert.AreEqual("Flat 2", address.SubBuildingName);
            Assert.AreEqual("Test Building", address.BuildingName);
            Assert.AreEqual("1", address.BuildingNumber);
            Assert.AreEqual("Test Street", address.Street);
            Assert.AreEqual("Test Town", address.Town);
            Assert.AreEqual("Test County", address.County);
            Assert.AreEqual("TT1 1TT", address.Postcode);
            Assert.AreEqual("Test Locality", address.Locality);
            Assert.AreEqual("Dependent Locality", address.DependentLocality);
            Assert.IsFalse(address.IsManualAddress);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Constructor_WithNullAddressLookupResponse_ThrowsArgumentException()
        {
            // Act
            var addressList = new AddressList(null);
        }
    }
}
