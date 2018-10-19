using Grand.Core.Domain.Localization;
using Grand.Core.Domain.Security;
using Grand.Core.Domain.Stores;
using System;
using System.Collections.Generic;

namespace Grand.Core.Domain.Polls
{
    /// <summary>
    /// Represents a poll
    /// </summary>
    public partial class Poll : BaseEntity, IStoreMappingSupported, ILocalizedEntity, IAclSupported
    {
        private ICollection<PollAnswer> _pollAnswers;

        public Poll()
        {
            Stores = new List<string>();
            Locales = new List<LocalizedProperty>();
            CustomerRoles = new List<string>();
        }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the system keyword
        /// </summary>
        public string SystemKeyword { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is published
        /// </summary>
        public bool Published { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity should be shown on home page
        /// </summary>
        public bool ShowOnHomePage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the anonymous votes are allowed
        /// </summary>
        public bool AllowGuestsToVote { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is limited/restricted to certain stores
        /// </summary>
        public virtual bool LimitedToStores { get; set; }
        public IList<string> Stores { get; set; }

        /// <summary>
        /// Gets or sets the collection of locales
        /// </summary>
        public IList<LocalizedProperty> Locales { get; set; }


        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the poll start date and time
        /// </summary>
        public DateTime? StartDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the poll end date and time
        /// </summary>
        public DateTime? EndDateUtc { get; set; }
        
        /// <summary>
        /// Gets or sets the news comments
        /// </summary>
        public virtual ICollection<PollAnswer> PollAnswers
        {
            get { return _pollAnswers ?? (_pollAnswers = new List<PollAnswer>()); }
            protected set { _pollAnswers = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is subject to ACL
        /// </summary>
        public bool SubjectToAcl { get; set; }
        public IList<string> CustomerRoles { get; set; }

    }
}