namespace ActiveDirectoryToRestApi.Extensions;

public static class LdapAttrExtensions
{
    public static string ConvertToDateTime(this string ldapTime)
    {
        if (ldapTime == "0" || ldapTime == "9223372036854775807") return "Never";
        var date = long.Parse(ldapTime);

        return DateTime.FromFileTime(date).ToString();
    }

    public static bool IsExpired(this string ldapTime)
    {
        if (ldapTime == "0" || ldapTime == "9223372036854775807") return false;
        var expiration = DateTime.FromFileTime(long.Parse(ldapTime));
        if (expiration < DateTime.Now) return true;

        return false;
    }

    public static bool IsPasswordExpired(this string userAccountControl)
    {
        if (userAccountControl == "8388608") return true;

        return false;
    }

    public static bool IsEnabled(this string userAccountControl)
    {
        if (userAccountControl == "512" || userAccountControl == "66048" || userAccountControl == "262656") return true;

        return false;
    }

    public static bool IsPasswordNeverExpire(this string userAccountControl)
    {
        if (userAccountControl == "66048" || userAccountControl == "66050") return true;

        return false;
    }

    public static string GroupType(this string groupType)
    {
        return groupType switch
        {
            "2" => "Global distribution group",
            "4" => "Global distribution group",
            "8" => "Universal distribution group",
            "-2147483646" => "Global security group",
            "-2147483644" => "Domain local security group",
            "-2147483640" => "Universal security group",
            _ => null,
        };
    }
}
