using System.Text.Json.Serialization;

namespace MyLife.Services.Shared.Models.Notion.Page;

public class NotionRichTextAnnotations
{
    [JsonPropertyName("bold")]
    public bool Bold { get; set; }

    [JsonPropertyName("italic")]
    public bool Italic { get; set; }

    [JsonPropertyName("strikethrough")]
    public bool Strikethrough { get; set; }

    [JsonPropertyName("underline")]
    public bool Underline { get; set; }

    [JsonPropertyName("code")]
    public bool Code { get; set; }

    [JsonPropertyName("color")]
    public string Color { get; set; } = "default";
}
