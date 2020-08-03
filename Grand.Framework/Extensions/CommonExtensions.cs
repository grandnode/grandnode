using Grand.Core;
using Grand.Core.Infrastructure;
using Grand.Framework.Kendoui;
using Grand.Services.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
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

        public static SelectList ToSelectList<TEnum>(this TEnum enumObj, HttpContext httpContext, bool markCurrentAsSelected = true, int[] valuesToExclude = null) where TEnum : struct
        {
            if (!typeof(TEnum).GetTypeInfo().IsEnum) throw new ArgumentException("An Enumeration type is required.", "enumObj");

            var localizationService = httpContext.RequestServices.GetRequiredService<ILocalizationService>();
            var workContext = httpContext.RequestServices.GetRequiredService<IWorkContext>();

            return ToSelectList(enumObj, localizationService, workContext, markCurrentAsSelected, valuesToExclude);
        }

        public static SelectList ToSelectList<TEnum>(this TEnum enumObj, ILocalizationService localizationService = null, IWorkContext workContext = null, bool markCurrentAsSelected = true, int[] valuesToExclude = null) where TEnum : struct
        {
            if (!typeof(TEnum).GetTypeInfo().IsEnum) throw new ArgumentException("An Enumeration type is required.", "enumObj");

            var values = from TEnum enumValue in Enum.GetValues(typeof(TEnum))
                         where valuesToExclude == null || !valuesToExclude.Contains(Convert.ToInt32(enumValue))
                         select new
                         {
                             ID = Convert.ToInt32(enumValue),
                             Name = ((localizationService != null && workContext != null) ? enumValue.GetLocalizedEnum(localizationService, workContext) : enumValue.ToString())
                         };
            object selectedValue = null;
            if (markCurrentAsSelected)
                selectedValue = Convert.ToInt32(enumObj);
            return new SelectList(values, "ID", "Name", selectedValue);
        }
    }
}
