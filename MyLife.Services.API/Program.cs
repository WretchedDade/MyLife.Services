using Azure.Core.Diagnostics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using MyLife.Services.API;
using MyLife.Services.API.Infra;
using MyLife.Services.Shared.Services;
using Prometheus;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;
using System.Text.Json.Serialization;
using static Prometheus.MetricServerMiddleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"))
        .EnableTokenAcquisitionToCallDownstreamApi()
            .AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
            .AddInMemoryTokenCaches();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policyBuilder => policyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    options.AddPolicy("OnlyFrontEnd", policyBuilder => policyBuilder.WithOrigins(builder.Configuration["FrontEndBaseUrl"] ?? "").AllowAnyMethod().AllowAnyHeader());
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle


builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.UseApiBehavior = true;

    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader(), new HeaderApiVersionReader("x-api-version"), new MediaTypeApiVersionReader("x-api-version"));
});

//builder.Services.AddEndpointsApiExplorer();
builder.Services.AddVersionedApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});

// Configure Swagger
builder.Services.AddSwaggerGen();
_ = builder.Services.ConfigureOptions<SwaggerOptionsConfigurator>();

builder.Services.AddHealthChecks();

builder.Services.Configure<NotionAppSettings>(builder.Configuration.GetRequiredSection("Notion"));

builder.Services.AddHttpClient<INotionAPI, NotionAPI>((services, client) =>
{
    var accessToken = services.GetRequiredService<IOptions<NotionAppSettings>>().Value.AccessToken;

    client.BaseAddress = new Uri("https://api.notion.com");

    client.DefaultRequestHeaders.Authorization = new("Bearer", accessToken);
    client.DefaultRequestHeaders.Add("Notion-Version", "2022-02-22");
});

builder.Services.AddSingleton<CosmosClient>(services =>
{
    var configuration = services.GetRequiredService<IConfiguration>();


    var key = configuration["Cosmos:MyLifeKey"] ?? "";
    var endpoint = configuration["Cosmos:MyLifeEndpoint"] ?? "";

    CosmosClientOptions cosmosClientOptions = new()
    {
        SerializerOptions = new CosmosSerializationOptions()
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        }
    };

    return new(endpoint, key, cosmosClientOptions);
});

builder.Services.AddScoped<INotionService, NotionService>();

builder.Services.AddScoped<IBloodPressureService, BloodPressureService>();
builder.Services.AddScoped<IAccountActivityService, AccountActivityService>();
builder.Services.AddScoped<IBankKeywordConfigService, BankKeywordConfigService>();


// Setup a listener to monitor logged events.
using AzureEventSourceListener listener = AzureEventSourceListener.CreateConsoleLogger();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    IdentityModelEventSource.ShowPII = true;

    app.UseCors("AllowAll");
}
else
{
    app.UseCors("OnlyFrontEnd");
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.DocExpansion(DocExpansion.None);

    IApiVersionDescriptionProvider apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    foreach (ApiVersionDescription? description in apiVersionDescriptionProvider.ApiVersionDescriptions.Reverse())
    {
        options.RoutePrefix = string.Empty;
        options.DocumentTitle = "MCAPS Academy API Documentation";

        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
    }
});

app.MapHealthChecks("/health");

app.UseHttpsRedirection();

app.UseHttpMetrics(options => options.ReduceStatusCodeCardinality());

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapMetrics();

app.Run();