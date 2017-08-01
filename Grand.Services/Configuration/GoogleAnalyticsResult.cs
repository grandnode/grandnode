using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grand.Services.Configuration
{
    public partial class GoogleAnalyticsResult
    {
        public GoogleAnalyticsResult()
        {
            Records = new List<Dictionary<string, string>>();
        }

        /// <summary>
        /// Key: metric/dimension header,
        /// Value: metric/dimension value
        /// </summary>
        public List<Dictionary<string, string>> Records { get; private set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Message { get; set; }
    }
}
