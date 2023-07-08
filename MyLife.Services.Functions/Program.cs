using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyLife.Services.Functions;
using MyLife.Services.Shared.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var host = new HostBuilder()
            .ConfigureFunctionsWebApplication()
            .ConfigureServices(services =>
            {
                services.AddMvcCore().AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                });

                services.ConfigureHttpJsonOptions(options => options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

                services.AddHttpClient<INotionAPI, NotionAPI>(client =>
                {
                    client.BaseAddress = new Uri("https://api.notion.com");

                    client.DefaultRequestHeaders.Authorization = new("Bearer", FunctionHelpers.GetEnvironmentVariable(EnvironmentVariables.NotionAccessToken));
                    client.DefaultRequestHeaders.Add("Notion-Version", "2022-02-22");
                });

                services.AddScoped<INotionService, NotionService>();

                services.AddScoped<BloodPressureSettings>(services =>
                {
                    var key = FunctionHelpers.GetEnvironmentVariable(EnvironmentVariables.CosmosMyLifeKey);
                    var endpoint = FunctionHelpers.GetEnvironmentVariable(EnvironmentVariables.CosmosMyLifeEndpoint);

                    return new()
                    {
                        Key = key,
                        Endpoint = endpoint,
                    };
                });
                services.AddScoped<IBloodPressureService, BloodPressureService>();
            })
            .Build();

        await host.RunAsync();
    }
}