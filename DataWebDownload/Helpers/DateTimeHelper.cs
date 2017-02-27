using System;

namespace DataWebDownload.Helpers
{
    static class DateTimeHelper
    {
        public static int EpochTime(this DateTime date)
        {
            return (int)(date - new DateTime(1970, 1, 1)).TotalSeconds;
        }
    }
}