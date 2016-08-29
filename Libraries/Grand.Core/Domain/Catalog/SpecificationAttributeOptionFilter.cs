﻿
using MongoDB.Bson.Serialization.Attributes;

namespace Grand.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a specification attribute option filter
    /// </summary>
    [BsonIgnoreExtraElements]
    public class SpecificationAttributeOptionFilter
    {
        /// <summary>
        /// Gets or sets the specification attribute identifier
        /// </summary>
        public string SpecificationAttributeId { get; set; }

        /// <summary>
        /// Gets or sets the specification attribute name
        /// </summary>
        public string SpecificationAttributeName { get; set; }

        /// <summary>
        /// Gets or sets the specification attribute option color (RGB)
        /// </summary>
        public string SpecificationAttributeOptionColorRgb { get; set; }

        /// <summary>
        /// Gets or sets the specification attribute display order
        /// </summary>
        public  int SpecificationAttributeDisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the specification attribute option identifier
        /// </summary>
        public string SpecificationAttributeOptionId { get; set; }

        /// <summary>
        /// Gets or sets the specification attribute option name
        /// </summary>
        public string SpecificationAttributeOptionName { get; set; }

        /// <summary>
        /// Gets or sets the specification attribute option display order
        /// </summary>
        public int SpecificationAttributeOptionDisplayOrder { get; set; }
    }
}
