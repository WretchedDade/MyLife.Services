using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using MyLife.Services.Shared.Models.Notion.Filter;
using MyLife.Services.Shared.Models.Notion.Page;
using MyLife.Services.Shared.Models;
using MyLife.Services.Shared.Services;
using Microsoft.Extensions.Options;
using MyLife.Services.API.Infra;

namespace MyLife.Services.API.Controllers;

[Authorize]
[ApiController]
[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
public class BillsController : ControllerBase
{
    private readonly INotionAPI _notionAPI;
    private readonly INotionService _notionService;
    private readonly NotionAppSettings _notionAppSettings;


    public BillsController(INotionAPI notionAPI, INotionService notionService, IOptions<NotionAppSettings> notionAppSettingsOptions)
    {
        _notionAPI = notionAPI;
        _notionService = notionService;
        _notionAppSettings = notionAppSettingsOptions.Value;
    }

    [HttpGet("[controller]", Name = "Get All Bills")]
    public async Task<IActionResult> GetAll([FromQuery] bool? unpaidOnly = null)
    {
        NotionFilter? filter = null;

        if (unpaidOnly.HasValue)
            filter = new() { Property = "Bill Paid", Checkbox = NotionFilterCheckbox.ThatEquals(false) };

        var list = await _notionAPI.QueryDatabase<NotionPage>(_notionAppSettings.BillPaymentsDatabaseId, filter: filter);

        var billPayments = list.Results.Select(page => new BillPayment(page)).OrderBy(bill => bill.DateDue);

        return Ok(billPayments);
    }

    [HttpPost("[controller]/{id:guid}/Pay", Name = "Mark Bill as Paid")]
    public async Task<IActionResult> MarkBillAsPaid(Guid id)
    {
        var billPayment = await _notionService.MarkBillAsPaid(id.ToString());

        return Ok(billPayment);
    }
}
