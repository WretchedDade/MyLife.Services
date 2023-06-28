using System.Text.Json.Serialization;

namespace MyLife.Services.Functions.Models.Notion.Page;
internal class NotionRelationship
{
    public NotionRelationship()
    {

    }

    public NotionRelationship(string id) => Id = id;

    [JsonPropertyName("id")]
    public string Id { get; set; }
}
