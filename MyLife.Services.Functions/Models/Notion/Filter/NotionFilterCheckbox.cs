using System.Text.Json.Serialization;

namespace MyLife.Services.Functions.Models.Notion.Filter;

internal class NotionFilterCheckbox
{
    [JsonPropertyName("equals")]
    public bool? Equals { get; set; }

    [JsonPropertyName("does_not_equal")]
    public bool? DoesNotEqual { get; set; }
}
