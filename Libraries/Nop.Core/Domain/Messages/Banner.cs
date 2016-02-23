using MongoDB.Bson.Serialization.Attributes;
using Nop.Core.Domain.Localization;
using System;
using System.Collections.Generic;

namespace Nop.Core.Domain.Messages
{
    /// <summary>
    /// Represents a campaign
    /// </summary>
    [BsonIgnoreExtraElements]
    public partial class Banner : BaseEntity, ILocalizedEntity
    {
        public Banner()
        {
            Locales = new List<LocalizedProperty>();
        }
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the body
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the collection of locales
        /// </summary>
        public IList<LocalizedProperty> Locales { get; set; }
        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Unspecified)]
        public DateTime CreatedOnUtc { get; set; }

        
    }
}
