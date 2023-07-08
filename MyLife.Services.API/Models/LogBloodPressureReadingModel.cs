using System.ComponentModel.DataAnnotations;

namespace MyLife.Services.API.Models;

public class LogBloodPressureReadingModel
{
    [Required]
    public int Systolic { get; set; }

    [Required]
    public int Diastolic { get; set; }

    [Required]
    public int HeartRate { get; set; }

    [Required]
    public DateTime TimeAtReading { get; set; }
}
