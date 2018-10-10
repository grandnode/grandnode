using Grand.Core.Configuration;

namespace Grand.Core.Domain.Common
{
    public class AdminAreaSettings : ISettings
    {
        /// <summary>
        /// Default grid page size
        /// </summary>
        public int DefaultGridPageSize { get; set; }
        /// <summary>
        /// A comma-separated list of available grid page sizes
        /// </summary>
        public string GridPageSizes { get; set; }
        /// <summary>
        /// Additional settings for rich editor
        /// </summary>
        public string RichEditorAdditionalSettings { get; set; }
        /// <summary>
        ///A value indicating whether to javascript is supported in rcih editor
        /// </summary>
        public bool RichEditorAllowJavaScript { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use IsoDateTimeConverter in Json results (used for avoiding issue with dates in KendoUI grids)
        /// </summary>
        public bool UseIsoDateTimeConverterInJson { get; set; }

        /// <summary>
        /// Gets or sets a value for admin layout
        /// </summary>
        public string AdminLayout { get; set; }

        /// <summary>
        /// Gets or sets a value for admin grid layout
        /// </summary>
        public string KendoLayout { get; set; }
    }
}