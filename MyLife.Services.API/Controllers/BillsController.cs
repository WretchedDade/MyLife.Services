using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web.Resource;
using MyLife.Services.API.Infra;
using MyLife.Services.Shared.Extensions;
using MyLife.Services.Shared.Models;
using MyLife.Services.Shared.Models.Notion.Filter;
using MyLife.Services.Shared.Models.Notion.Page;
using MyLife.Services.Shared.Services;

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

    [HttpGet("[controller]/Unpaid", Name = "Get Unpaid Bills")]
    public async Task<IActionResult> GetUnpaid()
    {
        var pages = await _notionAPI.QueryDatabase<NotionPage>(_notionAppSettings.BillPaymentsDatabaseId, filter: new()
        {
            Property = "Bill Paid",
            Checkbox = NotionFilterCheckbox.ThatEquals(false)
        });

        var billPayments = pages.Select(page => new BillPayment(page)).OrderBy(bill => bill.DateDue);

        return Ok(billPayments);
    }

    [HttpGet("[controller]/ThisWeek", Name = "Get bills due this week")]
    public async Task<IActionResult> GetThisWeek()
    {
        var pages = await _notionAPI.QueryDatabase<NotionPage>(_notionAppSettings.BillPaymentsDatabaseId, filter: new()
        {
            Property = "Date",
            Date = new() { ThisWeek = new { } }
        });

        var billPayments = pages.Select(page => new BillPayment(page)).OrderBy(bill => bill.DateDue);

        return Ok(billPayments);
    }

    [HttpGet("[controller]/NextWeek", Name = "Get bills due next week")]
    public async Task<IActionResult> GetNextWeek()
    {
        var startOfNextWeek = DateTime.UtcNow.Date.GetNextMonday();

        var pages = await _notionAPI.QueryDatabase<NotionPage>(_notionAppSettings.BillPaymentsDatabaseId, filter: new()
        {
            And = new()
            {
                new(){ Property = "Date", Date = new() { OnOrAfter = startOfNextWeek } },
                new(){ Property = "Date", Date = new() { OnOrBefore = startOfNextWeek.AddDays(7) } }
            }
        });

        var billPayments = pages.Select(page => new BillPayment(page)).OrderBy(bill => bill.DateDue);

        return Ok(billPayments);
    }

    [HttpPost("[controller]/{id:guid}/Pay", Name = "Mark Bill as Paid")]
    public async Task<IActionResult> MarkBillAsPaid(Guid id)
    {
        var billPayment = await _notionService.MarkBillAsPaid(id.ToString());

        return Ok(billPayment);
    }
}
