using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MyLife.Services.Functions.Models.Notion.Page;
internal class NotionDate
{
    public NotionDate()
    {
    }

    public NotionDate(string start, string? end = null, string? timeZone = null)
    {
        Start = start;
        End = end;
        TimeZone = timeZone;
    }

    [JsonPropertyName("start")]
    public string Start { get; set; }

    [JsonPropertyName("end")]
    public string? End { get; set; }

    [JsonPropertyName("time_zone")]
    public string? TimeZone { get; set; }

    [JsonIgnore]
    public DateTime StartDate => DateTime.Parse(Start);

    [JsonIgnore] 
    public DateTime? EndDate => End == null ? null : DateTime.Parse(End);
}
