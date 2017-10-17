using Grand.Core.Domain.Localization;
using System;
using System.Collections.Generic;

namespace Grand.Core.Domain.Messages
{
    /// <summary>
    /// Represents a banner
    /// </summary>
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
        public DateTime CreatedOnUtc { get; set; }

        
    }
}
