using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Common;

namespace Nop.Services.Common
{
    /// <summary>
    /// Generic attribute service interface
    /// </summary>
    public partial interface IGenericAttributeService
    {
        void SaveAttribute<TPropType>(BaseEntity entity, string key, TPropType value, string storeId = "");
    }
}