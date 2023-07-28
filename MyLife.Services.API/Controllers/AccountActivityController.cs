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
public class AccountActivityController : MyLifeController
{
    private readonly IAccountActivityService _accountActivityService;

    public AccountActivityController(IConfiguration configuration, IAccountActivityService accountActivityService) : base(configuration)
    {
        _accountActivityService = accountActivityService;
    }

    [HttpGet("AccountActivity/{id}", Name = "Get an Account Activity Item")]
    public async Task<IActionResult> Get(string id)
    {
        var result = await _accountActivityService.Get(id);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("AccountActivity", Name = "Get Account Activity")]
    public async Task<IActionResult> Get(
        [FromQuery][Range(0, int.MaxValue)] int pageNumber = 0,
        [FromQuery][Range(1, int.MaxValue)] int? pageSize = null,
        [FromQuery] string? category = null
    )
    {
        var totalCount = await _accountActivityService.Count();

        var items = await _accountActivityService.Get(pageNumber, pageSize, category);

        Page<AccountActivityItem> page = new(
            pageNumber: pageNumber,
            pageSize: pageSize,
            totalCount: totalCount,
            items: items
        );

        return Ok(page);
    }

    [HttpGet("AccountActivity/{year}/{month}", Name = "Get Account Activity for Specific Month")]
    public async Task<IActionResult> Get(
        int year,
        int month,
        [FromQuery] int pageNumber = 0,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? category = null
    )
    {
        var totalCount = await _accountActivityService.Count(year, month);

        var items = string.IsNullOrEmpty(category) 
            ? await _accountActivityService.Get(year, month, pageNumber, pageSize) 
            : await _accountActivityService.Get(year, month, category, pageNumber, pageSize);

        Page<AccountActivityItem> page = new(
            pageNumber: pageNumber,
            pageSize: pageSize,
            totalCount: totalCount,
            items: items
        );

        return Ok(page);
    }

    [HttpPut("AccountActivity/{id}", Name = "Update an Account Activity Item")]
    public async Task<IActionResult> Update(string id, UpdateAccountActivityModel model)
    {
        var result = await _accountActivityService.Update(id, model.Name, model.Category);

        return Ok(result);
    }

    [HttpDelete("AccountActivity/{id}", Name = "Delete an Account Activity Item")]
    public async Task<IActionResult> Delete(string id)
    {
        await _accountActivityService.Delete(id);

        return Ok();
    }

    public class UpdateAccountActivityModel
    {
        [Required(AllowEmptyStrings = false)]
        public required string Name { get; set; }

        [Required(AllowEmptyStrings = false)]
        public required string Category { get; set; }
    }
}
