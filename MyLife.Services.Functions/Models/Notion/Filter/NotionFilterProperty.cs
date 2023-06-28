using System.Text.Json.Serialization;

namespace MyLife.Services.Functions.Models.Notion.Filter;

internal class NotionFilterProperty
{
    [JsonPropertyName("property")]
    public string? Property { get; set; }

    [JsonPropertyName("checkbox")]
    public NotionFilterCheckbox? Checkbox { get; set; }

    [JsonPropertyName("date")]
    public NotionFilterDate? Date { get; set; }

    //[JsonPropertyName("files")]
    //public string files { get; set; }

    [JsonPropertyName("formula")]
    public NotionFilterFormula? Formula { get; set; }

    [JsonPropertyName("multi_select")]
    public NotionFilterMultiSelect? MultiSelect { get; set; }

    [JsonPropertyName("number")]
    public NotionFilterNumber? Number { get; set; }

    //[JsonPropertyName("people")]
    //public string people { get; set; }

    //[JsonPropertyName("phone_number")]
    //public string phone_number { get; set; }

    [JsonPropertyName("relation")]
    public NotionFilterRelation? Relation { get; set; }

    [JsonPropertyName("rich_text")]
    public NotionFilterRichText? RichText { get; set; }

    [JsonPropertyName("select")]
    public NotionFilterSelect? Select { get; set; }

    [JsonPropertyName("status")]
    public NotionFilterSelect? Status { get; set; }

    //[JsonPropertyName("timestamp")]
    //public string timestamp { get; set; }
}
