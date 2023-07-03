using MyLife.Services.Shared.Models.Notion.Page;
using System;

namespace MyLife.Services.Shared.Models.Notion;
public class BillConfigurationPage : NotionPage
{
    public string Name => Properties["Name"].Title![0].PlainText;

    public decimal Amount => Properties["Amount"].Number!.Value;

    public bool IsAutoPay => Properties["Is Auto-Pay"].IsChecked.GetValueOrDefault();

    public DayDueTypes DayDueType => Properties["Day Due Type"].Select!.Id switch
    {
        DayDueTypeIds.Fixed => DayDueTypes.Fixed,
        DayDueTypeIds.EndOfMonth => DayDueTypes.EndOfMonth,
        _ => throw new NotImplementedException($"DayDueType {Properties["Day Due Type"].Id} is not implemented")
    };

    public int? DayDue => (int?)Properties["Day Due"].Number;

    public override string ToString()
    {
        return $"{Name}: {Amount}";
    }
}

public enum DayDueTypes
{
    Fixed, EndOfMonth
}

public static class DayDueTypeIds
{
    public const string Fixed = "g@]J";
    public const string EndOfMonth = "pv@@";
}
