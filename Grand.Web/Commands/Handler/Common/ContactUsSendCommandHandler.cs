using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Stores;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Media;
using Grand.Services.Messages;
using Grand.Web.Commands.Models.Common;
using Grand.Web.Models.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Commands.Handler.Common
{
    public class ContactUsSendCommandHandler : IRequestHandler<ContactUsSendCommand, (ContactUsModel model, IList<string> errors)>
    {
        private readonly IWorkContext _workContext;
        private readonly IContactAttributeService _contactAttributeService;
        private readonly IContactAttributeParser _contactAttributeParser;
        private readonly IContactAttributeFormatter _contactAttributeFormatter;
        private readonly IDownloadService _downloadService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ICustomerActivityService _customerActivityService;

        private readonly CommonSettings _commonSettings;

        public ContactUsSendCommandHandler(IWorkContext workContext, 
            IContactAttributeService contactAttributeService,
            IContactAttributeParser contactAttributeParser, 
            IContactAttributeFormatter contactAttributeFormatter,
            IDownloadService downloadService, 
            ILocalizationService localizationService, 
            IWorkflowMessageService workflowMessageService,
            ICustomerActivityService customerActivityService,
            CommonSettings commonSettings)
        {
            _workContext = workContext;
            _contactAttributeService = contactAttributeService;
            _contactAttributeParser = contactAttributeParser;
            _contactAttributeFormatter = contactAttributeFormatter;
            _downloadService = downloadService;
            _localizationService = localizationService;
            _workflowMessageService = workflowMessageService;
            _customerActivityService = customerActivityService;
            _commonSettings = commonSettings;
        }

        public async Task<(ContactUsModel model, IList<string> errors)> Handle(ContactUsSendCommand request, CancellationToken cancellationToken)
        {
            var errors = new List<string>();
            //parse contact attributes
            var attributeXml = await ParseContactAttributes(request);
            var contactAttributeWarnings = await GetContactAttributesWarnings(attributeXml, request.Store.Id);
            if (contactAttributeWarnings.Any())
            {
                foreach (var item in contactAttributeWarnings)
                {
                    errors.Add(item);
                }
            }
            if (!errors.Any())
            {
                request.Model.ContactAttributeXml = attributeXml;
                request.Model.ContactAttributeInfo = await _contactAttributeFormatter.FormatAttributes(attributeXml, _workContext.CurrentCustomer);
                request.Model = await SendContactUs(request.Model, request.Store);

                //activity log
                await _customerActivityService.InsertActivity("PublicStore.ContactUs", "", _localizationService.GetResource("ActivityLog.PublicStore.ContactUs"));

            }
            else
            {
                request.Model.ContactAttributes = await PrepareContactAttributeModel(attributeXml, request.Store.Id);
            }

            return (request.Model, errors);

        }

        private async Task<string> ParseContactAttributes(ContactUsSendCommand request)
        {
            string attributesXml = "";
            var contactAttributes = await _contactAttributeService.GetAllContactAttributes(request.Store.Id);
            foreach (var attribute in contactAttributes)
            {
                string controlId = string.Format("contact_attribute_{0}", attribute.Id);
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                        {
                            var ctrlAttributes = request.Form[controlId];
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                attributesXml = _contactAttributeParser.AddContactAttribute(attributesXml,
                                        attribute, ctrlAttributes);

                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = request.Form[controlId].ToString();
                            if (!String.IsNullOrEmpty(cblAttributes))
                            {
                                foreach (var item in cblAttributes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    attributesXml = _contactAttributeParser.AddContactAttribute(attributesXml, attribute, item);
                                }
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //load read-only (already server-side selected) values
                            var attributeValues = attribute.ContactAttributeValues;
                            foreach (var selectedAttributeId in attributeValues
                                .Where(v => v.IsPreSelected)
                                .Select(v => v.Id)
                                .ToList())
                            {
                                attributesXml = _contactAttributeParser.AddContactAttribute(attributesXml,
                                            attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            var ctrlAttributes = request.Form[controlId].ToString();
                            if (!String.IsNullOrEmpty(ctrlAttributes))
                            {
                                string enteredText = ctrlAttributes.Trim();
                                attributesXml = _contactAttributeParser.AddContactAttribute(attributesXml,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                        {
                            var date = request.Form[controlId + "_day"];
                            var month = request.Form[controlId + "_month"];
                            var year = request.Form[controlId + "_year"];
                            DateTime? selectedDate = null;
                            try
                            {
                                selectedDate = new DateTime(Int32.Parse(year), Int32.Parse(month), Int32.Parse(date));
                            }
                            catch { }
                            if (selectedDate.HasValue)
                            {
                                attributesXml = _contactAttributeParser.AddContactAttribute(attributesXml,
                                    attribute, selectedDate.Value.ToString("D"));
                            }
                        }
                        break;
                    case AttributeControlType.FileUpload:
                        {
                            Guid downloadGuid;
                            Guid.TryParse(request.Form[controlId], out downloadGuid);
                            var download = await _downloadService.GetDownloadByGuid(downloadGuid);
                            if (download != null)
                            {
                                attributesXml = _contactAttributeParser.AddContactAttribute(attributesXml,
                                           attribute, download.DownloadGuid.ToString());
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            //validate conditional attributes (if specified)
            foreach (var attribute in contactAttributes)
            {
                var conditionMet = await _contactAttributeParser.IsConditionMet(attribute, attributesXml);
                if (conditionMet.HasValue && !conditionMet.Value)
                    attributesXml = _contactAttributeParser.RemoveContactAttribute(attributesXml, attribute);
            }

            return attributesXml;
        }

        private async Task<IList<string>> GetContactAttributesWarnings(string contactAttributesXml, string storeId)
        {
            var warnings = new List<string>();

            //selected attributes
            var attributes1 = await _contactAttributeParser.ParseContactAttributes(contactAttributesXml);

            //existing contact attributes
            var attributes2 = await _contactAttributeService.GetAllContactAttributes(storeId);
            foreach (var a2 in attributes2)
            {
                var conditionMet = await _contactAttributeParser.IsConditionMet(a2, contactAttributesXml);
                if (a2.IsRequired && ((conditionMet.HasValue && conditionMet.Value) || !conditionMet.HasValue))
                {
                    bool found = false;
                    //selected checkout attributes
                    foreach (var a1 in attributes1)
                    {
                        if (a1.Id == a2.Id)
                        {
                            var attributeValuesStr = _contactAttributeParser.ParseValues(contactAttributesXml, a1.Id);
                            foreach (string str1 in attributeValuesStr)
                                if (!String.IsNullOrEmpty(str1.Trim()))
                                {
                                    found = true;
                                    break;
                                }
                        }
                    }

                    //if not found
                    if (!found)
                    {
                        if (!string.IsNullOrEmpty(a2.GetLocalized(a => a.TextPrompt, _workContext.WorkingLanguage.Id)))
                            warnings.Add(a2.GetLocalized(a => a.TextPrompt, _workContext.WorkingLanguage.Id));
                        else
                            warnings.Add(string.Format(_localizationService.GetResource("ContactUs.SelectAttribute"), a2.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id)));
                    }
                }
            }

            //now validation rules

            //minimum length
            foreach (var ca in attributes2)
            {
                if (ca.ValidationMinLength.HasValue)
                {
                    if (ca.AttributeControlType == AttributeControlType.TextBox ||
                        ca.AttributeControlType == AttributeControlType.MultilineTextbox)
                    {
                        var valuesStr = _contactAttributeParser.ParseValues(contactAttributesXml, ca.Id);
                        var enteredText = valuesStr.FirstOrDefault();
                        int enteredTextLength = String.IsNullOrEmpty(enteredText) ? 0 : enteredText.Length;

                        if (ca.ValidationMinLength.Value > enteredTextLength)
                        {
                            warnings.Add(string.Format(_localizationService.GetResource("ContactUs.TextboxMinimumLength"), ca.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id), ca.ValidationMinLength.Value));
                        }
                    }
                }

                //maximum length
                if (ca.ValidationMaxLength.HasValue)
                {
                    if (ca.AttributeControlType == AttributeControlType.TextBox ||
                        ca.AttributeControlType == AttributeControlType.MultilineTextbox)
                    {
                        var valuesStr = _contactAttributeParser.ParseValues(contactAttributesXml, ca.Id);
                        var enteredText = valuesStr.FirstOrDefault();
                        int enteredTextLength = String.IsNullOrEmpty(enteredText) ? 0 : enteredText.Length;

                        if (ca.ValidationMaxLength.Value < enteredTextLength)
                        {
                            warnings.Add(string.Format(_localizationService.GetResource("ContactUs.TextboxMaximumLength"), ca.GetLocalized(a => a.Name, _workContext.WorkingLanguage.Id), ca.ValidationMaxLength.Value));
                        }
                    }
                }
            }

            return warnings;
        }

        private async Task<IList<ContactUsModel.ContactAttributeModel>> PrepareContactAttributeModel(string selectedContactAttributes, string storeId)
        {
            var model = new List<ContactUsModel.ContactAttributeModel>();

            var contactAttributes = await _contactAttributeService.GetAllContactAttributes(storeId);
            foreach (var attribute in contactAttributes)
            {
                var attributeModel = new ContactUsModel.ContactAttributeModel {
                    Id = attribute.Id,
                    Name = attribute.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                    TextPrompt = attribute.GetLocalized(x => x.TextPrompt, _workContext.WorkingLanguage.Id),
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                    DefaultValue = attribute.DefaultValue
                };
                if (!String.IsNullOrEmpty(attribute.ValidationFileAllowedExtensions))
                {
                    attributeModel.AllowedFileExtensions = attribute.ValidationFileAllowedExtensions
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = attribute.ContactAttributeValues;
                    foreach (var attributeValue in attributeValues)
                    {
                        var attributeValueModel = new ContactUsModel.ContactAttributeValueModel {
                            Id = attributeValue.Id,
                            Name = attributeValue.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id),
                            ColorSquaresRgb = attributeValue.ColorSquaresRgb,
                            IsPreSelected = attributeValue.IsPreSelected,
                            DisplayOrder = attributeValue.DisplayOrder,
                        };
                        attributeModel.Values.Add(attributeValueModel);
                    }
                }

                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.Checkboxes:
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.ImageSquares:
                        {
                            if (!String.IsNullOrEmpty(selectedContactAttributes))
                            {
                                //clear default selection
                                foreach (var item in attributeModel.Values)
                                    item.IsPreSelected = false;

                                //select new values
                                var selectedValues = await _contactAttributeParser.ParseContactAttributeValues(selectedContactAttributes);
                                foreach (var attributeValue in selectedValues)
                                    if (attributeModel.Id == attributeValue.ContactAttributeId)
                                        foreach (var item in attributeModel.Values)
                                            if (attributeValue.Id == item.Id)
                                                item.IsPreSelected = true;
                            }
                        }
                        break;
                    case AttributeControlType.ReadonlyCheckboxes:
                        {
                            //do nothing
                            //values are already pre-set
                        }
                        break;
                    case AttributeControlType.TextBox:
                    case AttributeControlType.MultilineTextbox:
                        {
                            if (!String.IsNullOrEmpty(selectedContactAttributes))
                            {
                                var enteredText = _contactAttributeParser.ParseValues(selectedContactAttributes, attribute.Id);
                                if (enteredText.Any())
                                    attributeModel.DefaultValue = enteredText[0];
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                        {
                            //keep in mind my that the code below works only in the current culture
                            var selectedDateStr = _contactAttributeParser.ParseValues(selectedContactAttributes, attribute.Id);
                            if (selectedDateStr.Any())
                            {
                                DateTime selectedDate;
                                if (DateTime.TryParseExact(selectedDateStr[0], "D", CultureInfo.CurrentCulture,
                                                       DateTimeStyles.None, out selectedDate))
                                {
                                    //successfully parsed
                                    attributeModel.SelectedDay = selectedDate.Day;
                                    attributeModel.SelectedMonth = selectedDate.Month;
                                    attributeModel.SelectedYear = selectedDate.Year;
                                }
                            }

                        }
                        break;
                    case AttributeControlType.FileUpload:
                        {
                            if (!String.IsNullOrEmpty(selectedContactAttributes))
                            {
                                var downloadGuidStr = _contactAttributeParser.ParseValues(selectedContactAttributes, attribute.Id).FirstOrDefault();
                                Guid downloadGuid;
                                Guid.TryParse(downloadGuidStr, out downloadGuid);
                                var download = await _downloadService.GetDownloadByGuid(downloadGuid);
                                if (download != null)
                                    attributeModel.DefaultValue = download.DownloadGuid.ToString();
                            }
                        }
                        break;
                    default:
                        break;
                }

                model.Add(attributeModel);
            }

            return model;
        }

        private async Task<ContactUsModel> SendContactUs(ContactUsModel model, Store store)
        {
            string email = model.Email.Trim();
            string fullName = model.FullName;
            string subject = _commonSettings.SubjectFieldOnContactUsForm ? model.Subject : null;
            string body = Core.Html.HtmlHelper.FormatText(model.Enquiry, false, true, false, false, false, false);

            await _workflowMessageService.SendContactUsMessage(_workContext.CurrentCustomer, store, _workContext.WorkingLanguage.Id, model.Email.Trim(), model.FullName, subject, body, model.ContactAttributeInfo, model.ContactAttributeXml);

            model.SuccessfullySent = true;
            model.Result = _localizationService.GetResource("ContactUs.YourEnquiryHasBeenSent");

            return model;
        }
    }
}
