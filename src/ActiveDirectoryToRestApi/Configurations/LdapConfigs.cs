namespace ActiveDirectoryToRestApi.Configurations;

public class LdapConfigs
{
    public string Server { get; set; }
    public int Port { get; set; }
    public bool UseSSL { get; set; }
    public string DomainName { get; set; }
    public string User { get; set; }
    public string Pass { get; set; }
    public string DistinguishedName { get; set; }
}
