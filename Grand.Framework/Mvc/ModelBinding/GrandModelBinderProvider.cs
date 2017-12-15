using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Grand.Framework.Mvc.Models;
using System.Reflection;

namespace Grand.Framework.Mvc.ModelBinding
{
    /// <summary>
    /// Represents model binder provider for the creating GrandModelBinder
    /// </summary>
    public class GrandModelBinderProvider : IModelBinderProvider
    {
        /// <summary>
        /// Creates a model binder based on passed context
        /// </summary>
        /// <param name="context">Model binder provider context</param>
        /// <returns>Model binder</returns>
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));


            var modelType = context.Metadata.ModelType;
            if (!typeof(BaseGrandModel).IsAssignableFrom(modelType))
                return null;

            //use GrandModelBinder as a ComplexTypeModelBinder for BaseGrandModel
            if (context.Metadata.IsComplexType && !context.Metadata.IsCollectionType)
            {
                //create binders for all model properties
                var propertyBinders = context.Metadata.Properties
                    .ToDictionary(modelProperty => modelProperty, modelProperty => context.CreateBinder(modelProperty));

                return new GrandModelBinder(propertyBinders);
            }

            //or return null to further search for a suitable binder
            return null;
        }
    }
}
