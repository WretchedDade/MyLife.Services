using System.Text.Json.Serialization;

namespace MyLife.Services.Shared.Models.Notion.Page;
public class NotionRelationship
{
    public NotionRelationship()
    {

    }

    public NotionRelationship(string id) => Id = id;

    [JsonPropertyName("id")]
    public string Id { get; set; }
}
