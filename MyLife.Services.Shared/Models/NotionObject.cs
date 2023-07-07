using MyLife.Services.Shared.Models.Notion.Page;

namespace MyLife.Services.Shared.Models;

public class NotionObject
{
    public NotionObject(NotionPage notionPage)
    {
        Id = Guid.Parse(notionPage.Id);

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
            CoverUri = cover.Uri;
        }

        if (notionPage.Parent is NotionParent parent)
        {
            DatabaseUri = new Uri($"https://www.notion.so/dadecook/{parent.DatabaseId}");
        }

    }

    public Guid Id { get; set; }

    public string Name { get; set; }

    public string? Emoji { get; set; }

    public Uri? IconUri { get; set; }

    public Uri? PageUri { get; set; }

    public Uri? CoverUri { get; set; }

    public Uri? DatabaseUri { get; set; }
}
