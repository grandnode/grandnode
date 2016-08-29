﻿using MongoDB.Bson.Serialization.Attributes;
using Grand.Core.Domain.Localization;
using System.Collections.Generic;

namespace Grand.Core.Domain.Orders
{
    /// <summary>
    /// Represents a return request reason
    /// </summary>
    [BsonIgnoreExtraElements]
    public partial class ReturnRequestReason : BaseEntity, ILocalizedEntity
    {
        public ReturnRequestReason()
        {
            Locales = new List<LocalizedProperty>();
        }
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the collection of locales
        /// </summary>
        public IList<LocalizedProperty> Locales { get; set; }
    }
}
