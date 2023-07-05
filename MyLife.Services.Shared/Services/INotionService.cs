using MyLife.Services.Shared.Models;

namespace MyLife.Services.Shared.Services;

public interface INotionService
{
    Task<BillPayment> MarkBillAsPaid(string id);
}
