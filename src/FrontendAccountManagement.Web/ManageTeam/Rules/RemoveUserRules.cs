using FrontendAccountManagement.Core.Models;

namespace FrontendAccountManagement.Web.ManageTeam.Rules;

public class RemoveUserRules
{
    private string[] _userOptions;
    private readonly string _serviceRoleKey;
    private readonly List<ManageUserModel> _users;
    
    public RemoveUserRules(string serviceRoleKey, List<ManageUserModel> users)
    {
        _serviceRoleKey = serviceRoleKey;
        _users = users;
        
        SetUserOptions();
    }
    
    public List<ManageUserModel> SetRemovableUsers()
    {
        //check if user in context has a service role key matching one of the admin users granted service role keys,
        //then check that first/last name are populated to ensure they are a non-invited user
        foreach (var user in _users.Where(user => _userOptions.Contains(user.ServiceRoleKey) 
                                                  && !string.IsNullOrEmpty(user.FirstName) 
                                                  && !string.IsNullOrEmpty(user.LastName)))
        {
            user.IsRemoveable = true;
        }

        return _users;
    }
    
    private void SetUserOptions()
    {
        _userOptions = _serviceRoleKey switch
        {
            "Approved.Admin" => new[] { "Delegated.Admin", "Basic.Admin", "Basic.Employee" },
            "Delegated.Admin" => new[] { "Basic.Admin", "Basic.Employee" },
            "Basic.Admin" => new[] { "Basic.Admin", "Basic.Employee" },
            "RegulatorAdmin.Admin" => new []{ "Regulator.Admin", "Regulator.Basic" },
            _ => Array.Empty<string>()
        };
    }
}