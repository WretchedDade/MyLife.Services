using System.Text.Json.Serialization;

namespace MyLife.Services.Functions.Models.Notion.Page;
internal class NotionUser : NotionObject
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("avatar_url")]
    public string AvatarUrl { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("bot")]
    public string Bot { get; set; }

}
