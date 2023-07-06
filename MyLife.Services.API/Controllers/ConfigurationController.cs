using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using MyLife.Services.Shared.Models.Notion.Filter;
using MyLife.Services.Shared.Models.Notion.Page;
using MyLife.Services.Shared.Models;
using MyLife.Services.Shared.Services;
using Microsoft.Extensions.Options;
using MyLife.Services.API.Infra;
using MyLife.Services.Shared.Models.Notion;

namespace MyLife.Services.API.Controllers;

[Authorize]
[ApiController]
[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
public class ConfigurationController : ControllerBase
{
    private readonly INotionAPI _notionAPI;
    private readonly NotionAppSettings _notionAppSettings;


    public ConfigurationController(INotionAPI notionAPI, IOptions<NotionAppSettings> notionAppSettingsOptions)
    {
        _notionAPI = notionAPI;
        _notionAppSettings = notionAppSettingsOptions.Value;
    }

    [HttpGet("[controller]/Bills/{id:guid}", Name = "Get Bill Configuration by Id")]
    public async Task<IActionResult> GetBillConfigurationById(Guid id)
    {
        var billConfigurationPage = await _notionAPI.GetPage<BillConfigurationPage>(id.ToString());

        if (billConfigurationPage is BillConfigurationPage page)
            return Ok(new BillConfiguration(page));
        else
            return NotFound();
    }

    [HttpGet("[controller]/Bills", Name = "Get All Bill Configurations")]
    public async Task<IActionResult> GetAllBillConfigurations()
    {
        var list = await _notionAPI.QueryDatabase<BillConfigurationPage>(_notionAppSettings.BillConfigurationDatabaseId);

        var billConfigurations = list.Results.Select(page => new BillConfiguration(page));

        return Ok(billConfigurations);
    }
}
