namespace FrontendAccountManagement.Web.UnitTests.ViewComponents;

using FrontendAccountManagement.Web.Configs;
using FrontendAccountManagement.Web.ViewComponents;
using FrontendAccountManagement.Web.ViewModels.Shared;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class PhaseBannerTests
{
    [TestMethod]
    public void Invoke_SetsModel()
    {
        // Arrange
        var phaseBannerOptions = new PhaseBannerOptions
        {
            ApplicationStatus = "Beta", SurveyUrl = "testUrl", Enabled = true
        };
        var options = Options.Create(phaseBannerOptions);
        var component = new PhaseBannerViewComponent(options);

        // Act
        var model = component.Invoke().ViewData.Model as PhaseBannerModel;

        // Assert
        Assert.AreEqual($"PhaseBanner.{phaseBannerOptions.ApplicationStatus}", model.Status);
        Assert.AreEqual(phaseBannerOptions.SurveyUrl, model.Url);
        Assert.AreEqual(phaseBannerOptions.Enabled, model.ShowBanner);
    }
}