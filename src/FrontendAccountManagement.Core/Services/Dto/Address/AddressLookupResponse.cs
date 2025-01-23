namespace FrontendAccountManagement.Core.Services.Dto.Address
{
    public class AddressLookupResponse
    {
        public AddressLookupResponseHeader Header { get; set; } = default!;

        public AddressLookupResponseResult[] Results { get; set; } = default!;
    }
}
