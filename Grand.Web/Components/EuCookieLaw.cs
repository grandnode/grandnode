using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Grand.Web.Services;
using System.Linq;
using Grand.Core.Domain.Common;
using Grand.Core.Domain;
using Grand.Core.Domain.Customers;
using Grand.Core;
using Grand.Services.Common;

namespace Grand.Web.ViewComponents
{
    public class EuCookieLawViewComponent : ViewComponent
    {
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        public EuCookieLawViewComponent(StoreInformationSettings storeInformationSettings,
            IWorkContext workContext, IStoreContext storeContext)
        {
            this._storeInformationSettings = storeInformationSettings;
            this._workContext = workContext;
            this._storeContext = storeContext;
        }

        public IViewComponentResult Invoke()
        {
            if (!_storeInformationSettings.DisplayEuCookieLawWarning)
                //disabled
                return Content("");

            //ignore search engines because some pages could be indexed with the EU cookie as description
            if (_workContext.CurrentCustomer.IsSearchEngineAccount())
                return Content("");

            if (_workContext.CurrentCustomer.GetAttribute<bool>(SystemCustomerAttributeNames.EuCookieLawAccepted, _storeContext.CurrentStore.Id))
                //already accepted
                return Content("");

            //ignore notification?
            //right now it's used during logout so popup window is not displayed twice
            if (TempData["Grand.IgnoreEuCookieLawWarning"] != null && Convert.ToBoolean(TempData["Grand.IgnoreEuCookieLawWarning"]))
                return Content("");

            return View();

        }
    }
}