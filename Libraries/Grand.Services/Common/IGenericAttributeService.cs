using System.Collections.Generic;
using Grand.Core;
using Grand.Core.Domain.Common;

namespace Grand.Services.Common
{
    /// <summary>
    /// Generic attribute service interface
    /// </summary>
    public partial interface IGenericAttributeService
    {
        void SaveAttribute<TPropType>(BaseEntity entity, string key, TPropType value, string storeId = "");
    }
}