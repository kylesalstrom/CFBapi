using CFBSharp.Client;

var builder = WebApplication.CreateBuilder(args);

//Enable Cors for use in Dev
string AllowAnyOrigin = "AllowAnyOrigin";
builder.Services.AddCors(options => options.AddPolicy(AllowAnyOrigin, policy => policy.AllowAnyOrigin()));

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Connect to CollegeFootballData.com API
Configuration.Default.ApiKey.Add("Authorization", builder.Configuration["CFBD_API_KEY"]);
Configuration.Default.ApiKeyPrefix.Add("Authorization", "Bearer");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(AllowAnyOrigin);
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
