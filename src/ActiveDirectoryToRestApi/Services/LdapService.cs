namespace ActiveDirectoryToRestApi.Services;

public class LdapService
{
    private readonly ILogger<LdapService> _logger;
    private readonly LdapConfigs _ldapConfigs;
    private readonly LdapConnection _connection;
    private readonly IConfiguration _configuration;

    public LdapService(ILogger<LdapService> logger, LdapConnection connection, IConfiguration configuration)
    {
        _logger = logger;
        _connection = connection;
        _configuration = configuration;
        _ldapConfigs = _configuration.GetSection(nameof(LdapConfigs)).Get<LdapConfigs>();
        _logger = logger;
    }
    public User GetUserByUserName(string username)
    {
        string filter = $"(&(objectClass=user)(objectCategory=person)(sAMAccountName={username}))";
        return GetUser(filter);
    }

    public User GetUserByEmail(string email)
    {
        string filter = $"(&(objectClass=user)(objectCategory=person)(mail={email}))";
        return GetUser(filter);
    }

    private User GetUser(string filter)
    {
        string[] attributesToReturn = { "givenName", "sn", "mail", "userPrincipalName", "displayName", "sAMAccountName",
                                        "userAccountControl", "accountExpires", "memberOf", "distinguishedName", "pwdLastSet", "lockoutTime" };

        SearchRequest searchRequest = new(_ldapConfigs.DistinguishedName, filter, SearchScope.Subtree, attributesToReturn);

        SearchResponse searchResponse = (SearchResponse)_connection.SendRequest(searchRequest);

        if (searchResponse.Entries.Count > 0)
        {
            SearchResultEntry entry = searchResponse.Entries[0];

            bool isLocked = false;
            string lockoutTime = string.Empty;
            if (entry.Attributes.Contains("lockoutTime"))
            {
                var lockoutTimeTicks = entry.Attributes["lockoutTime"][0].ToString();
                isLocked = lockoutTimeTicks != "0";
                lockoutTime = lockoutTimeTicks.ConvertToDateTime();
            }

            string userAccountControl = entry.Attributes["userAccountControl"][0].ToString();
            string accountExpires = entry.Attributes["accountExpires"][0].ToString();
            string expiration = accountExpires.ConvertToDateTime();
            string pwdLastSet = entry.Attributes["pwdLastSet"][0].ToString().ConvertToDateTime();
            string passwordExpiration = GetPasswordExpiration(pwdLastSet);
            bool expired = accountExpires.IsExpired();
            string mail = entry.Attributes["mail"]?[0].ToString() ?? "";
            string givenName = entry.Attributes["givenName"]?[0].ToString() ?? "";
            string sn = entry.Attributes["sn"]?[0].ToString() ?? "";
            string displayName = entry.Attributes["displayName"]?[0].ToString() ?? "";
            string sAMAccountName = entry.Attributes["sAMAccountName"][0].ToString();
            string userPrincipalName = entry.Attributes["userPrincipalName"]?[0].ToString() ?? "";
            string distinguishedName = entry.DistinguishedName;
            bool enabled = userAccountControl.IsEnabled();
            bool passwordNeverExpire = userAccountControl.IsPasswordNeverExpire();
            bool passwordExpired = userAccountControl.IsPasswordExpired();

            var groups = new List<string>();
            var memberOfAttribute = entry.Attributes["memberOf"];
            if (memberOfAttribute != null)
            {
                foreach (var groupDn in memberOfAttribute.GetValues(typeof(string)))
                {
                    groups.Add(ParseGroupName(groupDn.ToString()));
                }
            }

            var user = new User
            {
                DisplayName = displayName,
                Email = mail,
                SamAccountName = sAMAccountName,
                Enabled = enabled,
                LogonName = userPrincipalName,
                FirstName = givenName,
                LastName = sn,
                PasswordNeverExpire = passwordNeverExpire,
                PasswordExpired = passwordExpired,
                PasswordLastSet = pwdLastSet,
                PasswordExpiration = passwordExpiration,
                Expiration = expiration,
                Expired = expired,
                IsLocked = isLocked,
                LockoutTime = lockoutTime,
                DistinguishedName = distinguishedName,
                Groups = groups
            };
            return user;
        }
        else
        {
            return null;
        }
    }

    private static string ParseGroupName(string groupDn)
    {
        int startIndex = groupDn.IndexOf('=') + 1;
        int endIndex = groupDn.IndexOf(',');

        if (startIndex > 0 && endIndex > 0)
        {
            return groupDn[startIndex..endIndex];
        }
        else
        {
            return groupDn;
        }
    }

    public Group GetGroupMembers(string groupName)
    {
        Group group = GetGroup(groupName);

        if (group is null) return null;

        string filter = $"(&(objectCategory=user)(memberOf={group.DistinguishedName}))";
        string[] attributesToReturn = { "sAMAccountName" };

        SearchRequest searchRequest = new(_ldapConfigs.DistinguishedName, filter, SearchScope.Subtree, attributesToReturn);
        SearchResponse searchResponse = (SearchResponse)_connection.SendRequest(searchRequest);

        foreach (SearchResultEntry entry in searchResponse.Entries)
        {
            group.Members.Add(entry.Attributes["sAMAccountName"][0].ToString());
        }
        return group;
    }

    private Group GetGroup(string groupName)
    {
        string filter = $"(&(objectClass=group)(cn={groupName}))";
        string[] attributesToReturn = { "distinguishedName", "cn", "groupType" };

        SearchRequest searchRequest = new(_ldapConfigs.DistinguishedName, filter, SearchScope.Subtree, attributesToReturn);
        SearchResponse searchResponse = (SearchResponse)_connection.SendRequest(searchRequest);

        if (searchResponse.Entries.Count > 0)
        {
            SearchResultEntry entry = searchResponse.Entries[0];
            Group group = new()
            {
                Name = entry.Attributes["cn"][0].ToString(),
                Type = entry.Attributes["groupType"][0].ToString().GroupType(),
                DistinguishedName = searchResponse.Entries[0].DistinguishedName
            };
            return group;
        }
        return null;
    }

    private string GetPasswordExpiration(string pwdLastSet)
    {
        var pwdLastSetDate = DateTime.Parse(pwdLastSet);
        var passwordExpiration = pwdLastSetDate.AddDays(GetMaxPwdAge());
        return passwordExpiration.ToString();
    }

    private double GetMaxPwdAge()
    {
        string filter = "(objectClass=domain)";
        string[] attributesToRetrieve = { "maxPwdAge" };

        SearchRequest searchRequest = new(_ldapConfigs.DistinguishedName, filter, SearchScope.Subtree, attributesToRetrieve);
        SearchResponse searchResponse = (SearchResponse)_connection.SendRequest(searchRequest);

        if (searchResponse.Entries.Count == 1)
        {
            SearchResultEntry entry = searchResponse.Entries[0];
            long value = long.Parse(entry.Attributes["maxPwdAge"][0].ToString());
            TimeSpan maxPwdAge = TimeSpan.FromTicks(value);
            double days = maxPwdAge.TotalDays;
            return Math.Abs(days);
        }
        else
        {
            return 42;
        }
    }

    public bool CheckLdapConnection()
    {
        try
        {
            _connection.Bind();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("LDAP connection failed: {ex.StackTrace}", ex.StackTrace);
            return false;
        }
    }
}
