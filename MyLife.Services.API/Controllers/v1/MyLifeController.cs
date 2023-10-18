using Azure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;

namespace MyLife.Services.API.Controllers.v1;

public class MyLifeController : ControllerBase
{
    protected readonly IConfiguration configuration;

    public MyLifeController(IConfiguration configuration) => this.configuration = configuration;

    protected async Task<OnBehalfOfCredential> GetOnBehalfOfCredentialAsync()
    {
        var tenantId = configuration["AzureAd:TenantId"];
        var clientId = configuration["AzureAd:ClientId"];
        var clientSecret = configuration["AzureAd:ClientSecret"];

        var options = new OnBehalfOfCredentialOptions
        {
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
        };

        var accessToken = await HttpContext.GetTokenAsync("access_token");

        return new OnBehalfOfCredential(tenantId, clientId, clientSecret, accessToken, options);
    }
}
