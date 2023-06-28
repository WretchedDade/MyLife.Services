using System.Text.Json.Serialization;

namespace MyLife.Services.Shared.Models.Notion.Page;
public class NotionRichText
{
    public NotionRichText()
    {

    }

    public NotionRichText(string text)
    {
        Type = "text";

        Text = new()
        {
            Content = text
        };

        PlainText = text;
    }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("text")]
    public NotionText Text { get; set; } = new();

    [JsonPropertyName("annotations")]
    public NotionRichTextAnnotations Annotations { get; set; } = new();

    [JsonPropertyName("plain_text")]
    public string PlainText { get; set; }

    [JsonPropertyName("href")]
    public string? Href { get; set; }
}
