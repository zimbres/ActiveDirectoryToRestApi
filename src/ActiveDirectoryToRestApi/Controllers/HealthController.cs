namespace ActiveDirectoryToRestApi.Controllers;

[ApiController]
[Route("/")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;
    private readonly LdapService _ldapService;

    public HealthController(ILogger<HealthController> logger, LdapService ldapService)
    {
        _logger = logger;
        _ldapService = ldapService;
    }

    [HttpGet]
    public ActionResult<Health> Get()
    {
        var connection = _ldapService.CheckLdapConnection();
        if (connection) return Ok(new Health());
        return StatusCode(500, new Health { Data = new HealthData { Health = false } });
    }
}
