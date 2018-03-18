using System;

namespace NDecred.Common
{
    public class DateTimeUtil
    {
        public static DateTime FromUnixTime(double timestamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp);;
        }
    }
}