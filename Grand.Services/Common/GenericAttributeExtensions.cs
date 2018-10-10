using Grand.Core;
using Grand.Core.Infrastructure;
using System;

namespace Grand.Services.Common
{
    public static class GenericAttributeExtensions
    {
        /// <summary>
        /// Get an attribute of an entity
        /// </summary>
        /// <typeparam name="TPropType">Property type</typeparam>
        /// <param name="entity">Entity</param>
        /// <param name="key">Key</param>
        /// <param name="storeId">Load a value specific for a certain store; pass 0 to load a value shared for all stores</param>
        /// <returns>Attribute</returns>
        public static TPropType GetAttribute<TPropType>(this BaseEntity entity, string key, string storeId = "")
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            var genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();
            return genericAttributeService.GetAttributesForEntity<TPropType>(entity, key, storeId);

            
        }

    }
}
