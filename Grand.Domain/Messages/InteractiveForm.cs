using Grand.Domain.Localization;
using System;
using System.Collections.Generic;

namespace Grand.Domain.Messages
{
    /// <summary>
    /// Represents a interactive forms
    /// </summary>
    public partial class InteractiveForm : BaseEntity, ILocalizedEntity
    {
        private ICollection<FormAttribute> _formAttributes;
        public InteractiveForm()
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
        /// Gets or sets the used email account identifier
        /// </summary>
        public string EmailAccountId { get; set; }
        /// <summary>
        /// Gets or sets the collection of locales
        /// </summary>
        public IList<LocalizedProperty> Locales { get; set; }
        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }


        public virtual ICollection<FormAttribute> FormAttributes
        {
            get { return _formAttributes ?? (_formAttributes = new List<FormAttribute>()); }
            protected set { _formAttributes = value; }
        }

        public class FormAttribute : SubBaseEntity, ILocalizedEntity
        {
            private ICollection<FormAttributeValue> _formAttributeValues;

            public FormAttribute()
            {
                Locales = new List<LocalizedProperty>();
            }
            /// <summary>
            /// Gets or sets the name
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the SystemName
            /// </summary>
            public string SystemName { get; set; }

            /// <summary>
            /// Gets or sets the regex validation
            /// </summary>
            public string RegexValidation { get; set; }

            /// <summary>
            /// Gets or sets the css style
            /// </summary>
            public string Style { get; set; }

            /// <summary>
            /// Gets or sets the css class
            /// </summary>
            public string Class { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the entity is required
            /// </summary>
            public bool IsRequired { get; set; }

            /// <summary>
            /// Gets or sets the attribute control type identifier
            /// </summary>
            public int FormControlTypeId { get; set; }
            /// <summary>
            /// Gets the attribute control type
            /// </summary>
            public FormControlType AttributeControlType
            {
                get
                {
                    return (FormControlType)this.FormControlTypeId;
                }
                set
                {
                    this.FormControlTypeId = (int)value;
                }
            }
            /// <summary>
            /// Gets or sets the validation rule for minimum length (for textbox and multiline textbox)
            /// </summary>
            public int? ValidationMinLength { get; set; }

            /// <summary>
            /// Gets or sets the validation rule for maximum length (for textbox and multiline textbox)
            /// </summary>
            public int? ValidationMaxLength { get; set; }
            /// <summary>
            /// Gets or sets the default value (for textbox and multiline textbox)
            /// </summary>
            public string DefaultValue { get; set; }

            public IList<LocalizedProperty> Locales { get; set; }
            public virtual ICollection<FormAttributeValue> FormAttributeValues
            {
                get { return _formAttributeValues ?? (_formAttributeValues = new List<FormAttributeValue>()); }
                protected set { _formAttributeValues = value; }
            }
        }


        public class FormAttributeValue : SubBaseEntity, ILocalizedEntity
        {
            public FormAttributeValue()
            {
                Locales = new List<LocalizedProperty>();
            }
            /// <summary>
            /// Gets or sets the checkout attribute name
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether the value is pre-selected
            /// </summary>
            public bool IsPreSelected { get; set; }

            /// <summary>
            /// Gets or sets the display order
            /// </summary>
            public int DisplayOrder { get; set; }

            public IList<LocalizedProperty> Locales { get; set; }
        }
    }
}
