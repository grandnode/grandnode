using System.Threading.Tasks;

namespace Grand.Framework.Themes
{
    /// <summary>
    /// Work context
    /// </summary>
    public interface IThemeContext
    {
        /// <summary>
        /// Get current theme system name
        /// </summary>
        string WorkingThemeName { get; }

        /// <summary>
        /// Set current theme system name
        /// </summary>
        Task SetWorkingTheme(string themeName);
    }
}
