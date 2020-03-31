using Grand.Framework.Components;
using Grand.Web.Features.Models.Polls;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class PollBlockViewComponent : BaseViewComponent
    {
        private readonly IMediator _mediator;

        public PollBlockViewComponent(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<IViewComponentResult> InvokeAsync(string systemKeyword)
        {
            if (string.IsNullOrWhiteSpace(systemKeyword))
                return Content("");

            var model = await _mediator.Send(new GetPollBySystemName() { SystemName = systemKeyword });
            if (model == null || string.IsNullOrWhiteSpace(model.Id))
                return Content("");

            return View(model);

        }
    }
}