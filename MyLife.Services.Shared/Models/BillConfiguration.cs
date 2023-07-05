using MyLife.Services.Shared.Models.Notion;

namespace MyLife.Services.Shared.Models;
public class BillConfiguration : NotionObject
{
    public BillConfiguration(BillConfigurationPage page) : base(page)
    {
        DayDue = page.DayDue;
        Amount = page.Amount;
        IsAutoPay = page.IsAutoPay;
        LinkToPay = page.Properties["Link to Pay"].Uri;
        DayDueType = page.DayDueType;
        Tags = page.Properties["Tags"].MultiSelect!.Select(tag => new NotionTag(tag.Color, tag.Name)).ToList();
    }

    public int? DayDue { get; set; }

    public decimal Amount { get; set; }

    public bool IsAutoPay { get; set; }

    public Uri? LinkToPay { get; set; }

    public DayDueTypes DayDueType { get; set; }

    public List<NotionTag> Tags { get; set; } = new();

}
