using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MyLife.Services.Shared.Models.Notion.Page;
public class NotionPage : NotionObject
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("created_time")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("last_edited_time")]
    public DateTime LastEditedAt { get; set; }

    [JsonPropertyName("cover")]
    public NotionCover? Cover { get; set; }

    [JsonPropertyName("icon")]
    public NotionIcon? Icon { get; set; }

    [JsonPropertyName("archived")]
    public bool Archived { get; set; }

    [JsonPropertyName("properties")]
    public Dictionary<string, NotionProperty> Properties { get; set; }

    [JsonPropertyName("url")]
    public Uri? Uri { get; set; }

    [JsonPropertyName("parent")]
    public NotionParent? Parent { get; set; }

    [JsonIgnore]
    public string Name => GetProperty("Name")?.Title![0].PlainText ?? string.Empty;

    public NotionProperty? GetProperty(string name)
    {
        return Properties.ContainsKey(name) ? Properties[name] : null;
    }
}
