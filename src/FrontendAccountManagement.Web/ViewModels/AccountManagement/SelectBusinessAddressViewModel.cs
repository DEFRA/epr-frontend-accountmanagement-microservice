using FrontendAccountManagement.Core.Addresses;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FrontendAccountManagement.Web.ViewModels.AccountManagement
{
    public class SelectBusinessAddressViewModel
    {
        public string? Postcode { get; set; }

        [Required(ErrorMessage = "SelectBusinessAddress.ErrorMessage")]
        public string? SelectedListIndex { get; set; }

        public void SetAddressItems(IList<Address> addresses, string? selectedItem)
        {
            for (var index = 0; index < addresses.Count; index++)
            {
                var value = index.ToString();
                AddressItems.Add(new SelectListItem { Value = value, Text = addresses[index].AddressSingleLine, Selected = value == selectedItem });
            }
        }

        public ICollection<SelectListItem> AddressItems { get; set; } = new List<SelectListItem>();
    }
}