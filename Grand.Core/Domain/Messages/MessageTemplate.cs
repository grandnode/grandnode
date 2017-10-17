using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Stores;
using System.Collections.Generic;

namespace Grand.Core.Domain.Messages
{
    /// <summary>
    /// Represents a message template
    /// </summary>
    public partial class MessageTemplate : BaseEntity, ILocalizedEntity, IStoreMappingSupported
    {
        public MessageTemplate()
        {
            Stores = new List<string>();
            Locales = new List<LocalizedProperty>();
        }
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the BCC Email addresses
        /// </summary>
        public string BccEmailAddresses { get; set; }

        /// <summary>
        /// Gets or sets the subject
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the body
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the template is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the delay before sending message
        /// </summary>
        public int? DelayBeforeSend { get; set; }

        /// <summary>
        /// Gets or sets the period of message delay 
        /// </summary>
        public int DelayPeriodId { get; set; }

        /// <summary>
        /// Gets or sets the download identifier of attached file
        /// </summary>
        public string AttachedDownloadId { get; set; }

        /// <summary>
        /// Gets or sets the used email account identifier
        /// </summary>
        public string EmailAccountId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is limited/restricted to certain stores
        /// </summary>
        public bool LimitedToStores { get; set; }
        public IList<string> Stores { get; set; }

        /// <summary>
        /// Gets or sets the period of message delay
        /// </summary>
        public MessageDelayPeriod DelayPeriod
        {
            get { return (MessageDelayPeriod)this.DelayPeriodId; }
            set { this.DelayPeriodId = (int)value; }
        }
        /// <summary>
        /// Gets or sets the collection of locales
        /// </summary>
        public IList<LocalizedProperty> Locales { get; set; }

    }
}
