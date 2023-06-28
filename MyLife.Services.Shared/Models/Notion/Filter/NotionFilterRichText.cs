using System.Text.Json.Serialization;

namespace MyLife.Services.Shared.Models.Notion.Filter;

public class NotionFilterRichText
{
    [JsonPropertyName("contains")]
    public string? Contains { get; set; }

    [JsonPropertyName("does_not_contain")]
    public string? DoesNotContain { get; set; }

    [JsonPropertyName("does_not_equal")]
    public string? DoesNotEqual { get; set; }

    [JsonPropertyName("ends_with")]
    public string? EndsWith { get; set; }

    [JsonPropertyName("equals")]
    public new string? Equals { get; set; }

    [JsonPropertyName("is_empty")]
    public bool? IsEmpty { get; set; }

    [JsonPropertyName("is_not_empty")]
    public bool? IsNotEmpty { get; set; }

    [JsonPropertyName("starts_with")]
    public string? StartsWith { get; set; }

}
