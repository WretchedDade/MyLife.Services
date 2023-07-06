using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Graph.Models.ODataErrors;
using Microsoft.Identity.Web.Resource;

namespace MyLife.Services.API.Controllers;

[Authorize]
[ApiController]
[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
public class ProfileController : MyLifeController
{
    public ProfileController(IConfiguration configuration)
        : base(configuration) { }

    [HttpGet("[controller]/Me", Name = "Get My Profile")]
    public async Task<IActionResult> Me()
    {
        var onBehalfOfCredential = await GetOnBehalfOfCredentialAsync();
        var graphClient = new GraphServiceClient(onBehalfOfCredential);
        var me = await graphClient.Me.GetAsync();

        return Ok(me);

    }

    [HttpGet("[controller]/Me/Photo", Name = "Get My Profile Photo")]
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
}
