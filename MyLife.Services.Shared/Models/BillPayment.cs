using MyLife.Services.Shared.Models.Notion.Page;

namespace MyLife.Services.Shared.Models;
public class BillPayment : NotionObject
{
    public BillPayment(NotionPage notionPage) : base(notionPage)
    {
        if(notionPage.GetProperty("Bill Paid") is NotionProperty billPaid)
        {
            IsPaid = billPaid.IsChecked.GetValueOrDefault();
        }

        if(notionPage.GetProperty("Is Auto-Pay") is NotionProperty isAutoPay)
        {
            IsAutoPay = isAutoPay.IsChecked.GetValueOrDefault();
        }

        if (notionPage.GetProperty("Link to Pay") is NotionProperty linkToPay)
        {
            LinkToPay = linkToPay.Rollup?.Array?.FirstOrDefault()?.Uri;
        }

        if (notionPage.Properties["Amount"].Rollup?.Array?.FirstOrDefault() is NotionProperty amount)
        {
            Amount = amount.Number;
        }

        DateDue = DateTime.Parse(notionPage.Properties["Date"].Date!.Start).ToUniversalTime();

        if (notionPage.Properties["Date Paid"].Date is NotionDate datePaid)
        {
            DatePaid = DateTime.Parse(datePaid.Start).ToUniversalTime();
        }

        if (notionPage.Properties["Bill Configuration"].Relationships?.FirstOrDefault() is NotionRelationship relationship)
        {
            BillConfigurationId = Guid.Parse(relationship.Id);
        }

        if (notionPage.Properties["Tags"].Rollup?.Array?.FirstOrDefault() is NotionProperty tags)
        {
            Tags = tags.MultiSelect!.Select(tag => new NotionTag(tag.Color, tag.Name)).ToList();
        }
    }

    public bool IsPaid { get; set; }

    public bool IsAutoPay { get; set; }

    public Uri? LinkToPay { get; set; }

    public decimal? Amount { get; set; }

    public DateTime DateDue { get; set; }

    public DateTime? DatePaid { get; set; }

    public Guid BillConfigurationId { get; set; }

    public List<NotionTag> Tags { get; set; } = new();


}