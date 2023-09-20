namespace ActiveDirectoryToRestApi.Services;

public class AuthUserService
{
    private readonly IConfiguration _configuration;

    public AuthUserService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool AuthUser(string username, string password)
    {
        var configs = _configuration.GetSection(nameof(LdapConfigs)).Get<LdapConfigs>();
        try
        {
            var endpoint = new LdapDirectoryIdentifier(configs.Server, false, false);
            using var connection = new LdapConnection(endpoint,
            new NetworkCredential($"{configs.DomainName}\\{username}", password))
            {
                AuthType = AuthType.Basic
            };
            connection.SessionOptions.ReferralChasing = ReferralChasingOptions.None;
            connection.Bind();
            return true;
        }
        catch (LdapException)
        {
            return false;
        }
    }
}
