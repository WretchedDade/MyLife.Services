using System;
using System.Text.Json.Serialization;

namespace MyLife.Services.Functions.Models.Notion.Page;

internal class NotionExternalCover
{
    [JsonPropertyName("url")]
    public Uri Uri { get; set; }
}
