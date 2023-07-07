using System.Text.Json.Serialization;

namespace MyLife.Services.Shared.Models.Notion.Page;
public class NotionCover
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "external";

    [JsonPropertyName("file")]
    public NotionFileCover? File {get;set;}

    [JsonPropertyName("external")]
    public NotionExternalCover? External { get; set; }

    [JsonIgnore]
    public Uri? Uri => Type switch
    {
        "external" => External?.Uri,
        "file" => File?.Uri,
        _ => null
    };
}

public class NotionFileCover
{
    [JsonPropertyName("url")]
    public Uri Uri { get; set; }

    [JsonPropertyName("expiry_time")]
    public DateTime ExpiryTime { get; set; }
}
