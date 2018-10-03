using Grand.Core.Domain.Common;
using System.Collections.Generic;

namespace Grand.Core
{
    /// <summary>
    /// Base class for entities
    /// </summary>
    public abstract partial class BaseEntity: ParentEntity
    {
        protected BaseEntity()
        {
            GenericAttributes = new List<GenericAttribute>();
        }

        public IList<GenericAttribute> GenericAttributes { get; set; }
        
    }
}
