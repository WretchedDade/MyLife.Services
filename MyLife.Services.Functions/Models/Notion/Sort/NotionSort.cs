﻿using System;
using System.Text.Json.Serialization;

namespace MyLife.Services.Functions.Models.Notion.Sort;
internal abstract class NotionSort
{
    public NotionSort()
    {

    }

    public NotionSort(NotionSortDirections sortDirection)
    {
        Direction = sortDirection switch
        {
            NotionSortDirections.Ascending => "ascending",
            NotionSortDirections.Descending => "descending",
            _ => throw new NotImplementedException()
        };
    }

    [JsonPropertyName("direction")]
    public string Direction { get; set; } = "ascending";
}
