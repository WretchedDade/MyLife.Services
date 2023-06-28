using System.Text.Json.Serialization;

namespace MyLife.Services.Shared.Models.Notion.Filter;

public class NotionFilterCheckbox
{
    [JsonPropertyName("equals")]
    public bool? Equals { get; set; }

    [JsonPropertyName("does_not_equal")]
    public bool? DoesNotEqual { get; set; }
}
