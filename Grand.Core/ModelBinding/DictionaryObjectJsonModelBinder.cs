//contributing: https://github.com/joseftw/JOS.SystemTextJsonDictionaryStringObjectJsonConverter
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Grand.Core.ModelBinding
{
    public class DictionaryObjectJsonModelBinder : IModelBinder
    {

        private static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new JsonSerializerOptions() {
            Converters = { new DictionaryStringObjectJsonConverter() }
        };

        public DictionaryObjectJsonModelBinder()
        {
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            if (bindingContext.ModelType != typeof(Dictionary<string, object>))
            {
                throw new NotSupportedException($"The '{nameof(DictionaryObjectJsonModelBinder)}' " +
                    $"model binder should only be used on Dictionary<string, object>, it will not work on '{bindingContext.ModelType.Name}'");
            }

            try
            {
                var data = await JsonSerializer.DeserializeAsync<Dictionary<string, object>>(bindingContext.HttpContext.Request.Body, DefaultJsonSerializerOptions);
                bindingContext.Result = ModelBindingResult.Success(data);
            }
            catch
            {
                bindingContext.Result = ModelBindingResult.Failed();
            }
        }
    }
}
