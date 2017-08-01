using System.Collections.Generic;

namespace Grand.Framework.Events
{
    /// <summary>
    /// Product search event
    /// </summary>
    public class ProductSearchEvent
    {
        public string SearchTerm { get; set; }
        public bool SearchInDescriptions { get; set; }
        public IList<string> CategoryIds { get; set; }
        public string ManufacturerId { get; set; }
        public string WorkingLanguageId { get; set; }
        public string VendorId { get; set; }
    }
}
