namespace ActiveDirectoryToRestApi.Models;

public class User
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string DisplayName { get; set; }
    public string Email { get; set; }
    public string LogonName { get; set; }
    public string SamAccountName { get; set; }
    public bool Enabled { get; set; }
    public bool PasswordNeverExpire { get; set; }
    public bool PasswordExpired { get; set; }
    public string PasswordLastSet { get; set; }
    public string PasswordExpiration { get; set; }
    public bool Expired { get; set; }
    public string Expiration { get; set; }
    public string DistinguishedName { get; set; }
    public List<string> Groups { get; set; }
}
