using System.Text.Json.Serialization;

namespace MyLife.Services.Functions.Models.Notion.Filter;

internal class NotionFilterSelect
{
    [JsonPropertyName("equals")]
    public new string? Equals { get; set; }

    [JsonPropertyName("does_not_equal")]
    public string? DoesNotEqual { get; set; }

    [JsonPropertyName("is_empty")]
    public bool? IsEmpty { get; set; }

    [JsonPropertyName("is_not_empty")]
    public bool? IsNotEmpty { get; set; }
}
