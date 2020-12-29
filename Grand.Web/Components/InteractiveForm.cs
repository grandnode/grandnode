using Grand.Framework.Components;
using Grand.Web.Features.Models.Messages;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Grand.Web.Components
{
    public class InteractiveFormViewComponent : BaseViewComponent
    {
        #region Fields

        private readonly IMediator _mediator;

        #endregion

        #region Constructors

        public InteractiveFormViewComponent(
            IMediator mediator)
        {
            _mediator = mediator;
        }

        #endregion

        #region Invoker

        public async Task<IViewComponentResult> InvokeAsync(string systemName)
        {
            var model = await _mediator.Send(new GetInteractiveForm() { SystemName = systemName });
            if (model == null)
                return Content("");

            return View(model);
        }

        #endregion

    }
}
