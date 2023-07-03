using System.Text.Json.Serialization;

namespace MyLife.Services.Shared.Models.Notion.Filter;

public class NotionFilterCheckbox
{
    public NotionFilterCheckbox()
    {
        
    }

    public NotionFilterCheckbox(bool? equals, bool? doesNotEqual)
    {
        Equals = equals;
        DoesNotEqual = doesNotEqual;
    }

    [JsonPropertyName("equals")]
    public new bool? Equals { get; set; }

    [JsonPropertyName("does_not_equal")]
    public bool? DoesNotEqual { get; set; }

    public static NotionFilterCheckbox ThatEquals(bool value) => new(value, null);

    public static NotionFilterCheckbox ThatDoesNotEqual(bool value) => new(null, value);
}
