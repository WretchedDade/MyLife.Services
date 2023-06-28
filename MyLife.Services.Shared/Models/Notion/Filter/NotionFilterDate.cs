using System;
using System.Text.Json.Serialization;

namespace MyLife.Services.Shared.Models.Notion.Filter;

public class NotionFilterDate
{
    [JsonPropertyName("after")]
    public DateTime? After { get; set; }

    [JsonPropertyName("before")]
    public DateTime? Before { get; set; }

    [JsonPropertyName("equals")]
    public new DateTime? Equals { get; set; }

    [JsonPropertyName("is_empty")]
    public bool? IsEmpty { get; set; }

    [JsonPropertyName("is_not_empty")]
    public bool? IsNotEmpty { get; set; }

    [JsonPropertyName("next_month")]
    public object? NextMonth { get; set; }

    [JsonPropertyName("next_week")]
    public object? NextWeek { get; set; }

    [JsonPropertyName("next_year")]
    public object? NextYear { get; set; }

    [JsonPropertyName("on_or_after")]
    public DateTime? OnOrAfter { get; set; }

    [JsonPropertyName("on_or_before")]
    public DateTime? OnOrBefore { get; set; }

    [JsonPropertyName("past_month")]
    public object? PastMonth { get; set; }

    [JsonPropertyName("past_week")]
    public object? PastWeek { get; set; }

    [JsonPropertyName("past_year")]
    public object? PastYear { get; set; }

    [JsonPropertyName("this_week")]
    public object? ThisWeek { get; set; }

}
