using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Messages;
using Nop.Services.Customers;
using Nop.Services.Events;

namespace Nop.Services.Messages
{
    public partial class BannerService : IBannerService
    {
        private readonly IRepository<Banner> _bannerRepository;
        private readonly ICustomerService _customerService;
        private readonly IStoreContext _storeContext;
        private readonly IEventPublisher _eventPublisher;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="bannerRepository">Banner repository</param>
        /// <param name="customerService">Customer service</param>
        /// <param name="storeContext">Store context</param>
        /// <param name="eventPublisher">Event published</param>
        /// <param name="languageService">Language service</param>
        public BannerService(IRepository<Banner> bannerRepository,            
            ICustomerService customerService, IStoreContext storeContext,
            IEventPublisher eventPublisher)
        {
            this._bannerRepository = bannerRepository;
            this._storeContext = storeContext;
            this._customerService = customerService;
            this._eventPublisher = eventPublisher;
        }

        /// <summary>
        /// Inserts a banner
        /// </summary>
        /// <param name="banner">Banner</param>        
        public virtual void InsertBanner(Banner banner)
        {
            if (banner == null)
                throw new ArgumentNullException("banner");

            _bannerRepository.Insert(banner);

            //event notification
            _eventPublisher.EntityInserted(banner);
        }

        /// <summary>
        /// Updates a banner
        /// </summary>
        /// <param name="banner">Banner</param>
        public virtual void UpdateBanner(Banner banner)
        {
            if (banner == null)
                throw new ArgumentNullException("banner");

            _bannerRepository.Update(banner);

            //event notification
            _eventPublisher.EntityUpdated(banner);
        }

        /// <summary>
        /// Deleted a banner
        /// </summary>
        /// <param name="banner">Banner</param>
        public virtual void DeleteBanner(Banner banner)
        {
            if (banner == null)
                throw new ArgumentNullException("banner");

            _bannerRepository.Delete(banner);

            //event notification
            _eventPublisher.EntityDeleted(banner);
        }

        /// <summary>
        /// Gets a banner by identifier
        /// </summary>
        /// <param name="bannerId">Banner identifier</param>
        /// <returns>Banner</returns>
        public virtual Banner GetBannerById(int bannerId)
        {
            if (bannerId == 0)
                return null;

            return _bannerRepository.GetById(bannerId);

        }

        /// <summary>
        /// Gets all banners
        /// </summary>
        /// <returns>Banners</returns>
        public virtual IList<Banner> GetAllBanners()
        {

            var query = from c in _bannerRepository.Table
                        orderby c.CreatedOnUtc
                        select c;
            var banners = query.ToList();

            return banners;
        }        

    }
}
