using Grand.Core;
using Grand.Domain.Catalog;
using Grand.Domain.Messages;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Messages;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Interfaces;
using Grand.Web.Areas.Admin.Models.Messages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Services
{
    public partial class ContactAttributeViewModelService : IContactAttributeViewModelService
    {

        private readonly IContactAttributeService _contactAttributeService;
        private readonly IContactAttributeParser _contactAttributeParser;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerActivityService _customerActivityService;

        public ContactAttributeViewModelService(IContactAttributeService contactAttributeService,
            IContactAttributeParser contactAttributeParser,
            ILocalizationService localizationService,
            IWorkContext workContext,
            ICustomerActivityService customerActivityService)
        {
            _contactAttributeService = contactAttributeService;
            _contactAttributeParser = contactAttributeParser;
            _localizationService = localizationService;
            _workContext = workContext;
            _customerActivityService = customerActivityService;
        }

        #region Utilities

        protected virtual async Task SaveConditionAttributes(ContactAttribute contactAttribute, ContactAttributeModel model)
        {
            string attributesXml = null;
            if (model.ConditionModel.EnableCondition)
            {
                var attribute = await _contactAttributeService.GetContactAttributeById(model.ConditionModel.SelectedAttributeId);
                if (attribute != null)
                {
                    switch (attribute.AttributeControlType)
                    {
                        case AttributeControlType.DropdownList:
                        case AttributeControlType.RadioList:
                        case AttributeControlType.ColorSquares:
                        case AttributeControlType.ImageSquares:
                            {
                                var selectedAttribute = model.ConditionModel.ConditionAttributes
                                    .FirstOrDefault(x => x.Id == model.ConditionModel.SelectedAttributeId);
                                var selectedValue = selectedAttribute != null ? selectedAttribute.SelectedValueId : null;
                                if (!String.IsNullOrEmpty(selectedValue))
                                    attributesXml = _contactAttributeParser.AddContactAttribute(attributesXml, attribute, selectedValue);
                                else
                                    attributesXml = _contactAttributeParser.AddContactAttribute(attributesXml, attribute, string.Empty);
                            }
                            break;
                        case AttributeControlType.Checkboxes:
                            {
                                var selectedAttribute = model.ConditionModel.ConditionAttributes
                                    .FirstOrDefault(x => x.Id == model.ConditionModel.SelectedAttributeId);
                                var selectedValues = selectedAttribute != null ? selectedAttribute.Values.Where(x => x.Selected).Select(x => x.Value) : null;
                                if (selectedValues.Any())
                                    foreach (var value in selectedValues)
                                        attributesXml = _contactAttributeParser.AddContactAttribute(attributesXml, attribute, value);
                                else
                                    attributesXml = _contactAttributeParser.AddContactAttribute(attributesXml, attribute, string.Empty);
                            }
                            break;
                        case AttributeControlType.ReadonlyCheckboxes:
                        case AttributeControlType.TextBox:
                        case AttributeControlType.MultilineTextbox:
                        case AttributeControlType.Datepicker:
                        case AttributeControlType.FileUpload:
                        default:
                            //these attribute types are not supported as conditions
                            break;
                    }
                }
            }
            contactAttribute.ConditionAttributeXml = attributesXml;
        }
        #endregion
         
        public virtual async Task<IEnumerable<ContactAttributeModel>> PrepareContactAttributeListModel()
        {
            var contactAttributes = await _contactAttributeService.GetAllContactAttributes(_workContext.CurrentCustomer.StaffStoreId, ignorAcl: true);
            return contactAttributes.Select(x =>
            {
                var attributeModel = x.ToModel();
                attributeModel.AttributeControlTypeName = x.AttributeControlType.GetLocalizedEnum(_localizationService, _workContext);
                return attributeModel;
            });
        }

        public virtual async Task PrepareConditionAttributes(ContactAttributeModel model, ContactAttribute contactAttribute)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            //currenty any contact attribute can have condition.
            model.ConditionAllowed = true;

            if (contactAttribute == null)
                return;

            var selectedAttribute = (await _contactAttributeParser.ParseContactAttributes(contactAttribute.ConditionAttributeXml)).FirstOrDefault();
            var selectedValues = await _contactAttributeParser.ParseContactAttributeValues(contactAttribute.ConditionAttributeXml);

            model.ConditionModel = new ConditionModel()
            {
                EnableCondition = !string.IsNullOrEmpty(contactAttribute.ConditionAttributeXml),
                SelectedAttributeId = selectedAttribute != null ? selectedAttribute.Id : "",
                ConditionAttributes = (await _contactAttributeService.GetAllContactAttributes(_workContext.CurrentCustomer.StaffStoreId, ignorAcl: true))
                    //ignore this attribute and non-combinable attributes
                    .Where(x => x.Id != contactAttribute.Id && x.CanBeUsedAsCondition())
                    .Select(x =>
                        new AttributeConditionModel()
                        {
                            Id = x.Id,
                            Name = x.Name,
                            AttributeControlType = x.AttributeControlType,
                            Values = x.ContactAttributeValues
                                .Select(v => new SelectListItem()
                                {
                                    Text = v.Name,
                                    Value = v.Id.ToString(),
                                    Selected = selectedAttribute != null && selectedAttribute.Id == x.Id && selectedValues.Any(sv => sv.Id == v.Id)
                                }).ToList()
                        }).ToList()
            };
        }

        public virtual async Task<ContactAttribute> InsertContactAttributeModel(ContactAttributeModel model)
        {
            var contactAttribute = model.ToEntity();
            await _contactAttributeService.InsertContactAttribute(contactAttribute);

            //activity log
            await _customerActivityService.InsertActivity("AddNewContactAttribute", contactAttribute.Id, _localizationService.GetResource("ActivityLog.AddNewContactAttribute"), contactAttribute.Name);
            return contactAttribute;
        }
        public virtual async Task<ContactAttribute> UpdateContactAttributeModel(ContactAttribute contactAttribute, ContactAttributeModel model)
        {
            contactAttribute = model.ToEntity(contactAttribute);
            await SaveConditionAttributes(contactAttribute, model);
            await _contactAttributeService.UpdateContactAttribute(contactAttribute);

            //activity log
            await _customerActivityService.InsertActivity("EditContactAttribute", contactAttribute.Id, _localizationService.GetResource("ActivityLog.EditContactAttribute"), contactAttribute.Name);
            return contactAttribute;
        }

        public virtual ContactAttributeValueModel PrepareContactAttributeValueModel(ContactAttribute contactAttribute)
        {
            var model = new ContactAttributeValueModel();
            model.ContactAttributeId = contactAttribute.Id;

            //color squares
            model.DisplayColorSquaresRgb = contactAttribute.AttributeControlType == AttributeControlType.ColorSquares;
            model.ColorSquaresRgb = "#000000";
            return model;
        }
        public virtual ContactAttributeValueModel PrepareContactAttributeValueModel(ContactAttribute contactAttribute, ContactAttributeValue contactAttributeValue)
        {
            var model = contactAttributeValue.ToModel();
            model.DisplayColorSquaresRgb = contactAttribute.AttributeControlType == AttributeControlType.ColorSquares;
            return model;
        }
        public virtual async Task<ContactAttributeValue> InsertContactAttributeValueModel(ContactAttribute contactAttribute, ContactAttributeValueModel model)
        {
            var cav = model.ToEntity();
            contactAttribute.ContactAttributeValues.Add(cav);
            await _contactAttributeService.UpdateContactAttribute(contactAttribute);
            return cav;
        }

        public virtual async Task<ContactAttributeValue> UpdateContactAttributeValueModel(ContactAttribute contactAttribute, ContactAttributeValue contactAttributeValue, ContactAttributeValueModel model)
        {
            contactAttributeValue = model.ToEntity(contactAttributeValue);
            await _contactAttributeService.UpdateContactAttribute(contactAttribute);
            return contactAttributeValue;
        }
    }
}
