namespace ActiveDirectoryToRestApi.Controllers;

[ApiController]
[Route("[controller]")]
[ServiceFilter(typeof(ApiKeyAuthFilter))]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly LdapService _ldapService;
    private readonly AuthUserService _authUserService;

    public UserController(ILogger<UserController> logger, LdapService ldapService, AuthUserService authUserService)
    {
        _logger = logger;
        _ldapService = ldapService;
        _authUserService = authUserService;
    }

    [HttpPost("/email")]
    public ActionResult<User> Email(string email = "test.user@zimbres.loc")
    {
        var user = _ldapService.GetUserByEmail(email);
        if (user == null)
        {
            return NotFound();
        }
        return user;
    }

    [HttpPost("/username")]
    public ActionResult<User> Username(string username = "testUser")
    {
        var user = _ldapService.GetUserByUserName(username);
        if (user == null)
        {
            return NotFound();
        }
        return user;
    }

    [HttpPost("/authUser")]
    public IActionResult AuthUser(UserLogin userLogin)
    {
        var status = _authUserService.AuthUser(userLogin.UserName, userLogin.Password);
        if (status)
        {
            return Ok();
        }
        return Unauthorized();
    }
}
