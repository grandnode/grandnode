namespace Grand.Core.Plugins
{
    /// <summary>
    /// Represents a theme descriptor
    /// </summary>
    public class ThemeDescriptor : IDescriptor
    {
        /// <summary>
        /// Gets or sets the theme system name
        /// </summary>
        public string SystemName { get; set; }

        /// <summary>
        /// Gets or sets the theme friendly name
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the theme supports RTL (right-to-left)
        /// </summary>
        public bool SupportRtl { get; set; }

        /// <summary>
        /// Gets or sets the path to the preview image of the theme
        /// </summary>
        public string PreviewImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the preview text of the theme
        /// </summary>
        public string PreviewText { get; set; }
    }
}