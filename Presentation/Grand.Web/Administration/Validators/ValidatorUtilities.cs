﻿using System.Linq;
using Grand.Admin.Models.Settings;

namespace Grand.Admin.Validators
{
    public class ValidatorUtilities
    {
        public static bool PageSizeOptionsValidator(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return true;
            }
            var notValid = value.Split(',').Select(p => p.Trim()).GroupBy(p => p).Any(p => p.Count() > 1);
            return !notValid;
        }

        public static bool PageSizeOptionsInAdvancedSettingsValidator(SettingModel model, string value)
        {
            if (model.Name.ToLower().Contains("pagesizeoptions"))
            {
                if (string.IsNullOrEmpty(value))
                {
                    return false;
                }

                var notValid = value.Split(',').Select(p => p.Trim()).GroupBy(p => p).Any(p => p.Count() > 1);
                return !notValid;
            }
            return true;
        }
    }
}