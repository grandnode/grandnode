using System.Collections.Generic;
using Grand.Core.Domain.Messages;

namespace Grand.Services.Messages
{
    public partial interface IPopupService
    {
        /// <summary>
        /// Inserts a popup
        /// </summary>
        /// <param name="Popup">Popup</param>        
        void InsertPopupActive(PopupActive popup);
        /// <summary>
        /// Gets active banner for customer
        /// </summary>
        /// <returns>BannerActive</returns>
        PopupActive GetActivePopupByCustomerId(string customerId);

        /// <summary>
        /// Move popup to archive
        /// </summary>
        void MovepopupToArchive(string id, string customerId);

    }
}
