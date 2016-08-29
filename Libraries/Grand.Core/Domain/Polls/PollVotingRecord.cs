using System;
using Grand.Core.Domain.Customers;
using MongoDB.Bson.Serialization.Attributes;

namespace Grand.Core.Domain.Polls
{
    /// <summary>
    /// Represents a poll voting record
    /// </summary>
    [BsonIgnoreExtraElements]
    public partial class PollVotingRecord : SubBaseEntity
    {
        /// <summary>
        /// Gets or sets the poll answer identifier
        /// </summary>
        public string PollAnswerId { get; set; }

        /// <summary>
        /// Gets or sets the poll identifier
        /// </summary>
        public string PollId { get; set; }

        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public string CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

    }
}