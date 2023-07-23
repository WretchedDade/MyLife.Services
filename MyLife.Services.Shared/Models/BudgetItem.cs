using MyLife.Services.Shared.Models.Notion.Page;

namespace MyLife.Services.Shared.Models;
public class BudgetItem : NotionObject
{
    public BudgetItem(NotionPage notionPage) : base(notionPage)
    {
        if (notionPage.GetProperty("Day")?.Number is decimal day)
        {
            Day = (int)day;
        }

        if (notionPage.GetProperty("Date Type")?.Select is NotionSelectOption dateTypeSelect)
        {
            DateType = dateTypeSelect.Name;
        }

        if (notionPage.GetProperty("Amount")?.Number is decimal amount)
        {
            Amount = amount;
        }

        if (notionPage.GetProperty("Category")?.Select is NotionSelectOption categorySelect)
        {
            Category = categorySelect.Name;
        }

        if (notionPage.GetProperty("Tags")?.MultiSelect is NotionSelectOption[] tags)
        {
            Tags = tags.Select(tag => new NotionTag(tag.Color ?? string.Empty, tag.Name)).ToList();
        }

    }

    public int? Day { get; set; }

    public decimal Amount { get; set; } = default;

    public string DateType { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public bool IsIncome => Category == "Income";

    public List<NotionTag> Tags { get; set; } = new();
}
