using FrontendAccountManagement.Web.ViewModels.Shared.GovUK;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FrontendAccountManagement.Web.UnitTests.ViewModels
{
    [TestClass]
    public class ErrorsViewModelTests
    {
        [TestMethod]
        public void CreateViewModel_WithViewLocalizer_OrdersAndLocalizesErrors()
        {
            const string KeyOne = "keyOne";
            const string KeyTwo = "keyTwo";
            var errorsOne = new List<ErrorViewModel> { new ErrorViewModel { Key = "test1", Message = "test" } };
            var errorsTwo = new List<ErrorViewModel> { new ErrorViewModel { Key = "test2", Message = "test" } };
            var localizerMock = new Mock<IViewLocalizer>();
            localizerMock.Setup(x => x[It.IsAny<string>()]).Returns(new LocalizedHtmlString("test", "localized"));
            var errorsViewModel = new ErrorsViewModel(
                new List<(string Key, List<ErrorViewModel> Errors)> { (KeyOne, errorsOne), (KeyTwo, errorsTwo) },
                localizerMock.Object,
                KeyTwo, KeyOne);
            localizerMock.Verify(x => x[It.IsAny<string>()], Times.Exactly(2));
            Assert.AreEqual(2, errorsViewModel.Errors.Count);
            Assert.AreEqual(KeyTwo, errorsViewModel.Errors[0].Key);
            Assert.AreEqual(KeyOne, errorsViewModel.Errors[1].Key);
            foreach (var error in errorsViewModel.Errors.SelectMany(e => e.Errors))
            {
                Assert.AreEqual("localized", error.Message);
            }
        }

        [TestMethod]
        public void CreateViewModel_WithStringLocalizer_LocalizesErrors()
        {
            var errors = new List<(string Key, List<ErrorViewModel> Errors)>
            {
                ("key", new List<ErrorViewModel> { new ErrorViewModel { Key = "k", Message = "msg" } })
            };
            var localizerMock = new Mock<IStringLocalizer<SharedResources>>();
            localizerMock.Setup(x => x[It.IsAny<string>()]).Returns(new LocalizedString("msg", "localized"));
            var vm = new ErrorsViewModel(errors, localizerMock.Object);
            Assert.AreEqual("localized", vm.Errors[0].Errors[0].Message);
        }

        [TestMethod]
        public void Indexer_ReturnsNullForMissingKey()
        {
            var errors = new List<(string Key, List<ErrorViewModel> Errors)>
            {
                ("key", new List<ErrorViewModel>())
            };
            var localizerMock = new Mock<IStringLocalizer<SharedResources>>();
            localizerMock.Setup(x => x[It.IsAny<string>()]).Returns(new LocalizedString("msg", "localized"));
            var vm = new ErrorsViewModel(errors, localizerMock.Object);
            Assert.IsNull(vm["missing"]);
        }

        [TestMethod]
        public void Indexer_ReturnsErrorsForPresentKey()
        {
            var errorsList = new List<ErrorViewModel> { new ErrorViewModel { Key = "k", Message = "msg" } };
            var errors = new List<(string Key, List<ErrorViewModel> Errors)>
            {
                ("key", errorsList)
            };
            var localizerMock = new Mock<IStringLocalizer<SharedResources>>();
            localizerMock.Setup(x => x[It.IsAny<string>()]).Returns(new LocalizedString("msg", "localized"));
            var vm = new ErrorsViewModel(errors, localizerMock.Object);
            Assert.AreEqual(errorsList, vm["key"]);
        }

        [TestMethod]
        public void HasErrorKey_ReturnsTrueForPresentKey()
        {
            var errors = new List<(string Key, List<ErrorViewModel> Errors)>
            {
                ("key", new List<ErrorViewModel>())
            };
            var localizerMock = new Mock<IStringLocalizer<SharedResources>>();
            localizerMock.Setup(x => x[It.IsAny<string>()]).Returns(new LocalizedString("msg", "localized"));
            var vm = new ErrorsViewModel(errors, localizerMock.Object);
            Assert.IsTrue(vm.HasErrorKey("key"));
        }

        [TestMethod]
        public void HasErrorKey_ReturnsFalseForMissingKey()
        {
            var errors = new List<(string Key, List<ErrorViewModel> Errors)>
            {
                ("key", new List<ErrorViewModel>())
            };
            var localizerMock = new Mock<IStringLocalizer<SharedResources>>();
            localizerMock.Setup(x => x[It.IsAny<string>()]).Returns(new LocalizedString("msg", "localized"));
            var vm = new ErrorsViewModel(errors, localizerMock.Object);
            Assert.IsFalse(vm.HasErrorKey("missing"));
        }

        [TestMethod]
        public void TextInserts_CanBeSetAndRetrieved()
        {
            var errors = new List<(string Key, List<ErrorViewModel> Errors)>
            {
                ("key", new List<ErrorViewModel>())
            };
            var localizerMock = new Mock<IStringLocalizer<SharedResources>>();
            localizerMock.Setup(x => x[It.IsAny<string>()]).Returns(new LocalizedString("msg", "localized"));
            var vm = new ErrorsViewModel(errors, localizerMock.Object);
            var inserts = new List<(string Key, string Value)> { ("key", "value") };
            vm.TextInserts = inserts;
            Assert.AreEqual(inserts, vm.TextInserts);
        }
    }
}
