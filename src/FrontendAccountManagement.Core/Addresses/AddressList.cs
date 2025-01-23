using FrontendAccountManagement.Core.Services.Dto.Address;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrontendAccountManagement.Core.Addresses
{
    public class AddressList
    {
        public AddressList()
        {
        }

        public AddressList(AddressLookupResponse? addressLookupResponse) : this()
        {
            if (addressLookupResponse == null)
            {
                throw new ArgumentException("addressLookupResponse cannot be null.");
            }

            Addresses = addressLookupResponse.Results.Select(item => new Address()
            {
                AddressSingleLine = item.Address.AddressLine,
                SubBuildingName = item.Address.SubBuildingName,
                BuildingName = item.Address.BuildingName,
                BuildingNumber = item.Address.BuildingNumber,
                Street = item.Address.Street,
                Town = item.Address.Town,
                County = item.Address.County,
                Postcode = item.Address.Postcode,
                IsManualAddress = false
            }).ToList();
        }

        public IList<Address>? Addresses { get; set; }
    }
}
