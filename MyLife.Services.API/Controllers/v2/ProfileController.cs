using Azure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Graph.Models.ODataErrors;
using Microsoft.Identity.Web.Resource;

namespace MyLife.Services.API.Controllers.v2;

[Authorize]
[ApiController]
[ApiVersion("2.0")]
[Route("v{version:apiVersion}/[controller]")]
[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
public class ProfileController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public ProfileController(IConfiguration configuration) => _configuration = configuration;


    [HttpGet(Name = "Get the signed in user's profile")]
    public async Task<IActionResult> GetProfile()
    {
        var onBehalfOfCredential = await GetOnBehalfOfCredentialAsync();
        var graphClient = new GraphServiceClient(onBehalfOfCredential);
        var me = await graphClient.Me.GetAsync();

        return Ok(me);

    }

    [HttpGet("Photo", Name = "Get the signed-in user's profile photo")]
    public async Task<IActionResult> MyPhoto()
    {
        try
        {
            var onBehalfOfCredential = await GetOnBehalfOfCredentialAsync();
            var graphClient = new GraphServiceClient(onBehalfOfCredential);
            var photo = await graphClient.Me.Photo.Content.GetAsync();

            return Ok(photo);
        }
        catch (ODataError oDataError) when (oDataError.Error?.Code == "ImageNotFound")
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex);
        }

    }

    private async Task<OnBehalfOfCredential> GetOnBehalfOfCredentialAsync()
    {
        var tenantId = _configuration["AzureAd:TenantId"];
        var clientId = _configuration["AzureAd:ClientId"];
        var clientSecret = _configuration["AzureAd:ClientSecret"];

        var options = new OnBehalfOfCredentialOptions
        {
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
        };

        var accessToken = await base.HttpContext.GetTokenAsync("access_token");

        return new OnBehalfOfCredential(tenantId, clientId, clientSecret, accessToken, options);
    }
}
