
namespace Grand.Core.Plugins
{
    /// <summary>
    /// Theme uploaded event
    /// </summary>
    public class ThemeUploadedEvent
    {
        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="uploadedTheme">Uploaded themes</param>
        public ThemeUploadedEvent(ThemeDescriptor uploadedTheme)
        {
            this.UploadedTheme = uploadedTheme;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Uploaded theme
        /// </summary>
        public ThemeDescriptor UploadedTheme { get; private set; }

        #endregion
    }
}