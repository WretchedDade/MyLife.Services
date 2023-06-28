using MyLife.Services.Functions.Models.Notion.Page;
using System;

namespace MyLife.Services.Functions.Models;
internal class BillConfiguration : NotionPage
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

internal enum DayDueTypes
{
    Fixed, EndOfMonth
}

internal static class DayDueTypeIds
{
    internal const string Fixed = "g@]J";
    internal const string EndOfMonth = "pv@@";
}
