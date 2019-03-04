using DotLiquid;
using Grand.Core.Domain.Messages;
using Grand.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Grand.Services.Messages.DotLiquidDrops
{
    public static class LiquidExtensions
    {
        public static List<string> GetTokens(params Type[] drops)
        {
            List<string> toReturn = new List<string>();
            foreach (var drop in drops)
            {
                toReturn.AddRange(drop.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Select(x => "{{" + drop.Name.Substring(6, drop.Name.Length - 6) + "." + x.Name + "}}"));
            }

            return toReturn;
        }

        public static string Render(LiquidObject liquidObject, string source)
        {
            var hash = Hash.FromAnonymousObject(liquidObject);
            Template.NamingConvention = new DotLiquid.NamingConventions.CSharpNamingConvention();
            Template template = Template.Parse(source);
            var replaced = template.Render(hash);

            return replaced;
        }

        public static string GetStoreUrl(this IStoreService storeService, string storeId = "")
        {
            var store = storeService.GetStoreById(storeId) ?? storeService.GetAllStores().FirstOrDefault();

            if (store == null)
                throw new Exception("No store could be loaded");

            return store.SslEnabled ? store.SecureUrl : store.Url;
        }
    }
}