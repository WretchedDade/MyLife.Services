using MyLife.Services.Shared.Models.Notion.Page;

namespace MyLife.Services.Shared.Models;
public class BillPayment
{
    public BillPayment(NotionPage notionPage)
    {
        Name = notionPage.Properties["Name"].Title![0].PlainText;

        if (notionPage.Icon is NotionIcon icon)
        {
            Emoji = icon.Emoji;

            if (icon.External is NotionExternalIcon externalIcon)
            {
                IconUri = new Uri(externalIcon.Url);
            }
        }

        PageUri = notionPage.Uri;

        if (notionPage.Cover is NotionCover cover)
        {
            CoverUri = cover.External.Uri;
        }

        if (notionPage.Parent is NotionParent parent)
        {
            DatabaseUri = new Uri($"https://www.notion.so/dadecook/{parent.DatabaseId}");
        }

        IsPaid = notionPage.Properties["Bill Paid"].IsChecked.GetValueOrDefault();
        IsAutoPay = notionPage.Properties["Is Auto-Pay"].IsChecked.GetValueOrDefault();

        if (notionPage.Properties["Amount"].Rollup?.Array?.FirstOrDefault() is NotionProperty amount)
        {
            Amount = amount.Number;
        }

        DateDue = DateOnly.Parse(notionPage.Properties["Date"].Date!.Start);

        if (notionPage.Properties["Date Paid"].Date is NotionDate datePaid)
        {
            DatePaid = DateOnly.Parse(datePaid.Start);
        }

        if (notionPage.Properties["Tags"].Rollup?.Array?.FirstOrDefault() is NotionProperty tags)
        {
            Tags = tags.MultiSelect!.Select(tag => new BillPaymentTag(tag.Color, tag.Name)).ToList();
        }

        if (notionPage.Properties["Bill Configuration"].Relationships?.FirstOrDefault() is NotionRelationship relationship)
        {
            BillConfigurationId = Guid.Parse(relationship.Id);
        }
    }


    public string Name { get; set; }

    public string? Emoji { get; set; }

    public Uri? IconUri { get; set; }

    public Uri? PageUri { get; set; }

    public Uri? CoverUri { get; set; }

    public Uri? DatabaseUri { get; set; }


    public bool IsPaid { get; set; }

    public bool IsAutoPay { get; set; }

    public decimal? Amount { get; set; }

    public DateOnly DateDue { get; set; }

    public DateOnly? DatePaid { get; set; }

    public List<BillPaymentTag> Tags { get; set; } = new();

    public Guid BillConfigurationId { get; set; }

}

public record BillPaymentTag(string Color, string Name);