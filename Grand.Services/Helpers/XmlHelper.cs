using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Grand.Services.Helpers
{
    /// <summary>
    /// Xml helper class
    /// </summary>
    public static class XmlHelper
    {
        #region Methods

        /// <summary>
        /// XML Encode
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Encoded string</returns>
        public static string XmlEncode(string str)
        {
            if (str == null)
                return null;
            str = Regex.Replace(str, @"[^\u0009\u000A\u000D\u0020-\uD7FF\uE000-\uFFFD]", "", RegexOptions.Compiled);
            return XmlEncodeAsIs(str);
        }

        /// <summary>
        /// XML Encode as is
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Encoded string</returns>
        public static string XmlEncodeAsIs(string str)
        {
            if (str == null)
                return null;

            var xwSettings = new XmlWriterSettings {
                ConformanceLevel = ConformanceLevel.Auto
            };
            using (var sw = new StringWriter())
            using (var xwr = XmlWriter.Create(sw, xwSettings))
            {
                xwr.WriteString(str);
                xwr.Flush();
                return sw.ToString();
            }
        }

        /// <summary>
        /// Decodes an attribute
        /// </summary>
        /// <param name="str">Attribute</param>
        /// <returns>Decoded attribute</returns>
        public static string XmlDecode(string str)
        {
            var sb = new StringBuilder(str);
            return sb.Replace("&quot;", "\"").Replace("&apos;", "'").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&").ToString();
        }


        #endregion
    }
}
