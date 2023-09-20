namespace ActiveDirectoryToRestApi.Models;

public class UserLogin
{
    [Required]
    [DefaultValue("testUser")]
    public string UserName { get; set; }

    [Required]
    [DefaultValue("Thi$1sMyPassw0rd")]
    public string Password { get; set; }
}
