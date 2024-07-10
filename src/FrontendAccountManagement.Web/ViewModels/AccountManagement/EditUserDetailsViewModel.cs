﻿using FrontendAccountManagement.Web.Resources.Views.AccountManagement;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FrontendAccountManagement.Web.ViewModels.AccountManagement
{
    [ExcludeFromCodeCoverage]
    public class EditUserDetailsViewModel
    {
        [Required(ErrorMessageResourceName = "FirstNameMissing", ErrorMessageResourceType = typeof(EditUserDetails))]
        [StringLength(50, ErrorMessageResourceName = "FirstNameMaxLength", ErrorMessageResourceType = typeof(EditUserDetails))]
        public string FirstName { get; set; }

        [Required(ErrorMessageResourceName = "LastNameMissing", ErrorMessageResourceType = typeof(EditUserDetails))]
        [StringLength(50, ErrorMessageResourceName = "LastNameMaxLength", ErrorMessageResourceType = typeof(EditUserDetails))]
        public string LastName { get; set; }

        public string? OriginalJobTitle { get; set; }

        public string? OriginalTelephone { get; set; }

        [Required(ErrorMessageResourceName = "JobTitleMissing", ErrorMessageResourceType = typeof(EditUserDetails))]
        [StringLength(50, ErrorMessageResourceName = "JobTitleMaxLength", ErrorMessageResourceType = typeof(EditUserDetails))]
        public string? JobTitle { get; set; }

        [Required(ErrorMessageResourceName = "TelephoneNumberMissing", ErrorMessageResourceType = typeof(EditUserDetails))]
        [StringLength(50, ErrorMessageResourceName = "TelephoneMaxLength", ErrorMessageResourceType = typeof(EditUserDetails))]
        public string? Telephone { get; set; }

        public bool PropertyExists(Func<EditUserDetailsViewModel, string> propertySelector)
        {
            var propertyValue = propertySelector(this);
            return !string.IsNullOrWhiteSpace(propertyValue);
        }
    }
}