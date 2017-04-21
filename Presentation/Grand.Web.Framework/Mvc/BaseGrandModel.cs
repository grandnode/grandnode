using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Grand.Web.Framework.Mvc
{
    /// <summary>
    /// Base GrandNode model
    /// </summary>
    [ModelBinder(typeof(GrandModelBinder))]
    public partial class BaseGrandModel
    {
        public BaseGrandModel()
        {
            this.CustomProperties = new Dictionary<string, object>();
            PostInitialize();
        }

        public virtual void BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
        }

        /// <summary>
        /// Developers can override this method in custom partial classes
        /// in order to add some custom initialization code to constructors
        /// </summary>
        protected virtual void PostInitialize()
        {
            
        }

        /// <summary>
        /// Use this property to store any custom value for your models. 
        /// </summary>
        public Dictionary<string, object> CustomProperties { get; set; }
    }

    /// <summary>
    /// Base GrandNode entity model
    /// </summary>
    public partial class BaseNopEntityModel : BaseGrandModel
    {
        public virtual string Id { get; set; }
    }
}
