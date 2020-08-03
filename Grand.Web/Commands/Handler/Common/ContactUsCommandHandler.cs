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
            var model = new ContactUsModel {
                Email = request.Customer.Email,
                FullName = request.Customer.GetFullName(),
                SubjectEnabled = _commonSettings.SubjectFieldOnContactUsForm,
                DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnContactUsPage
            };
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
                    DefaultValue = attribute.DefaultValue
                };
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
                        var attributeValueModel = new ContactUsModel.ContactAttributeValueModel {
                            Id = attributeValue.Id,
                            Name = attributeValue.GetLocalized(x => x.Name, request.Language.Id),
                            ColorSquaresRgb = attributeValue.ColorSquaresRgb,
                            IsPreSelected = attributeValue.IsPreSelected,
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
