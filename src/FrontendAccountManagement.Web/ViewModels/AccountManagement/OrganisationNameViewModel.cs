using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Web.ViewModels.AccountManagement
{
    
    public class OrganisationNameViewModel
    {
        [MaxLength(170, ErrorMessage = "CompanyName.NameLengthErrorMessage")]
        [Required(ErrorMessage = "CompanyName.NameErrorMessage")]
        public string OrganisationName { get; set; }
       
    }
}