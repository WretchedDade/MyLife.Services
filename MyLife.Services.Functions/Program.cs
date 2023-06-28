using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyLife.Services.Functions;
using MyLife.Services.Shared.Services;
using System;
using System.Threading.Tasks;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults()
            .ConfigureServices(services =>
            {
                services.AddHttpClient<INotionAPI, NotionAPI>(client =>
                {
                    client.BaseAddress = new Uri("https://api.notion.com");

                    client.DefaultRequestHeaders.Authorization = new("Bearer", FunctionHelpers.GetEnvironmentVariable(EnvironmentVariables.NotionAccessToken));
                    client.DefaultRequestHeaders.Add("Notion-Version", "2022-02-22");
                });
            })
            .Build();

        await host.RunAsync();
    }
}