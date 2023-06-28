using MyLife.Services.Shared.Models.Notion;
using MyLife.Services.Shared.Models.Notion.Filter;
using MyLife.Services.Shared.Models.Notion.Page;
using MyLife.Services.Shared.Models.Notion.Sort;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyLife.Services.Shared.Services;

public interface INotionAPI : IDisposable
{
    Task<NotionPage> CreatePage(NotionPage page);
    Task<NotionList<TResult>> QueryDatabase<TResult>(string databaseId, int pageSize = 10, string? startCursor = null, NotionFilter? filter = null, NotionSort[]? sorts = null) where TResult : NotionPage;
    Task<NotionPage> UpdatePage(string pageId, Dictionary<string, NotionProperty>? propertyUpdates = null, bool? archived = null, NotionIcon? icon = null, NotionCover? cover = null);
}