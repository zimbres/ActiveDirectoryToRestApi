namespace ActiveDirectoryToRestApi.Controllers;

[ApiController]
[Route("[controller]")]
public class GroupController : ControllerBase
{
    private readonly ILogger<GroupController> _logger;
    private readonly LdapService _ldapService;

    public GroupController(ILogger<GroupController> logger, LdapService ldapService)
    {
        _logger = logger;
        _ldapService = ldapService;
    }

    [HttpPost("/group")]
    public ActionResult<Group> Group(string group = "mygroup")
    {
        var members = _ldapService.GetGroupMembers(group);
        if (members == null)
        {
            return NotFound();
        }
        return members;
    }
}
