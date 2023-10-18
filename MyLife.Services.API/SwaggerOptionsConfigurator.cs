using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MyLife.Services.API;

public class SwaggerOptionsConfigurator: IConfigureNamedOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public SwaggerOptionsConfigurator(IApiVersionDescriptionProvider provider) => _provider = provider;

    public void Configure(SwaggerGenOptions options)
    {
        // add swagger document for every API version discovered
        foreach (ApiVersionDescription description in this._provider.ApiVersionDescriptions)
        {
            OpenApiInfo openApiInfo = new()
            {
                Version = description.ApiVersion.ToString(),
                Title = $"My Life API {description.GroupName}",
            };

            if (description.IsDeprecated)
            {
                openApiInfo.Description += " This API version has been deprecated. Please use one of the new APIs available from the explorer.";
            }

            options.SwaggerDoc(description.GroupName, openApiInfo);
        }
    }

    public void Configure(string? name, SwaggerGenOptions options) => this.Configure(options);
}