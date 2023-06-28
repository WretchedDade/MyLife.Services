﻿using System.Text.Json.Serialization;

namespace MyLife.Services.Functions.Models.Notion.Page;
internal class NotionFormula
{
    [JsonPropertyName("boolean")]
    public bool? Boolean { get; set; }

    [JsonPropertyName("date")]
    public NotionDate? Date { get; set; }

    [JsonPropertyName("number")]
    public double? Number { get; set; }

    [JsonPropertyName("string")]
    public string? String { get; set; }

    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public NotionFormulaTypes Type { get; set; }
}
