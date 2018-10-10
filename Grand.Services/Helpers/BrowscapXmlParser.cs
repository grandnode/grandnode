using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Grand.Services.Helpers
{
    /// <summary>
    /// Helper class for working with XML file of Browser Capabilities Project (http://browscap.org/)
    /// </summary>
    public class BrowscapXmlHelper
    {
        private readonly List<string> _crawlerUserAgentsRegexp;
        private readonly List<string> _crawlerUserAgents;
        private readonly List<string> _notCrawlerUserAgents;

        public BrowscapXmlHelper(string filePath)
        {
            _crawlerUserAgentsRegexp = new List<string>();
            _crawlerUserAgents = new List<string>();
            _notCrawlerUserAgents = new List<string>();

            Initialize(filePath);
        }

        private void Initialize(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open))
            using (var sr = new StreamReader(stream))
            {
                var browsercapItems = XDocument.Load(sr).Element("browsercapitems").Elements("browscapitem").Select(x => x.Attribute("name").Value).ToList();
                if (browsercapItems == null)
                    throw new Exception("Incorrect file format");

                _crawlerUserAgentsRegexp.AddRange(browsercapItems.Select(ToRegexp));
            }
        }

        private static string ToRegexp(string str)
        {
            var sb = new StringBuilder(Regex.Escape(str));
            sb.Replace("&amp;", "&").Replace("\\?", ".").Replace("\\*", ".*?");
            return string.Format("^{0}$", sb);
        }

        /// <summary>
        /// Determines whether a user agent is a crawler
        /// </summary>
        /// <param name="userAgent">User agent string</param>
        /// <returns>True if user agent is a crawler, otherwise - false</returns>
        public bool IsCrawler(string userAgent)
        {
            if (_crawlerUserAgents.Any(p => p == userAgent))
                return true;

            if (_notCrawlerUserAgents.Any(p => p == userAgent))
                return false;

            var flag = _crawlerUserAgentsRegexp.Any(p => Regex.IsMatch(userAgent, p));
            if (flag)
                _crawlerUserAgents.Add(userAgent);
            else
                _notCrawlerUserAgents.Add(userAgent);

            return flag;
        }
    }
}