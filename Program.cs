using Microsoft.AspNetCore.HostFiltering;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Core;
using CFBSharp.Client;

var builder = WebApplication.CreateBuilder(args);

//Enable Cors for use in Dev
string AllowAnyOrigin = "_allowAnyOrigin";
string AllowSpecificOrigins = "_allowSpecificOrigins";
builder.Services.AddCors(options => options.AddPolicy(AllowAnyOrigin, policy => policy.AllowAnyOrigin()));
builder.Services.AddCors(options => options.AddPolicy(AllowSpecificOrigins,
                      policy  =>
                      {
                          policy.WithOrigins("https://dev.CFBtracker.com",
                                              "https://www.CFBtracker.com",
                                              "https://*.CFBtracker.com",
                                              "http://dev.CFBtracker.com",
                                              "http://www.CFBtracker.com",
                                              "http://*.CFBtracker.com",
                                              "*.CFBtracker.com",
                                              "*CFBtracker.com",
                                              "CFBtracker.com")
                                                  .AllowAnyHeader()
                                                  .AllowAnyMethod();
                      }));

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
    app.UseCors(AllowAnyOrigin);
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
    app.UseCors(AllowSpecificOrigins);
}


// Connect to CollegeFootballData.com API
Configuration.Default.ApiKey.Add("Authorization", cfbdAPIkey);
Configuration.Default.ApiKeyPrefix.Add("Authorization", "Bearer");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapGet("/echo",
        context => context.Response.WriteAsync("echo"))
        .RequireCors(AllowSpecificOrigins);

    endpoints.MapControllers()
             .RequireCors(AllowAnyOrigin);

    endpoints.MapGet("/echo2",
        context => context.Response.WriteAsync("echo2"));
});

app.Run();
