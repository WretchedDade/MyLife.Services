using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLife.Services.Functions.Extensions;
internal static class DateTimeExtensions
{
    public static DateTime ToFirstOfMonth(this DateTime dateTime) => new(dateTime.Year, dateTime.Month, 1);

    public static DateTime ToEndOfMonth(this DateTime dateTime) => dateTime.ToFirstOfMonth().AddMonths(1).AddTicks(-1);

}
