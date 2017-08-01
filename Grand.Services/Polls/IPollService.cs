using Grand.Core;
using Grand.Core.Domain.Polls;

namespace Grand.Services.Polls
{
    /// <summary>
    /// Poll service interface
    /// </summary>
    public partial interface IPollService
    {
        /// <summary>
        /// Gets a poll
        /// </summary>
        /// <param name="pollId">The poll identifier</param>
        /// <returns>Poll</returns>
        Poll GetPollById(string pollId);

        /// <summary>
        /// Gets a poll
        /// </summary>
        /// <param name="systemKeyword">The poll system keyword</param>
        /// <returns>Poll</returns>
        Poll GetPollBySystemKeyword(string systemKeyword, string storeId);
        
        /// <summary>
        /// Gets polls
        /// </summary>
        /// <param name="loadShownOnHomePageOnly">Retrieve only shown on home page polls</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Polls</returns>
        IPagedList<Poll> GetPolls(string storeId = "", bool loadShownOnHomePageOnly = false,
             int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        /// <summary>
        /// Deletes a poll
        /// </summary>
        /// <param name="poll">The poll</param>
        void DeletePoll(Poll poll);

        /// <summary>
        /// Inserts a poll
        /// </summary>
        /// <param name="poll">Poll</param>
        void InsertPoll(Poll poll);

        /// <summary>
        /// Updates the poll
        /// </summary>
        /// <param name="poll">Poll</param>
        void UpdatePoll(Poll poll);
        

        /// <summary>
        /// Gets a value indicating whether customer already vited for this poll
        /// </summary>
        /// <param name="pollId">Poll identifier</param>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>Result</returns>
        bool AlreadyVoted(string pollId, string customerId);
    }
}
