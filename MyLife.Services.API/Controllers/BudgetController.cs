using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web.Resource;
using MyLife.Services.API.Infra;
using MyLife.Services.Shared.Models;
using MyLife.Services.Shared.Models.Notion.Page;
using MyLife.Services.Shared.Services;

namespace MyLife.Services.API.Controllers;

[Authorize]
[ApiController]
[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
public class BudgetController : ControllerBase
{
    private readonly INotionAPI _notionAPI;
    private readonly NotionAppSettings _notionAppSettings;


    public BudgetController(INotionAPI notionAPI, IOptions<NotionAppSettings> notionAppSettingsOptions)
    {
        _notionAPI = notionAPI;
        _notionAppSettings = notionAppSettingsOptions.Value;
    }

    [HttpGet("[controller]", Name = "Get All Budget Items")]
    public async Task<IActionResult> GetAll()
    {
        var pages = await _notionAPI.QueryDatabase<NotionPage>(_notionAppSettings.BudgetDatabaseId);

        var budgetItems = pages.Select(page => new BudgetItem(page)).OrderBy(bill => bill.Day ?? 31);

        return Ok(budgetItems);
    }
}
