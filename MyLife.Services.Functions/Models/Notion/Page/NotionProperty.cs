﻿using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace MyLife.Services.Functions.Models.Notion.Page;

internal class NotionProperty
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public NotionPropertyType Type { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("checkbox")]
    public bool? IsChecked { get; set; }

    [JsonPropertyName("created_by")]
    public NotionUser? CreatedBy { get; set; }

    [JsonPropertyName("created_time")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("date")]
    public NotionDate? Date { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("formula")]
    public NotionFormula? Formula { get; set; }

    [JsonPropertyName("last_edited_by")]
    public NotionUser? LastEditedBy { get; set; }

    [JsonPropertyName("last_edited_time")]
    public DateTime? LastEditedAt { get; set; }

    [JsonPropertyName("multi_select")]
    public NotionSelectOption[]? MultiSelect { get; set; }

    [JsonPropertyName("number")]
    public decimal? Number { get; set; }

    [JsonPropertyName("people")]
    public NotionUser[]? People { get; set; }

    [JsonPropertyName("phone_number")]
    public string? PhoneNumber { get; set; }

    [JsonPropertyName("relation")]
    public NotionRelationship[]? Relation { get; set; }

    [JsonPropertyName("rich_text")]
    public NotionRichText[]? RichText { get; set; }

    //[JsonPropertyName("rollup")]
    //public string rollup { get; set; }

    [JsonPropertyName("select")]
    public NotionSelectOption? Select { get; set; }

    [JsonPropertyName("status")]
    public NotionSelectOption? Status { get; set; }

    [JsonPropertyName("title")]
    public NotionRichText[]? Title { get; set; }

    [JsonPropertyName("url")]
    public Uri? Uri { get; set; }

    [JsonPropertyName("has_more")]
    public bool? HasMore { get; set; }

    public static NotionProperty OfDate(string start, string? end = null, string? timeZone = null) => new()
    {
        Type = NotionPropertyType.date,
        Date = new NotionDate(start, end, timeZone)
    };

    public static NotionProperty OfTitle(string title) => new()
    {
        Type = NotionPropertyType.title,
        Title = new[] { new NotionRichText(title) }
    };

    public static NotionProperty OfRelationship(params string[] ids) => new()
    {
        Type = NotionPropertyType.relation,
        Relation = ids.Select(id => new NotionRelationship(id)).ToArray()
    };

    public static NotionProperty OfCheckbox(bool isChecked) => new()
    {
        Type = NotionPropertyType.checkbox,
        IsChecked = isChecked
    };
}
