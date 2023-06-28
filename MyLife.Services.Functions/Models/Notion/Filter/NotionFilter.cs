using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MyLife.Services.Functions.Models.Notion.Filter;
internal class NotionFilter : NotionFilterProperty
{
    [JsonPropertyName("and")]
    public List<NotionFilter>? And { get; set; }

    [JsonPropertyName("or")]
    public List<NotionFilter>? Or { get; set; }
}
