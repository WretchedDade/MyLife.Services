using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using MyLife.Services.API.Models;
using MyLife.Services.Shared.Services;
using System.ComponentModel.DataAnnotations;

namespace MyLife.Services.API.Controllers.v1;

[Authorize]
[ApiController]
[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
public class BloodPressureController : MyLifeController
{
    private readonly IBloodPressureService _bloodPressureService;

    public BloodPressureController(IConfiguration configuration, IBloodPressureService bloodPressureService)
        : base(configuration)
    {
        _bloodPressureService = bloodPressureService;
    }

    [HttpGet("[controller]/{id}", Name = "Get Blood Pressure Reading")]
    public async Task<IActionResult> GetBloodPressureReading(string id)
    {
        var reading = await _bloodPressureService.GetById(id);

        return Ok(reading);
    }

    [HttpGet("[controller]", Name = "Get Blood Pressure Readings")]
    public async Task<IActionResult> GetBloodPressureReadings(
        [FromQuery][Range(0, int.MaxValue)] int pageNumber = 0,
        [FromQuery][Range(1, int.MaxValue)] int pageSize = 25
    )
    {
        var totalCount = await _bloodPressureService.Count();

        var readings = await _bloodPressureService.GetReadings(
            skip: pageNumber * pageSize,
            take: pageSize
        );

        Page<BloodPressureReading> page = new(
            pageNumber: pageNumber,
            pageSize: pageSize,
            totalCount: totalCount,
            items: readings
        );

        return Ok(page);
    }

    [HttpPost("[controller]", Name = "Log Blood Pressure Reading")]
    public async Task<IActionResult> LogBloodPressureReading(LogBloodPressureReadingModel model)
    {
        var reading = await _bloodPressureService.CreateReading(model.Systolic, model.Diastolic, model.HeartRate, model.TimeAtReading);

        return CreatedAtAction(nameof(GetBloodPressureReading), new { id = reading.Id }, reading);
    }

    [HttpPut("[controller]/{id}", Name = "Update Blood Pressure Reading")]
    public async Task<IActionResult> UpdateBloodPressureReading(string id, LogBloodPressureReadingModel model)
    {
        var reading = await _bloodPressureService.UpdateReading(id, model.Systolic, model.Diastolic, model.HeartRate, model.TimeAtReading);

        return Ok(reading);
    }

    [HttpDelete("[controller]/{id}", Name = "Delete Blood Pressure Reading")]
    public async Task<IActionResult> DeleteBloodPressureReading(string id)
    {
        await _bloodPressureService.DeleteReading(id);

        return Ok();
    }
}
