using Grand.Domain.Data;
using Grand.Domain.Messages;
using Grand.Services.Events;
using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MediatR;

namespace Grand.Services.Messages
{
    public partial class PopupService : IPopupService
    {
        private readonly IRepository<PopupActive> _popupActiveRepository;
        private readonly IRepository<PopupArchive> _popupArchiveRepository;
        private readonly IMediator _mediator;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="popupActiveRepository">Popup Active repository</param>
        /// <param name="popupArchiveRepository">Popup Archive repository</param>
        /// <param name="mediator">Mediator</param>
        public PopupService(IRepository<PopupActive> popupActiveRepository,
            IRepository<PopupArchive> popupArchiveRepository,
            IMediator mediator)
        {
            _popupActiveRepository = popupActiveRepository;
            _popupArchiveRepository = popupArchiveRepository;
            _mediator = mediator;
        }

        /// <summary>
        /// Inserts a popup
        /// </summary>
        /// <param name="popup">Popup</param>        
        public virtual async Task InsertPopupActive(PopupActive popup)
        {
            if (popup == null)
                throw new ArgumentNullException("popup");

            await _popupActiveRepository.InsertAsync(popup);

            //event notification
            await _mediator.EntityInserted(popup);
        }


        public virtual async Task<PopupActive> GetActivePopupByCustomerId(string customerId)
        {
            var query = from c in _popupActiveRepository.Table
                        where c.CustomerId == customerId
                        orderby c.CreatedOnUtc
                        select c;
            return await query.FirstOrDefaultAsync();
        }

        public virtual async Task MovepopupToArchive(string id, string customerId)
        {
            if (String.IsNullOrEmpty(customerId) || String.IsNullOrEmpty(id))
                return;

            var query = from c in _popupActiveRepository.Table
                        where c.CustomerId == customerId && c.Id == id
                        select c;

            var popup = await query.FirstOrDefaultAsync();
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
                await _popupArchiveRepository.InsertAsync(archiveBanner);
                await _popupActiveRepository.DeleteAsync(popup);
            }

        }

    }
}
