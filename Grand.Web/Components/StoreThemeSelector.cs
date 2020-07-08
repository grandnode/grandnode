using Grand.Domain.Stores;
using Grand.Framework.Components;
using Grand.Framework.Themes;
using Grand.Web.Models.Common;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Grand.Web.ViewComponents
{
    public class StoreThemeSelectorViewComponent : BaseViewComponent
    {
        private readonly IThemeProvider _themeProvider;
        private readonly IThemeContext _themeContext;

        private readonly StoreInformationSettings _storeInformationSettings;
        public StoreThemeSelectorViewComponent(IThemeProvider themeProvider,
            IThemeContext themeContext,
            StoreInformationSettings storeInformationSettings)
        {
            _themeProvider = themeProvider;
            _themeContext = themeContext;
            _storeInformationSettings = storeInformationSettings;
        }

        public IViewComponentResult Invoke()
        {
            if (!_storeInformationSettings.AllowCustomerToSelectTheme)
                return Content("");

            var model = PrepareStoreThemeSelector();
            return View(model);
        }

        private StoreThemeSelectorModel PrepareStoreThemeSelector()
        {
            var model = new StoreThemeSelectorModel();

            var currentTheme = _themeProvider.GetThemeConfiguration(_themeContext.WorkingThemeName);
            model.CurrentStoreTheme = new StoreThemeModel {
                Name = currentTheme.ThemeName,
                Title = currentTheme.ThemeTitle
            };
            model.AvailableStoreThemes = _themeProvider.GetThemeConfigurations()
                .Select(x => new StoreThemeModel {
                    Name = x.ThemeName,
                    Title = x.ThemeTitle
                })
                .ToList();
            return model;
        }

    }
}