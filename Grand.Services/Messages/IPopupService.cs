using Grand.Domain.Messages;
using System.Threading.Tasks;

namespace Grand.Services.Messages
{
    public partial interface IPopupService
    {
        /// <summary>
        /// Inserts a popup
        /// </summary>
        /// <param name="popup">Popup</param>        
        Task InsertPopupActive(PopupActive popup);
        /// <summary>
        /// Gets active banner for customer
        /// </summary>
        /// <returns>BannerActive</returns>
        Task<PopupActive> GetActivePopupByCustomerId(string customerId);

        /// <summary>
        /// Move popup to archive
        /// </summary>
        Task MovepopupToArchive(string id, string customerId);

    }
}
