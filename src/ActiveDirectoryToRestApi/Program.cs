var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton(provider =>
{
    var configs = builder.Configuration.GetSection(nameof(LdapConfigs)).Get<LdapConfigs>();
    var endpoint = new LdapDirectoryIdentifier(configs.Server, false, false);
    var ldapConnection = new LdapConnection(endpoint,
    new NetworkCredential($"{configs.User}@{configs.DomainName}", configs.Pass))
    {
        AuthType = AuthType.Basic
    };
    ldapConnection.SessionOptions.ReferralChasing = ReferralChasingOptions.None;
    ldapConnection.Bind();
    return ldapConnection;
});
builder.Services.AddSingleton<LdapService>();
builder.Services.AddTransient<AuthUserService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();

app.Run();
