using Grand.Domain.Messages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Messages
{
    public partial interface IBannerService
    {
        /// <summary>
        /// Inserts a banner
        /// </summary>
        /// <param name="banner">Banner</param>        
        Task InsertBanner(Banner banner);

        /// <summary>
        /// Updates a banner
        /// </summary>
        /// <param name="banner">Banner</param>
        Task UpdateBanner(Banner banner);

        /// <summary>
        /// Deleted a banner
        /// </summary>
        /// <param name="banner">Banner</param>
        Task DeleteBanner(Banner banner);

        /// <summary>
        /// Gets a banner by identifier
        /// </summary>
        /// <param name="bannerId">Banner identifier</param>
        /// <returns>Banner</returns>
        Task<Banner> GetBannerById(string bannerId);

        /// <summary>
        /// Gets all banner
        /// </summary>
        /// <returns>Banners</returns>
        Task<IList<Banner>> GetAllBanners();
    }
}
