using Grand.Core;
using Grand.Core.Infrastructure;
using Grand.Framework.Kendoui;
using Grand.Services.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Grand.Framework.Extensions
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class CommonExtensions
    {
        public static IEnumerable<T> PagedForCommand<T>(this IEnumerable<T> current, DataSourceRequest command)
        {
            return current.Skip((command.Page - 1) * command.PageSize).Take(command.PageSize);
        }
        public static IEnumerable<T> PagedForCommand<T>(this IEnumerable<T> current, int pageIndex, int pageSize)
        {
            return current.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }
        public static SelectList ToSelectList<TEnum>(this TEnum enumObj
            , bool markCurrentAsSelected = true
            , int[] valuesToExclude = null
            , bool useLocaliztion = true) where TEnum : struct
        {
            if (!typeof(TEnum).GetTypeInfo().IsEnum) throw new ArgumentException("An Enumeration type is required.", "enumObj");

            if (useLocaliztion)
            {
                var workContext = EngineContext.Current.Resolve<IWorkContext>();
                var localizationService = EngineContext.Current.Resolve<ILocalizationService>();

                var items = Enum.GetValues(enumObj.GetType())
                    .Cast<TEnum>()
                    .Where(enumValue => valuesToExclude == null || !valuesToExclude.Contains(Convert.ToInt32(enumValue)));
                var values = items.Select(enumValue => new
                {
                    ID = Convert.ToInt32(enumValue),
                    Name = enumValue.GetLocalizedEnum(localizationService, workContext)
                });
                object selectedValue = null;
                if (markCurrentAsSelected)
                    selectedValue = Convert.ToInt32(enumObj);
                return new SelectList(values, "ID", "Name", selectedValue);

            }
            else
            {
                var values = from Enum e in Enum.GetValues(enumObj.GetType())
                             orderby e
                             select new
                             {
                                 Id = Convert.ToInt32(e),
                                 Name = e.GetDisplayName()
                             };
                object selectedValue = null;
                if (markCurrentAsSelected)
                    selectedValue = Convert.ToInt32(enumObj);
                return new SelectList(values, "ID", "Name", selectedValue);
            }

        }

        public static List<SelectListItem> ToSelectListItems<T>(this T val) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException("An Enumeration type is required.", "enumObj");
            var items = from Enum e in Enum.GetValues(val.GetType())
                        orderby e
                        select new SelectListItem {
                            Value = e.ToString(),
                            Text = e.GetDisplayName(),
                            Selected = e.ToString() == val.ToString()
                        };
            return items.ToList();
        }

        public static string GetDisplayName(this Enum value)
        {
            var result = string.Empty;
            var display = value.GetType()
                           .GetMember(value.ToString())
                           .First()
                           .GetCustomAttribute<DisplayAttribute>()
                           ?.Name;

            return string.IsNullOrEmpty(display) ? value.ToString() : display;
        }
    }
}
