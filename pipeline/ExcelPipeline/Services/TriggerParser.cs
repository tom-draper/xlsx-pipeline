using System.Text.RegularExpressions;

namespace ExcelPipeline.Services
{
    public interface ITriggerParser
    {
        bool TryParseDateTime(string dateTimeString, out DateTime dateTime);
        bool TryParseTimeOfDay(string timeString, out TimeSpan timeOfDay);
        bool TryParseInterval(string intervalString, out TimeSpan interval);
        bool IsCronExpression(string expression);
        bool TryParseCronExpression(string cronExpression, out DateTime nextRunTime);
        DateTime GetNextRunTimeForDaily(TimeSpan timeOfDay);
    }

    public partial class TriggerParser : ITriggerParser
    {
        public bool TryParseDateTime(string dateTimeString, out DateTime dateTime)
        {
            dateTime = default;
            dateTimeString = dateTimeString.Trim();

            // Try to parse "YYYY-MM-DD HH:MM" format
            var dateTimeRegex = DateTimeRegex();
            var match = dateTimeRegex.Match(dateTimeString);

            if (match.Success)
            {
                if (int.TryParse(match.Groups[1].Value, out int year) &&
                    int.TryParse(match.Groups[2].Value, out int month) &&
                    int.TryParse(match.Groups[3].Value, out int day) &&
                    int.TryParse(match.Groups[4].Value, out int hour) &&
                    int.TryParse(match.Groups[5].Value, out int minute))
                {
                    try
                    {
                        dateTime = new DateTime(year, month, day, hour, minute, 0);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            // Try to parse "YYYY-MM-DD" format (default to current time)
            var dateRegex = DateRegex();
            match = dateRegex.Match(dateTimeString);

            if (match.Success)
            {
                if (int.TryParse(match.Groups[1].Value, out int year) &&
                    int.TryParse(match.Groups[2].Value, out int month) &&
                    int.TryParse(match.Groups[3].Value, out int day))
                {
                    try
                    {
                        var now = DateTime.Now;
                        dateTime = new DateTime(year, month, day, now.Hour, now.Minute, now.Second);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        public bool TryParseTimeOfDay(string timeString, out TimeSpan timeOfDay)
        {
            timeOfDay = default;
            timeString = timeString.Trim().ToLowerInvariant();

            // Handle formats like "6pm", "14:30", "9:15am"
            var timeRegex = TimeRegex();
            var match = timeRegex.Match(timeString);

            if (!match.Success)
                return false;

            if (!int.TryParse(match.Groups[1].Value, out int hours))
                return false;

            int minutes = 0;
            if (match.Groups[2].Success && !int.TryParse(match.Groups[2].Value, out minutes))
                return false;

            var ampm = match.Groups[3].Value;

            if (!string.IsNullOrEmpty(ampm))
            {
                if (ampm == "pm" && hours != 12)
                    hours += 12;
                else if (ampm == "am" && hours == 12)
                    hours = 0;
            }

            if (hours < 0 || hours > 23 || minutes < 0 || minutes > 59)
                return false;

            timeOfDay = new TimeSpan(hours, minutes, 0);
            return true;
        }

        public bool TryParseInterval(string intervalString, out TimeSpan interval)
        {
            interval = default;
            intervalString = intervalString.Trim().ToLowerInvariant();

            // Handle formats like "5 minutes", "2 hours", "1 day"
            var intervalRegex = IntervalRegex();
            var match = intervalRegex.Match(intervalString);

            if (!match.Success)
                return false;

            if (!int.TryParse(match.Groups[1].Value, out int value))
                return false;

            var unit = match.Groups[2].Value;

            interval = unit switch
            {
                "second" => TimeSpan.FromSeconds(value),
                "minute" => TimeSpan.FromMinutes(value),
                "hour" => TimeSpan.FromHours(value),
                "day" => TimeSpan.FromDays(value),
                _ => default
            };

            return interval != default;
        }

        public bool IsCronExpression(string expression)
        {
            // Basic check for cron expression format (5 or 6 parts separated by spaces)
            var parts = expression.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 5 && parts.Length <= 6;
        }

        public bool TryParseCronExpression(string cronExpression, out DateTime nextRunTime)
        {
            nextRunTime = default;

            try
            {
                var parts = cronExpression.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 5 || parts.Length > 6)
                    return false;

                // Parse cron parts (supports both 5 and 6 field formats)
                var secondsPart = parts.Length == 6 ? parts[0] : "0";
                var minutesPart = parts.Length == 6 ? parts[1] : parts[0];
                var hoursPart = parts.Length == 6 ? parts[2] : parts[1];
                var daysPart = parts.Length == 6 ? parts[3] : parts[2];
                var monthsPart = parts.Length == 6 ? parts[4] : parts[3];
                var dayOfWeekPart = parts.Length == 6 ? parts[5] : parts[4];

                var now = DateTime.Now;

                // Simple cron parser - find next valid time within the next 7 days
                for (int dayOffset = 0; dayOffset < 7; dayOffset++)
                {
                    var checkDate = now.Date.AddDays(dayOffset);

                    // Check if day of week matches
                    if (!IsValidDayOfWeek(checkDate.DayOfWeek, dayOfWeekPart))
                        continue;

                    // Check if month matches
                    if (!IsValidMonth(checkDate.Month, monthsPart))
                        continue;

                    // Check if day matches
                    if (!IsValidDay(checkDate.Day, daysPart))
                        continue;

                    // Find valid hours and minutes for this day
                    for (int hour = 0; hour < 24; hour++)
                    {
                        if (!IsValidHour(hour, hoursPart))
                            continue;

                        for (int minute = 0; minute < 60; minute++)
                        {
                            if (!IsValidMinute(minute, minutesPart))
                                continue;

                            var candidateTime = new DateTime(checkDate.Year, checkDate.Month, checkDate.Day, hour, minute, 0);
                            if (candidateTime > now)
                            {
                                nextRunTime = candidateTime;
                                return true;
                            }
                        }
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public DateTime GetNextRunTimeForDaily(TimeSpan timeOfDay)
        {
            var now = DateTime.Now;
            var today = now.Date.Add(timeOfDay);

            // If the time has already passed today, schedule for tomorrow
            return today > now ? today : today.AddDays(1);
        }

        private static bool IsValidDayOfWeek(DayOfWeek dayOfWeek, string cronPart)
        {
            if (cronPart == "*")
                return true;

            var dayNumber = (int)dayOfWeek; // 0 = Sunday, 1 = Monday, etc.

            // Handle ranges like MON-FRI
            if (cronPart.Contains('-'))
            {
                var range = cronPart.Split('-');
                if (range.Length == 2)
                {
                    var start = ParseDayOfWeek(range[0]);
                    var end = ParseDayOfWeek(range[1]);
                    if (start != -1 && end != -1)
                    {
                        return dayNumber >= start && dayNumber <= end;
                    }
                }
            }

            // Handle specific days
            if (int.TryParse(cronPart, out int specificDay))
            {
                return dayNumber == specificDay;
            }

            // Handle named days
            return dayNumber == ParseDayOfWeek(cronPart);
        }

        private static int ParseDayOfWeek(string dayName)
        {
            return dayName.ToUpperInvariant() switch
            {
                "SUN" or "SUNDAY" => 0,
                "MON" or "MONDAY" => 1,
                "TUE" or "TUESDAY" => 2,
                "WED" or "WEDNESDAY" => 3,
                "THU" or "THURSDAY" => 4,
                "FRI" or "FRIDAY" => 5,
                "SAT" or "SATURDAY" => 6,
                _ => int.TryParse(dayName, out int day) ? day : -1
            };
        }

        private static bool IsValidMonth(int month, string cronPart)
        {
            if (cronPart == "*")
                return true;

            if (int.TryParse(cronPart, out int specificMonth))
            {
                return month == specificMonth;
            }

            return false;
        }

        private static bool IsValidDay(int day, string cronPart)
        {
            if (cronPart == "*")
                return true;

            if (int.TryParse(cronPart, out int specificDay))
            {
                return day == specificDay;
            }

            return false;
        }

        private static bool IsValidHour(int hour, string cronPart)
        {
            if (cronPart == "*")
                return true;

            // Handle ranges like 9-17
            if (cronPart.Contains("-"))
            {
                var range = cronPart.Split('-');
                if (range.Length == 2 && int.TryParse(range[0], out int start) && int.TryParse(range[1], out int end))
                {
                    return hour >= start && hour <= end;
                }
            }

            if (int.TryParse(cronPart, out int specificHour))
            {
                return hour == specificHour;
            }

            return false;
        }

        private static bool IsValidMinute(int minute, string cronPart)
        {
            if (cronPart == "*")
                return true;

            // Handle step values like */15
            if (cronPart.StartsWith("*/"))
            {
                if (int.TryParse(cronPart.AsSpan(2), out int step))
                {
                    return minute % step == 0;
                }
            }

            if (int.TryParse(cronPart, out int specificMinute))
            {
                return minute == specificMinute;
            }

            return false;
        }

        [GeneratedRegex(@"^(\d{4})-(\d{1,2})-(\d{1,2})\s+(\d{1,2}):(\d{2})$")]
        private static partial Regex DateTimeRegex();

        [GeneratedRegex(@"^(\d{4})-(\d{1,2})-(\d{1,2})$")]
        private static partial Regex DateRegex();

        [GeneratedRegex(@"^(\d{1,2})(?::(\d{2}))?\s*(am|pm)?$")]
        private static partial Regex TimeRegex();

        [GeneratedRegex(@"^(\d+)\s*(second|minute|hour|day)s?$")]
        private static partial Regex IntervalRegex();
    }
}