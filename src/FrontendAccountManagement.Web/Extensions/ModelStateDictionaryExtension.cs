﻿using FrontendAccountManagement.Web.ViewModels.Shared.GovUK;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FrontendAccountManagement.Web.Extensions;

public static class ModelStateDictionaryExtension
{
    public static List<(string Key, List<ErrorViewModel> Errors)> ToErrorDictionary(this ModelStateDictionary modelState)
    {
        var errors = new List<ErrorViewModel>();

        foreach (var item in modelState)
        {
            foreach (var error in item.Value.Errors)
            {
                errors.Add(new ErrorViewModel
                {
                    Key = item.Key,
                    Message = error.ErrorMessage
                });
            }
        }

        var errorsDictionary = new List<(string Key, List<ErrorViewModel> Errors)>();

        var groupedErrors = errors.GroupBy(e => e.Key);
        
        foreach (var error in groupedErrors)
        {
            errorsDictionary.Add((error.Key, error.ToList()));
        }
        
        return errorsDictionary;
    }
}