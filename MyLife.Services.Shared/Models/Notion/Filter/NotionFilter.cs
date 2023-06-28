using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MyLife.Services.Shared.Models.Notion.Filter;
public class NotionFilter : NotionFilterProperty
{
    [JsonPropertyName("and")]
    public List<NotionFilter>? And { get; set; }

    [JsonPropertyName("or")]
    public List<NotionFilter>? Or { get; set; }
}
