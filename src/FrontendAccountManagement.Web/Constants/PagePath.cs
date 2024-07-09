namespace FrontendAccountManagement.Web.Constants;

public static class PagePath
{
    // Journey paths
    public const string ManageAccount = "manage";
    public const string TeamMemberEmail = "team-member-email";
    public const string TeamMemberPermissions = "team-member-permissions";
    public const string TeamMemberDetails = "team-member-details";
    public const string RemoveTeamMember = "remove-team-member";
    public const string PreRemoveTeamMember = "pre-remove-team-member";
    public const string CheckYourDetails = "check-your-details";

    // Change role journey paths
    public const string ChangeAccountPermissions = "change-account-permissions";
    public const string RelationshipWithOrganisation = "relationship-with-organisation";
    public const string ConfirmChangePermission = "confirm-change-permission";
    public const string JobTitle = "job-title";
    public const string NameOfConsultancy = "name-of-consultancy";
    public const string NameOfComplianceScheme = "name-of-compliance-scheme";
    public const string NameOfOrganisation = "name-of-organisation";
    public const string CheckDetailsSendInvite = "check-details-send-invite";
    public const string InvitationToChangeSent = "invitation-to-change-sent";

    // Non journey paths
    public const string Accessibility = "accessibility";
    public const string SignedOut = "signed-out";
    public const string Error = "error";
    public const string Culture = "culture";
    public const string AcknowledgeCookieAcceptance = "acknowledge-cookie-acceptance";
    public const string UpdateCookieAcceptance = "update-cookie-acceptance";
}
