using Grand.Core;
using Grand.Domain;
using Grand.Domain.Localization;
using Grand.Domain.Seo;
using Grand.Framework.Localization;
using Grand.Services.Localization;
using Grand.Services.Seo;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class LocalizedExtension
    {
        public static List<LocalizedProperty> ToLocalizedProperty<T>(this IList<T> list) where T : ILocalizedModelLocal
        {
            var local = new List<LocalizedProperty>();
            foreach (var item in list)
            {
                Type[] interfaces = item.GetType().GetInterfaces();
                PropertyInfo[] props = item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                foreach (var prop in props)
                {
                    bool insert = true;

                    foreach (var i in interfaces)
                    {
                        if (i.HasProperty(prop.Name))
                        {
                            insert = false;
                        }
                    }

                    if (insert && prop.GetValue(item) != null)
                        local.Add(new LocalizedProperty()
                        {
                            LanguageId = item.LanguageId,
                            LocaleKey = prop.Name,
                            LocaleValue = prop.GetValue(item).ToString(),
                        });
                }
            }
            return local;
        }
        public static async Task<List<LocalizedProperty>> ToLocalizedProperty<T, E>(this IList<T> list, E entity, Expression<Func<T, string>> keySelector,
            SeoSettings _seoSettings, IUrlRecordService _urlRecordService, ILanguageService _languageService) where T : ILocalizedModelLocal where E : BaseEntity, ISlugSupported
        {
            var local = new List<LocalizedProperty>();
            foreach (var item in list)
            {
                Type[] interfaces = item.GetType().GetInterfaces();
                PropertyInfo[] props = item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                foreach (var prop in props)
                {
                    bool insert = true;

                    foreach (var i in interfaces)
                    {
                        if (i.HasProperty(prop.Name) && typeof(ILocalizedModelLocal).IsAssignableFrom(i))
                        {
                            insert = false;
                        }
                        if (i.HasProperty(prop.Name) && typeof(ISlugModelLocal).IsAssignableFrom(i))
                        {
                            var member = keySelector.Body as MemberExpression;
                            if (member == null)
                            {
                                throw new ArgumentException(string.Format(
                                    "Expression '{0}' refers to a method, not a property.",
                                    keySelector));
                            }
                            var propInfo = member.Member as PropertyInfo;
                            if (propInfo == null)
                            {
                                throw new ArgumentException(string.Format(
                                       "Expression '{0}' refers to a field, not a property.",
                                       keySelector));
                            }
                            var value = item.GetType().GetProperty(propInfo.Name).GetValue(item, null);
                            if (value != null)
                            {
                                var name = value.ToString();
                                var itemvalue = prop.GetValue(item) ?? "";
                                var seName = await entity.ValidateSeName(itemvalue.ToString(), name, false, _seoSettings, _urlRecordService, _languageService );
                                prop.SetValue(item, seName);
                                await _urlRecordService.SaveSlug(entity, seName, item.LanguageId);
                            }
                            else
                            {
                                var itemvalue = prop.GetValue(item) ?? "";
                                if (itemvalue != null && !string.IsNullOrEmpty(itemvalue.ToString()))
                                {
                                    var seName = await entity.ValidateSeName(itemvalue.ToString(), "", false, _seoSettings, _urlRecordService, _languageService);
                                    prop.SetValue(item, seName);
                                    await _urlRecordService.SaveSlug(entity, seName, item.LanguageId);
                                }
                                else
                                    insert = false;
                            }
                        }
                    }

                    if (insert && prop.GetValue(item) != null)
                        local.Add(new LocalizedProperty()
                        {
                            LanguageId = item.LanguageId,
                            LocaleKey = prop.Name,
                            LocaleValue = prop.GetValue(item).ToString(),
                        });
                }
            }
            return local;
        }


        public static bool HasProperty(this Type obj, string propertyName)
        {
            return obj.GetProperty(propertyName) != null;
        }
    }
}
