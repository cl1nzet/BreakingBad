using System;
using System.Collections.Generic;

namespace Engine.Utils {
    public static class StringUtils {
        public static string[] Split(char separator, string input) {
            if (string.IsNullOrWhiteSpace(input)) return Array.Empty<string>();

            ReadOnlySpan<char> span = input.AsSpan().Trim();

            List<string> output = new(8);

            foreach (var range in span.Split(separator))
            {
                var word = span[range];
                if (!word.IsWhiteSpace())
                {
                    output.Add(word.ToString());
                }
            }

            return output.ToArray();
        }
    }
}