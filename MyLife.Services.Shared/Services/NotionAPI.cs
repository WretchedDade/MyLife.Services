﻿using MyLife.Services.Shared.Models.Notion;
using MyLife.Services.Shared.Models.Notion.Filter;
using MyLife.Services.Shared.Models.Notion.Page;
using MyLife.Services.Shared.Models.Notion.Sort;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyLife.Services.Shared.Services;
public class NotionAPI : INotionAPI
{
    private readonly HttpClient _httpClient;

    public NotionAPI(HttpClient httpClient) => _httpClient = httpClient;

    public void Dispose() => _httpClient.Dispose();

    public async Task<TResult?> GetPage<TResult>(string id) where TResult : NotionPage
    {
        var response = await _httpClient.GetAsync($"v1/pages/{id}");

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TResult>();
    }

    public async Task<IEnumerable<TResult>> QueryDatabase<TResult>(string databaseId, int pageSize = int.MaxValue, string? startCursor = null, NotionFilter? filter = null, NotionSort[]? sorts = null) where TResult : NotionPage
    {
        JsonSerializerOptions serializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        NotionDatabaseQuery query = new()
        {
            PageSize = pageSize,
            StartCursor = startCursor,
            Filter = filter,
            Sorts = sorts
        };

        var body = JsonSerializer.Serialize(query, serializerOptions);

        StringContent content = new(body);
        content.Headers.ContentType = new("application/json");

        var response = await _httpClient.PostAsync($"v1/databases/{databaseId}/query", content);

        response.EnsureSuccessStatusCode();

        var notionList = await response.Content.ReadFromJsonAsync<NotionList<TResult>>();

        if (notionList == null)
            return Enumerable.Empty<TResult>();

        return notionList.Results;

    }

    public async Task<NotionPage> CreatePage(NotionPage page)
    {
        JsonSerializerOptions serializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var body = JsonSerializer.Serialize(page, serializerOptions);

        StringContent content = new(body);
        content.Headers.ContentType = new("application/json");

        var response = await _httpClient.PostAsync($"v1/pages", content);

        response.EnsureSuccessStatusCode();

        var createdPage = await response.Content.ReadFromJsonAsync<NotionPage>();

        return createdPage!;
    }

    public async Task<NotionPage> UpdatePage(string pageId, Dictionary<string, NotionProperty>? propertyUpdates = null, bool? archived = null, NotionIcon? icon = null, NotionCover? cover = null)
    {
        JsonSerializerOptions serializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        NotionPageUpdate pageUpdate = new()
        {
            Properties = propertyUpdates,
            Archived = archived,
            Icon = icon,
            Cover = cover
        };

        var body = JsonSerializer.Serialize(pageUpdate, serializerOptions);

        StringContent content = new(body);
        content.Headers.ContentType = new("application/json");

        var response = await _httpClient.PatchAsync($"v1/pages/{pageId}", content);

        response.EnsureSuccessStatusCode();

        var updatedPage = await response.Content.ReadFromJsonAsync<NotionPage>();

        return updatedPage!;
    }

    public async Task DeletePages(params string[] pageIds)
    {
        await Task.WhenAll(pageIds.Select(id => UpdatePage(id, archived: true)));
    }
}
