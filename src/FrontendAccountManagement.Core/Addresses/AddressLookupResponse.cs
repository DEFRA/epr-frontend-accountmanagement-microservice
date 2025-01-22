namespace FrontendAccountManagement.Core.Addresses
{
    public class AddressLookupResponse
    {
        public AddressLookupResponseHeader Header { get; set; } = default!;

        public AddressLookupResponseResult[] Results { get; set; } = default!;
    }
}
