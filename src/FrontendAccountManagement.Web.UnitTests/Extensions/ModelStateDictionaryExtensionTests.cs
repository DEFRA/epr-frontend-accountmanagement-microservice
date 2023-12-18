using FrontendAccountManagement.Web.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
namespace FrontendAccountManagement.Web.UnitTests.Extensions
{
    [TestClass]
    public class ModelStateDictionaryExtensionTests
    {
        [TestMethod]
        public void ToErrorDictionary_ConvertsModelStateToKeyValueErrorList()
        {
            var modelState = new ModelStateDictionary();
            modelState.AddModelError("Name", "Name is required"); 
            modelState.AddModelError("Email", "Email is invalid");
            modelState.AddModelError("Email", "Email already exists");

            // Act
            var result = ModelStateDictionaryExtension.ToErrorDictionary(modelState);

            // Assert
            result.Should().HaveCount(2);

            result[0].Key.Equals("Name");
            result[0].Errors.Should().HaveCount(1);
            result[0].Errors[0].Message.Equals("Name is required");

            result[1].Key.Equals("Email");
            result[1].Errors.Should().HaveCount(2);
            result[1].Errors[0].Message.Equals("Email is invalid");
            result[1].Errors[0].Message.Equals("Email already exists");
        }
    }
}
