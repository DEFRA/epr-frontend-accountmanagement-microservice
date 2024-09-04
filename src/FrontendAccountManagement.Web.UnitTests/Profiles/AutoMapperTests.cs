using AutoMapper;
using FrontendAccountManagement.Web.Profiles;

namespace FrontendAccountManagement.Web.UnitTests.Profiles;

[TestClass]
public class AutoMapperTests
{
    private IMapper _mapper;

    [TestMethod]
    public void CompaniesHouseProfileValidValid()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CompaniesHouseResponseProfile>();
        });

        _mapper = config.CreateMapper();
        _mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }

    [TestMethod]
    public void AccountManagementProfileValidValid()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AccountManagementProfile>();
        });

        _mapper = config.CreateMapper();
        _mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }
}