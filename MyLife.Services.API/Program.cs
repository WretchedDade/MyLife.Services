using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using MyLife.Services.API.Infra;
using MyLife.Services.Shared.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"))
        .EnableTokenAcquisitionToCallDownstreamApi()
            .AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
            .AddInMemoryTokenCaches();

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<NotionAppSettings>(builder.Configuration.GetRequiredSection("Notion"));

builder.Services.AddHttpClient<INotionAPI, NotionAPI>((services, client) =>
{
    var accessToken = services.GetRequiredService<IOptions<NotionAppSettings>>().Value.AccessToken;

    client.BaseAddress = new Uri("https://api.notion.com");

    client.DefaultRequestHeaders.Authorization = new("Bearer", accessToken);
    client.DefaultRequestHeaders.Add("Notion-Version", "2022-02-22");
});

builder.Services.AddScoped<INotionService, NotionService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();