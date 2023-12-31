﻿using System.Text.Json.Serialization;

namespace MyLife.Services.Shared.Models.Notion.Sort;

public sealed class NotionPropertySort : NotionSort
{
    public NotionPropertySort()
    {

    }

    public NotionPropertySort(string property, NotionSortDirections sortDirection) : base(sortDirection) => Property = property;


    [JsonPropertyName("property")]
    public string Property { get; set; } = string.Empty;
}
