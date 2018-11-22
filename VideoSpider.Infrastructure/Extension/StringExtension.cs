using System;

namespace VideoSpider.Infrastructure.Extension
{
    public static class StringExtension
    {
        public static string TrimX(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return "";
            return s.Trim();
        }

        public static DateTime ToDateTime(this string s)
        {
            var result = DateTime.MinValue;
            if (string.IsNullOrEmpty(s))
                return result;
            DateTime.TryParse(s, out result);
            return result;
        }
    }
}
