using Grand.Domain.Common;
using Grand.Framework.Security.Captcha;
using Grand.Services.Customers;
using Grand.Services.Localization;
using Grand.Services.Messages;
using Grand.Web.Commands.Models.Common;
using Grand.Web.Models.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Commands.Handler.Common
{
    public class ContactUsCommandHandler : IRequestHandler<ContactUsCommand, ContactUsModel>
    {

        private readonly IContactAttributeService _contactAttributeService;

        private readonly CommonSettings _commonSettings;
        private readonly CaptchaSettings _captchaSettings;

        public ContactUsCommandHandler(IContactAttributeService contactAttributeService, CommonSettings commonSettings, CaptchaSettings captchaSettings)
        {
            _contactAttributeService = contactAttributeService;
            _commonSettings = commonSettings;
            _captchaSettings = captchaSettings;
        }

        public async Task<ContactUsModel> Handle(ContactUsCommand request, CancellationToken cancellationToken)
        {
            var model = request.Model ?? new ContactUsModel {
                Email = request.Customer.Email,
                FullName = request.Customer.GetFullName(),
            };

            model.SubjectEnabled = _commonSettings.SubjectFieldOnContactUsForm;
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage;

            model.ContactAttributes = await PrepareContactAttributeModel(request);

            return model;
        }

        private async Task<IList<ContactUsModel.ContactAttributeModel>> PrepareContactAttributeModel(ContactUsCommand request)
        {
            var model = new List<ContactUsModel.ContactAttributeModel>();

            var contactAttributes = await _contactAttributeService.GetAllContactAttributes(request.Store.Id);
            foreach (var attribute in contactAttributes)
            {
                var attributeModel = new ContactUsModel.ContactAttributeModel {
                    Id = attribute.Id,
                    Name = attribute.GetLocalized(x => x.Name, request.Language.Id),
                    TextPrompt = attribute.GetLocalized(x => x.TextPrompt, request.Language.Id),
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                    DefaultValue = request.Form?[$"contact_attribute_{attribute.Id}"] ?? attribute.DefaultValue,
                };
                if (attribute.AttributeControlType == Domain.Catalog.AttributeControlType.Datepicker)
                {
                    int.TryParse(request.Form?[$"contact_attribute_{attribute.Id}_day"], out var selectedDay);
                    if (selectedDay > 0)
                        attributeModel.SelectedDay = selectedDay;

                    int.TryParse(request.Form?[$"contact_attribute_{attribute.Id}_month"], out var selectedMonth);
                    if (selectedMonth > 0)
                        attributeModel.SelectedMonth = selectedMonth;

                    int.TryParse(request.Form?[$"contact_attribute_{attribute.Id}_year"], out var selectedYear);
                    if (selectedYear > 0)
                        attributeModel.SelectedYear = selectedYear;
                }

                if (!string.IsNullOrEmpty(attribute.ValidationFileAllowedExtensions))
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
                        var preSelected = request.Form?[$"contact_attribute_{attribute.Id}"].ToString();
                        var attributeValueModel = new ContactUsModel.ContactAttributeValueModel {
                            Id = attributeValue.Id,
                            Name = attributeValue.GetLocalized(x => x.Name, request.Language.Id),
                            ColorSquaresRgb = attributeValue.ColorSquaresRgb,
                            IsPreSelected = string.IsNullOrEmpty(preSelected) ? attributeValue.IsPreSelected : (preSelected.Contains(attributeValue.Id)),
                            DisplayOrder = attributeValue.DisplayOrder,
                        };
                        attributeModel.Values.Add(attributeValueModel);
                    }
                }

                model.Add(attributeModel);
            }

            return model;
        }
    }
}