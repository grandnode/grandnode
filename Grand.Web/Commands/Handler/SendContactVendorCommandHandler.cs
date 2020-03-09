﻿using Grand.Core;
using Grand.Core.Domain.Common;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Web.Commands.Models;
using Grand.Web.Models.Common;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Commands.Handler
{
    public class SendContactVendorCommandHandler : IRequestHandler<SendContactVendorCommandModel, ContactVendorModel>
    {
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;

        private readonly CommonSettings _commonSettings;

        public SendContactVendorCommandHandler(IWorkflowMessageService workflowMessageService, IWorkContext workContext,
            ILocalizationService localizationService, CommonSettings commonSettings)
        {
            _workflowMessageService = workflowMessageService;
            _workContext = workContext;
            _localizationService = localizationService;
            _commonSettings = commonSettings;
        }

        public async Task<ContactVendorModel> Handle(SendContactVendorCommandModel request, CancellationToken cancellationToken)
        {
            var subject = _commonSettings.SubjectFieldOnContactUsForm ? request.Model.Subject : null;
            var body = Core.Html.HtmlHelper.FormatText(request.Model.Enquiry, false, true, false, false, false, false);

            await _workflowMessageService.SendContactVendorMessage(_workContext.CurrentCustomer, request.Vendor, _workContext.WorkingLanguage.Id,
                request.Model.Email.Trim(), request.Model.FullName, subject, body);

            request.Model.SuccessfullySent = true;
            request.Model.Result = _localizationService.GetResource("ContactVendor.YourEnquiryHasBeenSent");
            return request.Model;
        }
    }
}
