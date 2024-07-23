namespace ActiveDirectoryToRestApi.Authentication;

public class ApiKeyAuthFilter : IAuthorizationFilter
{
    private readonly Configuration _configuration;

    public ApiKeyAuthFilter(IConfiguration configuration)
    {
        _configuration = configuration.GetSection(nameof(Configuration)).Get<Configuration>();
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (_configuration.AuthEnabled)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue("Authorization", out var extractedAuthToken))
            {
                context.Result = new UnauthorizedObjectResult("Authorization token was not provided");
                return;
            }

            var authToken = _configuration.Authorization;

            if (!authToken.Equals(extractedAuthToken))
            {
                context.Result = new UnauthorizedObjectResult("Unauthorized client");
                return;
            }
        }
    }
}