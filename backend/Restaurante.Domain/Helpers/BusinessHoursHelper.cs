using Restaurante.Domain.Entities;

namespace Restaurante.Domain.Helpers;

/// <summary>
/// Pure business-hours logic shared by later phases (delivery windows, coupons, etc.).
///
/// Contract:
/// - No configured hours at all (null or empty list) => null ("not applicable").
/// - A day with no configuration => null (the schedule does not cover it).
/// - Day marked IsClosed => false.
/// - Overnight schedules (CloseTime &lt;= OpenTime) cross midnight: the venue is open
///   when time >= OpenTime OR time < CloseTime.
/// - Otherwise open when OpenTime &lt;= time &lt; CloseTime.
/// </summary>
public static class BusinessHoursHelper
{
    public static bool HasHours(IEnumerable<BusinessHour>? hours) =>
        hours is not null && hours.Any();

    public static bool? IsOpenNow(IEnumerable<BusinessHour>? hours, DateTime now)
    {
        if (!HasHours(hours))
            return null;

        return IsOpenOn(hours, now.DayOfWeek, now.TimeOfDay);
    }

    public static bool? IsOpenOn(IEnumerable<BusinessHour>? hours, DayOfWeek dayOfWeek, TimeSpan time)
    {
        if (!HasHours(hours))
            return null;

        var hour = hours!.FirstOrDefault(h => h.DayOfWeek == (int)dayOfWeek);
        if (hour is null)
            return null;

        if (hour.IsClosed)
            return false;

        if (hour.CloseTime <= hour.OpenTime)
            return time >= hour.OpenTime || time < hour.CloseTime;

        return time >= hour.OpenTime && time < hour.CloseTime;
    }
}
