using MyLife.Services.Shared.Models;
using MyLife.Services.Shared.Models.Notion.Page;

namespace MyLife.Services.Shared.Services;

public class NotionService : INotionService
{
    private readonly INotionAPI _notionAPI;

    public NotionService(INotionAPI notionAPI) => _notionAPI = notionAPI;

    public async Task<BillPayment> MarkBillAsPaid(string id)
    {
        var updatedPage = await _notionAPI.UpdatePage(id, propertyUpdates: new()
        {
            { "Bill Paid", NotionProperty.OfCheckbox(true) },
            { "Date Paid", NotionProperty.OfDate(DateTime.Now.ToString("yyyy-MM-dd")) }
        });

        BillPayment billPayment = new(updatedPage);

        return billPayment;
    }
}
