using System.Text.Json.Serialization;

namespace MyLife.Services.Functions.Models.Notion.Filter;

internal class NotionFilterMultiSelect
{
    [JsonPropertyName("contains")]
    public string? Contains { get; set; }

    [JsonPropertyName("does_not_contain")]
    public string? DoesNotContain { get; set; }

    [JsonPropertyName("is_empty")]
    public bool? IsEmpty { get; set; }

    [JsonPropertyName("is_not_empty")]
    public bool? IsNotEmpty { get; set; }

}
