using Grand.Domain.Data;
using Grand.Domain.Topics;
using Grand.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MediatR;

namespace Grand.Services.Topics
{
    /// <summary>
    /// Topic template service
    /// </summary>
    public partial class TopicTemplateService : ITopicTemplateService
    {
        #region Fields

        private readonly IRepository<TopicTemplate> _topicTemplateRepository;
        private readonly IMediator _mediator;

        #endregion
        
        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="topicTemplateRepository">Topic template repository</param>
        /// <param name="mediator">Mediator</param>
        public TopicTemplateService(IRepository<TopicTemplate> topicTemplateRepository, 
            IMediator mediator)
        {
            _topicTemplateRepository = topicTemplateRepository;
            _mediator = mediator;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Delete topic template
        /// </summary>
        /// <param name="topicTemplate">Topic template</param>
        public virtual async Task DeleteTopicTemplate(TopicTemplate topicTemplate)
        {
            if (topicTemplate == null)
                throw new ArgumentNullException("topicTemplate");

            await _topicTemplateRepository.DeleteAsync(topicTemplate);

            //event notification
            await _mediator.EntityDeleted(topicTemplate);
        }

        /// <summary>
        /// Gets all topic templates
        /// </summary>
        /// <returns>Topic templates</returns>
        public virtual async Task<IList<TopicTemplate>> GetAllTopicTemplates()
        {
            var query = from pt in _topicTemplateRepository.Table
                        orderby pt.DisplayOrder
                        select pt;

            return await query.ToListAsync();
        }
 
        /// <summary>
        /// Gets a topic template
        /// </summary>
        /// <param name="topicTemplateId">Topic template identifier</param>
        /// <returns>Topic template</returns>
        public virtual Task<TopicTemplate> GetTopicTemplateById(string topicTemplateId)
        {
            return _topicTemplateRepository.GetByIdAsync(topicTemplateId);
        }

        /// <summary>
        /// Inserts topic template
        /// </summary>
        /// <param name="topicTemplate">Topic template</param>
        public virtual async Task InsertTopicTemplate(TopicTemplate topicTemplate)
        {
            if (topicTemplate == null)
                throw new ArgumentNullException("topicTemplate");

            await _topicTemplateRepository.InsertAsync(topicTemplate);

            //event notification
            await _mediator.EntityInserted(topicTemplate);
        }

        /// <summary>
        /// Updates the topic template
        /// </summary>
        /// <param name="topicTemplate">Topic template</param>
        public virtual async Task UpdateTopicTemplate(TopicTemplate topicTemplate)
        {
            if (topicTemplate == null)
                throw new ArgumentNullException("topicTemplate");

            await _topicTemplateRepository.UpdateAsync(topicTemplate);

            //event notification
            await _mediator.EntityUpdated(topicTemplate);
        }
        
        #endregion
    }
}
