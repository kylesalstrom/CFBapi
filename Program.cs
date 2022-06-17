using Microsoft.AspNetCore.HostFiltering;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Core;
using CFBSharp.Client;

var builder = WebApplication.CreateBuilder(args);

//Enable Cors for use in Dev
string AllowAnyOrigin = "AllowAnyOrigin";
builder.Services.AddCors(options => options.AddPolicy(AllowAnyOrigin, policy => policy.AllowAnyOrigin()));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddMemoryCache();

// Set up host filtering
var hosts = builder.Configuration["AllowedHosts"]?.Split(new[] { ';'}, StringSplitOptions.RemoveEmptyEntries);
if(hosts?.Length > 0)
{
    builder.Services.Configure<HostFilteringOptions>(options => options.AllowedHosts = hosts);
}

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

string cfbdAPIkey;
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    cfbdAPIkey = builder.Configuration["CFBD_API_KEY"];
}
else
{
    SecretClientOptions options = new SecretClientOptions()
    {
        Retry =
        {
            Delay= TimeSpan.FromSeconds(2),
            MaxDelay = TimeSpan.FromSeconds(16),
            MaxRetries = 5,
            Mode = RetryMode.Exponential
         }
    };
    var client = new SecretClient(new Uri("https://cfbvault.vault.azure.net/"), new DefaultAzureCredential(),options);
    KeyVaultSecret secret = client.GetSecret("CFBD-API-KEY");
    cfbdAPIkey = secret.Value;
}


// Connect to CollegeFootballData.com API
Configuration.Default.ApiKey.Add("Authorization", cfbdAPIkey);
Configuration.Default.ApiKeyPrefix.Add("Authorization", "Bearer");

app.UseHostFiltering();

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors(AllowAnyOrigin);
app.MapControllers();

app.Run();
