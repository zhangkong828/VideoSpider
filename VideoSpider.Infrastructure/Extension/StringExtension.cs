using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
