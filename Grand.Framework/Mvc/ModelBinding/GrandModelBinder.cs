using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Grand.Framework.Mvc.Models;

namespace Grand.Framework.Mvc.ModelBinding
{
    /// <summary>
    /// Represents model binder for the binding models inherited from the BaseGrandModel
    /// </summary>
    public class GrandModelBinder : ComplexTypeModelBinder
    {
        #region Ctor

        public GrandModelBinder(IDictionary<ModelMetadata, IModelBinder> propertyBinders) : base(propertyBinders)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create model for given binding context
        /// </summary>
        /// <param name="bindingContext">Model binding context</param>
        /// <returns>Model</returns>
        protected override object CreateModel(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));
            
            //create base model
            var model = base.CreateModel(bindingContext);

            //add custom model binding
            if (model is BaseGrandModel)
                (model as BaseGrandModel).BindModel(bindingContext);

            return model;
        }

        /// <summary>
        ///  Updates a property in the current model
        /// </summary>
        /// <param name="bindingContext">Model binding context</param>
        /// <param name="modelName">The model name</param>
        /// <param name="propertyMetadata">The model metadata for the property to set</param>
        /// <param name="bindingResult">The binding result for the property's new value</param>
        protected override void SetProperty(ModelBindingContext bindingContext, string modelName, 
            ModelMetadata propertyMetadata, ModelBindingResult bindingResult)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            //trim property string values for models
            var valueAsString = bindingResult.Model as string;
            if (bindingContext.Model is BaseGrandModel && !string.IsNullOrEmpty(valueAsString))
            {
                //excluding properties with [NoTrim] attribute
                var noTrim = (propertyMetadata as DefaultModelMetadata)?.Attributes?.Attributes?.OfType<NoTrimAttribute>().Any();
                if (!noTrim.HasValue || !noTrim.Value)
                    bindingResult = ModelBindingResult.Success(valueAsString.Trim());
            }

            base.SetProperty(bindingContext, modelName, propertyMetadata, bindingResult);
        }

        #endregion
    }
}
