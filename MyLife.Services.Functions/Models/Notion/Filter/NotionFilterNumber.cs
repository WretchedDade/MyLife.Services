using System.Text.Json.Serialization;

namespace MyLife.Services.Functions.Models.Notion.Filter;

internal class NotionFilterNumber
{
    [JsonPropertyName("does_not_equal")]
    public decimal? DoesNotEqual { get; set; }

    [JsonPropertyName("equals")]
    public new decimal? Equals { get; set; }

    [JsonPropertyName("greater_than")]
    public decimal? GreaterThan { get; set; }

    [JsonPropertyName("greater_than_or_equal_to")]
    public decimal? GreaterThanOrEqualTo { get; set; }

    [JsonPropertyName("is_empty")]
    public bool? IsEmpty { get; set; }

    [JsonPropertyName("is_not_empty")]
    public bool? IsNotEmpty { get; set; }

    [JsonPropertyName("less_than")]
    public decimal? LessThan { get; set; }

    [JsonPropertyName("less_than_or_equal_to")]
    public decimal? LessThanOrEqualTo { get; set; }

}
