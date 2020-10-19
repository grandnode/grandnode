using Grand.Core;
using Grand.Framework.Components;
using Grand.Services.Messages;
using Grand.Web.Events;
using Grand.Web.Models.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class PopupActionViewComponent : BaseViewComponent
    {
        private readonly IPopupService _popupService;
        private readonly IWorkContext _workContext;
        private readonly IMediator _mediator;

        public PopupActionViewComponent(IPopupService popupService, IWorkContext workContext, IMediator mediator)
        {
            _popupService = popupService;
            _workContext = workContext;
            _mediator = mediator;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var result = await _popupService.GetActivePopupByCustomerId(_workContext.CurrentCustomer.Id);
            if (result == null)
                return Content("");

            var model = new PopupModel();
            model.Id = result.Id;
            model.Body = result.Body;
            model.Name = result.Name;
            model.PopupType = (Domain.Messages.PopupType)result.PopupTypeId;
            model.CustomerActionId = result.CustomerActionId;

            await _popupService.MovepopupToArchive(result.Id, _workContext.CurrentCustomer.Id);

            //notification 
            await _mediator.Publish(new PopupRenderEvent(_workContext.CurrentCustomer, result));

            return View(model);
        }
    }
}