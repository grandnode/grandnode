using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System;
using System.Collections.Generic;

namespace Grand.Core.ModelBinding
{
    public class DictionaryStringObjectModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.ModelType == typeof(Dictionary<string, object>))
            {
                return new BinderTypeModelBinder(typeof(DictionaryObjectJsonModelBinder));
            }

            return null;
        }
    }
}
