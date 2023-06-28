using System;
using System.Text.Json.Serialization;

namespace MyLife.Services.Shared.Models.Notion.Page;

public class NotionExternalCover
{
    [JsonPropertyName("url")]
    public Uri Uri { get; set; }
}
