namespace MyLife.Services.API.Infra;

public class NotionAppSettings
{
    public required string AccessToken { get; set; }
    public required string BillPaymentsDatabaseId { get; set; }
    public required string BillConfigurationDatabaseId { get; set; }
    public required string BudgetDatabaseId { get; set; }
    public required string IncomeConfigurationDatabaseId { get; set; }
    public required string ExpenseConfigurationDatabaseId { get; set; }
}