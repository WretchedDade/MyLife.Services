using Microsoft.AspNetCore.Mvc;
using MyLife.Services.Functions.Models.Notion;
using MyLife.Services.Functions.Models.Notion.Filter;
using MyLife.Services.Functions.Models.Notion.Page;
using MyLife.Services.Functions.Models.Notion.Sort;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MyLife.Services.Functions;
internal class NotionAPI : IDisposable
{
    private readonly HttpClient _httpClient;

    public NotionAPI()
    {
        _httpClient = new()
        {
            BaseAddress = new Uri("https://api.notion.com"),
        };

        _httpClient.DefaultRequestHeaders.Authorization = new("Bearer", FunctionHelpers.GetEnvironmentVariable(EnvironmentVariables.NotionAccessToken));
        _httpClient.DefaultRequestHeaders.Add("Notion-Version", "2022-02-22");
    }

    public void Dispose() => _httpClient.Dispose();

    internal async Task<NotionList<TResult>> QueryDatabase<TResult>(string databaseId, int pageSize = 10, string? startCursor = null, NotionFilter? filter = null, NotionSort[]? sorts = null) where TResult : NotionPage
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

        return notionList ?? new NotionList<TResult>();
    }

    internal async Task<NotionPage> CreatePage(NotionPage page)
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

    internal async Task<NotionPage> UpdatePage(string pageId, Dictionary<string, NotionProperty>? propertyUpdates = null, bool? archived = null, NotionIcon? icon = null, NotionCover? cover = null)
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
}
