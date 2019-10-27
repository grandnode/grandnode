﻿using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Stores;
using System.Collections.Generic;

namespace Grand.Core.Domain.Messages
{
    public partial class NewsletterCategory : BaseEntity, ILocalizedEntity, IStoreMappingSupported
    {
        public NewsletterCategory()
        {
            Stores = new List<string>();
            Locales = new List<LocalizedProperty>();
        }
        
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the selected
        /// </summary>
        public bool Selected { get; set; }

        public bool Published { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is limited/restricted to certain stores
        /// </summary>
        public bool LimitedToStores { get; set; }
        public IList<string> Stores { get; set; }

        public int DisplayOrder { get; set; }

        public IList<LocalizedProperty> Locales { get; set; }
    }
}
