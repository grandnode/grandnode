using Grand.Core.Domain.Localization;
using System.Collections.Generic;

namespace Grand.Core.Domain.Polls
{
    /// <summary>
    /// Represents a poll answer
    /// </summary>
    public partial class PollAnswer : SubBaseEntity, ILocalizedEntity
    {
        private ICollection<PollVotingRecord> _pollVotingRecords;

        public PollAnswer()
        {
            Locales = new List<LocalizedProperty>();
        }

        /// <summary>
        /// Gets or sets the poll identifier
        /// </summary>
        public string PollId { get; set; }

        /// <summary>
        /// Gets or sets the poll answer name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the current number of votes
        /// </summary>
        public int NumberOfVotes { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the collection of locales
        /// </summary>
        public IList<LocalizedProperty> Locales { get; set; }

        /// <summary>
        /// Gets or sets the poll voting records
        /// </summary>
        public virtual ICollection<PollVotingRecord> PollVotingRecords
        {
            get { return _pollVotingRecords ?? (_pollVotingRecords = new List<PollVotingRecord>()); }
            protected set { _pollVotingRecords = value; }
        }
    }
}