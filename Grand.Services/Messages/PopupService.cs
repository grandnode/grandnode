using Grand.Core.Data;
using Grand.Core.Domain.Messages;
using Grand.Services.Events;
using System;
using System.Linq;

namespace Grand.Services.Messages
{
    public partial class PopupService : IPopupService
    {
        private readonly IRepository<PopupActive> _popupActiveRepository;
        private readonly IRepository<PopupArchive> _popupArchiveRepository;
        private readonly IEventPublisher _eventPublisher;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="popupActiveRepository">Popup Active repository</param>
        /// <param name="popupArchiveRepository">Popup Archive repository</param>
        /// <param name="eventPublisher">Event published</param>
        public PopupService(IRepository<PopupActive> popupActiveRepository,
            IRepository<PopupArchive> popupArchiveRepository,
            IEventPublisher eventPublisher)
        {
            this._popupActiveRepository = popupActiveRepository;
            this._popupArchiveRepository = popupArchiveRepository;
            this._eventPublisher = eventPublisher;
        }

        /// <summary>
        /// Inserts a popup
        /// </summary>
        /// <param name="popup">Popup</param>        
        public virtual void InsertPopupActive(PopupActive popup)
        {
            if (popup == null)
                throw new ArgumentNullException("popup");

            _popupActiveRepository.Insert(popup);

            //event notification
            _eventPublisher.EntityInserted(popup);
        }


        /// <summary>
        /// Gets a popup by identifier
        /// </summary>
        /// <param name="popupId">Popup identifier</param>
        /// <returns>Banner</returns>
        public virtual PopupActive GetActivePopupByCustomerId(string customerId)
        {
            var query = from c in _popupActiveRepository.Table
                        where c.CustomerId == customerId
                        orderby c.CreatedOnUtc
                        select c;
            var popup = query.FirstOrDefault();
            return popup;

        }

        public virtual void MovepopupToArchive(string id, string customerId)
        {
            if (String.IsNullOrEmpty(customerId) || String.IsNullOrEmpty(id))
                return;

            var query = from c in _popupActiveRepository.Table
                        where c.CustomerId == customerId && c.Id == id
                        select c;
            var popup = query.FirstOrDefault();
            if (popup != null)
            {
                var archiveBanner = new PopupArchive()
                {
                    Body = popup.Body,
                    BACreatedOnUtc = popup.CreatedOnUtc,
                    CreatedOnUtc = DateTime.UtcNow,
                    CustomerActionId = popup.CustomerActionId,
                    CustomerId = popup.CustomerId,
                    PopupActiveId = popup.Id,
                    PopupTypeId = popup.PopupTypeId,
                    Name = popup.Name,
                };
                _popupArchiveRepository.Insert(archiveBanner);
                _popupActiveRepository.Delete(popup);
            }

        }

    }
}
