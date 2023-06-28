using System.Text.Json.Serialization;

namespace MyLife.Services.Shared.Models.Notion.Filter;

public class NotionFilterFormula
{
    [JsonPropertyName("checkbox")]
    public NotionFilterCheckbox? Checkbox { get; set; }

    [JsonPropertyName("date")]
    public NotionFilterDate? Date { get; set; }

    [JsonPropertyName("number")]
    public NotionFilterNumber? Number { get; set; }

    [JsonPropertyName("string")]
    public NotionFilterRichText? String { get; set; }

}
