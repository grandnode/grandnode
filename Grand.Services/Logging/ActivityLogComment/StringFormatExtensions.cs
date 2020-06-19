using System.Text.RegularExpressions;

namespace Grand.Services.Logging.ActivityLogComment
{
    /// <summary>
    /// String format extensions
    /// </summary>
    public static class StringFormatExtensions
    {
        /// <summary>
        /// Parse already formatted sting data using pre-defined format
        /// </summary>
        /// <param name="data"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string[] ParseExact(this string data, string format)
        {
            if (TryParseExact(data, format, out string[] values))
                return values;
            else
                return new string[0];
        }

        private static bool TryParseExact(
            this string data,
            string format,
            out string[] values)
        {
            int tokenCount = 0;
            format = Regex.Escape(format).Replace("\\{", "{");

            for (tokenCount = 0; ; tokenCount++)
            {
                string token = string.Format("{{{0}}}", tokenCount);
                if (!format.Contains(token)) break;
                format = format.Replace(token, string.Format("(?'group{0}'.*)", tokenCount));
            }

            Match match = new Regex(format).Match(data);

            if (tokenCount != (match.Groups.Count - 1))
            {
                values = new string[] { };
                return false;
            }
            else
            {
                values = new string[tokenCount];
                for (int index = 0; index < tokenCount; index++)
                    values[index] = match.Groups[string.Format("group{0}", index)].Value;
                return true;
            }
        }
    }
}
