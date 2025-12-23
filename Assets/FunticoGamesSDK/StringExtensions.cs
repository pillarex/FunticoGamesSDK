using System;
using System.Text;

namespace FunticoGamesSDK
{
    public static class StringExtensions
    {
        public static bool IsUrl(this string str) => Uri.IsWellFormedUriString(str, UriKind.Absolute);

        public static string ToStringWithSpaces<T>(this T value) where T : Enum
        {
            var stringVal = value.ToString();
            var bld = new StringBuilder();
            var first = true;
            foreach (var symbol in stringVal)
            {
                if (char.IsUpper(symbol))
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        bld.Append(" ");
                    }
                }

                bld.Append(symbol);
            }

            return bld.ToString();
        }
        
        public static bool IsNullOrWhitespace(this string str) => string.IsNullOrWhiteSpace(str);
    }
}