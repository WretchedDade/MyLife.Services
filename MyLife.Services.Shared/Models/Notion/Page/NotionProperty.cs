using System.Text.Json.Serialization;

namespace MyLife.Services.Shared.Models.Notion.Page;

public class NotionProperty
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
    public NotionRelationship[]? Relationships { get; set; }

    [JsonPropertyName("rich_text")]
    public NotionRichText[]? RichText { get; set; }

    [JsonPropertyName("rollup")]
    public NotionRollup? Rollup { get; set; }

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
        Relationships = ids.Select(id => new NotionRelationship(id)).ToArray()
    };

    public static NotionProperty OfCheckbox(bool isChecked) => new()
    {
        Type = NotionPropertyType.checkbox,
        IsChecked = isChecked
    };
}

public class NotionRollup
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("function")]
    public string Function { get; set; }

    [JsonPropertyName("array")]
    public NotionProperty[]? Array { get; set; }
}

public static class NotionRollupTypes
{
    public const string Array = "array";
    public const string Date = "date";
    public const string Incomplete = "incomplete";
    public const string Number = "number";
    public const string Unsupported = "unsupported";
}

public static class NotionRollupFunctions
{
    public const string Average = "average";
    public const string Checked = "checked";
    public const string Count = "count";
    public const string CountPerGroup = "count_per_group";
    public const string CountValues = "count_values";
    public const string DateRange = "date_range";
    public const string EarliestDate = "earliest_date";
    public const string Empty = "empty";
    public const string LatestDate = "latest_date";
    public const string Max = "max";
    public const string Median = "median";
    public const string Min = "min";
    public const string NotEmpty = "not_empty";
    public const string PercentChecked = "percent_checked";
    public const string PercentEmpty = "percent_empty";
    public const string PercentNotEmpty = "percent_not_empty";
    public const string PercentPerGroup = "percent_per_group";
    public const string PercentUnched = "percent_unchecked";
    public const string Range = "range";
    public const string ShowOriginal = "show_original";
    public const string ShowUnique = "show_unique";
    public const string Sum = "sum";
    public const string Unchecked = "unchecked";
    public const string Unique = "unique";
}