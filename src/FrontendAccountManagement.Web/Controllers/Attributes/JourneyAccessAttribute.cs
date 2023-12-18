using FrontendAccountManagement.Web.Constants;

namespace FrontendAccountManagement.Web.Controllers.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class JourneyAccessAttribute : Attribute
{
    public JourneyAccessAttribute(string pagePath, string journeyType = JourneyName.ManageAccount)
    {
        PagePath = pagePath;
        JourneyType = journeyType;
    }

    public string PagePath { get; }
    public string JourneyType { get; }
}
