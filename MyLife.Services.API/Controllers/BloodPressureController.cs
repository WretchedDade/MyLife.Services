using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Graph.Models.ODataErrors;
using Microsoft.Identity.Web.Resource;
using MyLife.Services.API.Models;
using MyLife.Services.Shared.Services;

namespace MyLife.Services.API.Controllers;

//[Authorize]
[ApiController]
//[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
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

    [HttpGet("[controller]/Recent", Name = "Get Recent Blood Pressure Reading")]
    public async Task<IActionResult> GetRecentBloodPressureReadings([FromQuery] int count = 5)
    {
        var readings = await _bloodPressureService.GetRecentReadings(count);
        return Ok(readings);
    }


    [HttpPost("[controller]", Name = "Log Blood Pressure Reading")]
    public async Task<IActionResult> LogBloodPressureReading(LogBloodPressureReadingModel model)
    {
        var reading = await _bloodPressureService.CreateReading(model.Systolic, model.Diastolic, model.HeartRate, model.TimeAtReading);

        return CreatedAtAction(nameof(GetBloodPressureReading), new { id = reading.Id }, reading);

    }
}
