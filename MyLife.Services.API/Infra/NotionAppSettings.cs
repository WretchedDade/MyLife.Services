namespace MyLife.Services.API.Infra;

public class NotionAppSettings
{
    public required string AccessToken { get; set; }
    public required string BillPaymentsDatabaseId { get; set; }
    public required string BillConfigurationDatabaseId { get; set; }
}