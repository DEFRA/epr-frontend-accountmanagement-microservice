using FrontendAccountManagement.Web.ViewModels.Shared.GovUK;
using Microsoft.AspNetCore.Mvc.Localization;
using Moq;

namespace FrontendAccountManagement.Web.UnitTests.ViewModels
{
    [TestClass]
    public class ErrorsViewModelTests
    {
        [TestMethod]
        public void CreateViewModel()
        {
            // Arrange

            const string KeyOne = "keyOne";
            const string KeyTwo = "keyTwo";

            var errorsOne = new List<ErrorViewModel> { new ErrorViewModel { Key = "test1", Message = "test1" } };
            var errorsTwo = new List<ErrorViewModel> { new ErrorViewModel { Key = "test2", Message = "test2" } };

            var localizerMock = new Mock<IViewLocalizer>();

            localizerMock.Setup(x => x[It.IsAny<string>()]).Returns(new LocalizedHtmlString("test", "test2"));

            // Act

            var errorsViewModel = new ErrorsViewModel(
                new List<(string Key, List<ErrorViewModel> Errors)> { (KeyOne, errorsOne), (KeyTwo, errorsTwo) },
                localizerMock.Object,
                KeyTwo,
                KeyOne);

            // Assert

            localizerMock.Verify(x => x[It.IsAny<string>()], Times.Exactly(2));

            Assert.AreEqual(2, errorsViewModel.Errors.Count);
            Assert.AreEqual(KeyTwo, errorsViewModel.Errors[0].Key);
            Assert.AreEqual(KeyOne, errorsViewModel.Errors[1].Key);

            Assert.IsTrue(errorsViewModel.HasErrorKey(KeyOne));
            Assert.IsTrue(errorsViewModel.HasErrorKey(KeyOne));
            Assert.IsFalse(errorsViewModel.HasErrorKey("keyThree"));

            Assert.AreEqual(errorsOne, errorsViewModel[KeyOne]);
            Assert.AreEqual(errorsTwo, errorsViewModel[KeyTwo]);
            Assert.IsNull(errorsViewModel["keyThree"]);
        }
    }
}
