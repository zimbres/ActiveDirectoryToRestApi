using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(s =>
{
    s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter the token with the `Bearer` prefix, e.g. \"Bearer a1b2c3d4\"",
        Type = SecuritySchemeType.ApiKey,
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = "ApiKeyScheme",
    });
    var scheme = new OpenApiSecurityScheme
    {
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        },
        In = ParameterLocation.Header
    };
    var requirement = new OpenApiSecurityRequirement
    {
        { scheme, new List<string>() }
    };
    s.AddSecurityRequirement(requirement);
});
builder.Services.AddSingleton(provider =>
{
    var configs = builder.Configuration.GetSection(nameof(LdapConfigs)).Get<LdapConfigs>();
    var endpoint = new LdapDirectoryIdentifier(configs.Server, configs.Port, configs.UseSSL ? true : false, false);
    var ldapConnection = new LdapConnection(endpoint,
    new NetworkCredential($"{configs.User}@{configs.DomainName}", configs.Pass))
    {
        AuthType = AuthType.Basic
    };
    if (configs.UseSSL && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        ldapConnection.SessionOptions.SecureSocketLayer = true;
        ldapConnection.SessionOptions.VerifyServerCertificate = (con, cert) => true;
    }
    if (configs.UseSSL && RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
        ldapConnection.SessionOptions.SecureSocketLayer = true;
    }
    ldapConnection.SessionOptions.ReferralChasing = ReferralChasingOptions.None;
    ldapConnection.Bind();
    return ldapConnection;
});
builder.Services.AddSingleton<LdapService>();
builder.Services.AddTransient<AuthUserService>();
builder.Services.AddScoped<ApiKeyAuthFilter>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();
