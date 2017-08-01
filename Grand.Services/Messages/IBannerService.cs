using System.Collections.Generic;
using Grand.Core.Domain.Messages;

namespace Grand.Services.Messages
{
    public partial interface IBannerService
    {
        /// <summary>
        /// Inserts a banner
        /// </summary>
        /// <param name="banner">Banner</param>        
        void InsertBanner(Banner banner);

        /// <summary>
        /// Updates a banner
        /// </summary>
        /// <param name="banner">Banner</param>
        void UpdateBanner(Banner banner);

        /// <summary>
        /// Deleted a banner
        /// </summary>
        /// <param name="banner">Banner</param>
        void DeleteBanner(Banner banner);

        /// <summary>
        /// Gets a banner by identifier
        /// </summary>
        /// <param name="bannerId">Banner identifier</param>
        /// <returns>Banner</returns>
        Banner GetBannerById(string bannerId);

        /// <summary>
        /// Gets all banner
        /// </summary>
        /// <returns>Banners</returns>
        IList<Banner> GetAllBanners();


    }
}
