using Grand.Core;
using Grand.Framework.Components;
using Grand.Web.Features.Models.Catalog;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class HomePageManufacturersViewComponent : BaseViewComponent
    {
        private readonly IMediator _mediator;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;

        public HomePageManufacturersViewComponent(IMediator mediator,
            IWorkContext workContext,
            IStoreContext storeContext)
        {
            _mediator = mediator;
            _workContext = workContext;
            _storeContext = storeContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _mediator.Send(new GetHomepageManufacturers() {
                Customer = _workContext.CurrentCustomer,
                Language = _workContext.WorkingLanguage,
                Store = _storeContext.CurrentStore
            });

            if (!model.Any())
                return Content("");

            return View(model);
        }
    }
}