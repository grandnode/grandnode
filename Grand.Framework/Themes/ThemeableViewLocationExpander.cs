using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Grand.Framework.Themes
{
    public class ThemeableViewLocationExpander : IViewLocationExpander
    {
        private const string THEME_KEY = "Theme";
        private const string AREA_ADMIN_KEY = "AdminTheme";


        public void PopulateValues(ViewLocationExpanderContext context)
        {
            var themeContext = (IThemeContext)context.ActionContext.HttpContext.RequestServices.GetService(typeof(IThemeContext));

            if (context.AreaName?.Equals("Admin") ?? false)
                context.Values[AREA_ADMIN_KEY] = themeContext.AdminAreaThemeName;
            else
                context.Values[THEME_KEY] = themeContext.WorkingThemeName;
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (context.AreaName == null && context.Values.TryGetValue(THEME_KEY, out var theme))
            {
                viewLocations = new[] {
                        $"/Themes/{theme}/Views/{{1}}/{{0}}.cshtml",
                        $"/Themes/{theme}/Views/Shared/{{0}}.cshtml",
                    }
                    .Concat(viewLocations);
            }
            if ((context.AreaName?.Equals("Admin") ?? false) && context.Values.TryGetValue(AREA_ADMIN_KEY, out var admintheme))
            {
                viewLocations = new[] {
                        $"/Areas/{{2}}/Themes/{admintheme}/Views/{{1}}/{{0}}.cshtml",
                        $"/Areas/{{2}}/Themes/{admintheme}/Views/Shared/{{0}}.cshtml",
                    }
                    .Concat(viewLocations);
            }
            return viewLocations;
        }
    }
}
