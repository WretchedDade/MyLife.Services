using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using MyLife.Services.API.Models;
using MyLife.Services.Shared.Services;
using System.ComponentModel.DataAnnotations;

namespace MyLife.Services.API.Controllers;

[Authorize]
[ApiController]
[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
public class BankKeywordConfigController : MyLifeController
{
    private readonly IBankKeywordConfigService _bankKeywordConfigService;

    public BankKeywordConfigController(IConfiguration configuration, IBankKeywordConfigService bankKeywordConfigService) : base(configuration)
    {
        _bankKeywordConfigService = bankKeywordConfigService;
    }

    [HttpGet("AccountActivity/Config/Keywords/{keyword}", Name = "Get Keyword Config")]
    public async Task<IActionResult> Get(string keyword)
    {
        var result = await _bankKeywordConfigService.Get(keyword);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("AccountActivity/Config/Keywords", Name = "Get Keyword Configs")]
    public async Task<IActionResult> Get(
        [FromQuery][Range(0, int.MaxValue)] int pageNumber = 0,
        [FromQuery][Range(1, int.MaxValue)] int? pageSize = null
    )
    {
        var totalCount = await _bankKeywordConfigService.Count();

        var items = await _bankKeywordConfigService.Get(pageNumber, pageSize);

        Page<BankKeyword> page = new(
            pageNumber: pageNumber,
            pageSize: pageSize,
            totalCount: totalCount,
            items: items
        );

        return Ok(page);
    }

    [HttpPost("AccountActivity/Config/Keywords", Name = "Create Keyword Config")]
    public async Task<IActionResult> Create(CreateKeywordConfigModel model)
    {
        var result = await _bankKeywordConfigService.Create(model.Keyword, model.Name, model.Category);

        return CreatedAtAction(nameof(Get), new { keyword = result.Keyword }, result);
    }

    [HttpPut("AccountActivity/Config/Keywords/{keyword}", Name = "Update Keyword Config")]
    public async Task<IActionResult> Update(string keyword, UpdateKeywordConfigModel model)
    {
        var result = await _bankKeywordConfigService.Update(keyword, model.Name, model.Category);

        return Ok(result);
    }

    [HttpDelete("AccountActivity/Config/Keywords/{keyword}", Name = "Delete Keyword Config")]
    public async Task<IActionResult> Delete(string keyword)
    {
        await _bankKeywordConfigService.Delete(keyword);

        return Ok();
    }

    public class CreateKeywordConfigModel : UpdateKeywordConfigModel
    {
        [Required(AllowEmptyStrings = false)]
        public required string Keyword { get; set; }
    }

    public class UpdateKeywordConfigModel
    {
        [Required(AllowEmptyStrings = false)]
        public required string Name { get; set; }

        [Required(AllowEmptyStrings = false)]
        public required string Category { get; set; }
    }
}
