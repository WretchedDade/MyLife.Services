using MyLife.Services.Shared.Models.Notion.Page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MyLife.Services.Shared.Models.Notion;
public class NotionPageUpdate
{
    [JsonPropertyName("properties")]
    public Dictionary<string, NotionProperty>? Properties { get; set; } = new();

    [JsonPropertyName("archived")]
    public bool? Archived { get; set; }

    [JsonPropertyName("cover")]
    public NotionCover? Cover { get; set; }

    [JsonPropertyName("icon")]
    public NotionIcon? Icon { get; set; }
}
