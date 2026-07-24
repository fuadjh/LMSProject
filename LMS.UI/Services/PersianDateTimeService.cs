using System.Globalization;

namespace WebUi.Services;

public static class PersianDateTimeService
{
    private static readonly PersianCalendar Calendar = new();

    private static readonly TimeZoneInfo TehranTimeZone =
        ResolveTehranTimeZone();

    public static string Format(DateTime? utcDateTime)
    {
        if (!utcDateTime.HasValue)
            return "-";

        var local = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(
                utcDateTime.Value,
                DateTimeKind.Utc),
            TehranTimeZone);

        return
            $"{Calendar.GetYear(local):0000}/" +
            $"{Calendar.GetMonth(local):00}/" +
            $"{Calendar.GetDayOfMonth(local):00} " +
            $"{local:HH:mm}";
    }

    public static DateTime? ParseToUtc(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var parts = value.Trim()
            .Split(
                ['/', ' ', ':'],
                StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 3)
            return null;

        if (!int.TryParse(parts[0], out var year) ||
            !int.TryParse(parts[1], out var month) ||
            !int.TryParse(parts[2], out var day))
        {
            return null;
        }

        var hour = parts.Length > 3 &&
                   int.TryParse(parts[3], out var parsedHour)
            ? parsedHour
            : 0;

        var minute = parts.Length > 4 &&
                     int.TryParse(parts[4], out var parsedMinute)
            ? parsedMinute
            : 0;

        try
        {
            var local = Calendar.ToDateTime(
                year,
                month,
                day,
                hour,
                minute,
                0,
                0);

            return TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(
                    local,
                    DateTimeKind.Unspecified),
                TehranTimeZone);
        }
        catch
        {
            return null;
        }
    }

    private static TimeZoneInfo ResolveTehranTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(
                "Asia/Tehran");
        }
        catch
        {
            return TimeZoneInfo.FindSystemTimeZoneById(
                "Iran Standard Time");
        }
    }
}