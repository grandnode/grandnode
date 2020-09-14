﻿using Grand.Domain.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Grand.Core.Models
{
    /// <summary>
    /// Represents base model
    /// </summary>
    public partial class BaseModel
    {
        #region Ctor

        public BaseModel()
        {
            GenericAttributes = new List<GenericAttribute>();
            PostInitialize();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Perform additional actions for binding the model
        /// </summary>
        /// <param name="bindingContext">Model binding context</param>
        /// <remarks>Developers can override this method in custom partial classes in order to add some custom model binding</remarks>
        public virtual void BindModel(ModelBindingContext bindingContext)
        {
        }

        /// <summary>
        /// Perform additional actions for the model initialization
        /// </summary>
        /// <remarks>Developers can override this method in custom partial classes in order to add some custom initialization code to constructors</remarks>
        protected virtual void PostInitialize()
        {            
        }

        #endregion

        #region Properties        

        public IList<GenericAttribute> GenericAttributes { get; set; }

        #endregion

    }
}
