using Grand.Core;
using Grand.Framework.Components;
using Grand.Web.Features.Models.Catalog;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class SearchBoxViewComponent : BaseViewComponent
    {
        private readonly IMediator _mediator;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;

        public SearchBoxViewComponent(IMediator mediator,
            IWorkContext workContext, IStoreContext storeContext)
        {
            _mediator = mediator;
            _workContext = workContext;
            _storeContext = storeContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _mediator.Send(new GetSearchBox() {
                Customer = _workContext.CurrentCustomer,
                Store = _storeContext.CurrentStore
            });
            return View(model);
        }
    }
}