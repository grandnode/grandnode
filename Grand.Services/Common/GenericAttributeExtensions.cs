using Grand.Core;
using Grand.Domain;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Common
{
    public static class GenericAttributeExtensions
    {
        /// <summary>
        /// Get an attribute of an entity
        /// </summary>
        /// <typeparam name="TPropType">Property type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="genericAttributeService">GenericAttributeService</param>
        /// <param name="key">Key</param>
        /// <param name="storeId">Load a value specific for a certain store; pass 0 to load a value shared for all stores</param>
        /// <returns>Attribute</returns>
        public static async Task<TPropType> GetAttribute<TPropType>(this BaseEntity entity, IGenericAttributeService genericAttributeService, string key, string storeId = "")
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            return await genericAttributeService.GetAttributesForEntity<TPropType>(entity, key, storeId);
        }

        /// <summary>
        /// Get an attribute of an entity
        /// </summary>
        /// <typeparam name="TPropType">Property type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="key">Key</param>
        /// <param name="storeId">Load a value specific for a certain store; pass 0 to load a value shared for all stores</param>
        /// <returns>Attribute</returns>
        public static TPropType GetAttributeFromEntity<TPropType>(this BaseEntity entity, string key, string storeId = "")
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            var props = entity.GenericAttributes;
            if (props == null)
                return default(TPropType);
            props = props.Where(x => x.StoreId == storeId).ToList();
            if (!props.Any())
                return default(TPropType);

            var prop = props.FirstOrDefault(ga =>
                ga.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase));

            if (prop == null || string.IsNullOrEmpty(prop.Value))
                return default(TPropType);

            return CommonHelper.To<TPropType>(prop.Value);
        }

    }
}
