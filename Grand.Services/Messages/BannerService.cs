using Grand.Domain.Data;
using Grand.Domain.Messages;
using Grand.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;
using MongoDB.Driver;
using MediatR;

namespace Grand.Services.Messages
{
    public partial class BannerService : IBannerService
    {
        private readonly IRepository<Banner> _bannerRepository;
        private readonly IMediator _mediator;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="bannerRepository">Banner repository</param>
        /// <param name="mediator">Mediator</param>
        public BannerService(IRepository<Banner> bannerRepository,
            IMediator mediator)
        {
            _bannerRepository = bannerRepository;
            _mediator = mediator;
        }

        /// <summary>
        /// Inserts a banner
        /// </summary>
        /// <param name="banner">Banner</param>        
        public virtual async Task InsertBanner(Banner banner)
        {
            if (banner == null)
                throw new ArgumentNullException("banner");

            await _bannerRepository.InsertAsync(banner);

            //event notification
            await _mediator.EntityInserted(banner);
        }

        /// <summary>
        /// Updates a banner
        /// </summary>
        /// <param name="banner">Banner</param>
        public virtual async Task UpdateBanner(Banner banner)
        {
            if (banner == null)
                throw new ArgumentNullException("banner");

            await _bannerRepository.UpdateAsync(banner);

            //event notification
            await _mediator.EntityUpdated(banner);
        }

        /// <summary>
        /// Deleted a banner
        /// </summary>
        /// <param name="banner">Banner</param>
        public virtual async Task DeleteBanner(Banner banner)
        {
            if (banner == null)
                throw new ArgumentNullException("banner");

            await _bannerRepository.DeleteAsync(banner);

            //event notification
            await _mediator.EntityDeleted(banner);
        }

        /// <summary>
        /// Gets a banner by identifier
        /// </summary>
        /// <param name="bannerId">Banner identifier</param>
        /// <returns>Banner</returns>
        public virtual Task<Banner> GetBannerById(string bannerId)
        {
            return _bannerRepository.GetByIdAsync(bannerId);
        }

        /// <summary>
        /// Gets all banners
        /// </summary>
        /// <returns>Banners</returns>
        public virtual async Task<IList<Banner>> GetAllBanners()
        {

            var query = from c in _bannerRepository.Table
                        orderby c.CreatedOnUtc
                        select c;
            return await query.ToListAsync();
        }

    }
}
