using Grand.Core;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Messages;
using Grand.Services.Localization;
using Grand.Services.Logging;
using Grand.Services.Messages;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Messages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

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

        protected virtual void SaveConditionAttributes(ContactAttribute contactAttribute, ContactAttributeModel model)
        {
            string attributesXml = null;
            if (model.ConditionModel.EnableCondition)
            {
                var attribute = _contactAttributeService.GetContactAttributeById(model.ConditionModel.SelectedAttributeId);
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
         
        public virtual IEnumerable<ContactAttributeModel> PrepareContactAttributeListModel()
        {
            var contactAttributes = _contactAttributeService.GetAllContactAttributes();
            return contactAttributes.Select(x =>
            {
                var attributeModel = x.ToModel();
                attributeModel.AttributeControlTypeName = x.AttributeControlType.GetLocalizedEnum(_localizationService, _workContext);
                return attributeModel;
            });
        }

        public virtual void PrepareConditionAttributes(ContactAttributeModel model, ContactAttribute contactAttribute)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            //currenty any contact attribute can have condition.
            model.ConditionAllowed = true;

            if (contactAttribute == null)
                return;

            var selectedAttribute = _contactAttributeParser.ParseContactAttributes(contactAttribute.ConditionAttributeXml).FirstOrDefault();
            var selectedValues = _contactAttributeParser.ParseContactAttributeValues(contactAttribute.ConditionAttributeXml);

            model.ConditionModel = new ConditionModel()
            {
                EnableCondition = !string.IsNullOrEmpty(contactAttribute.ConditionAttributeXml),
                SelectedAttributeId = selectedAttribute != null ? selectedAttribute.Id : "",
                ConditionAttributes = _contactAttributeService.GetAllContactAttributes()
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

        public virtual ContactAttribute InsertContactAttributeModel(ContactAttributeModel model)
        {
            var contactAttribute = model.ToEntity();
            contactAttribute.CustomerRoles = model.SelectedCustomerRoleIds != null ? model.SelectedCustomerRoleIds.ToList() : new List<string>();
            contactAttribute.Locales = model.Locales.ToLocalizedProperty();
            contactAttribute.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();
            _contactAttributeService.InsertContactAttribute(contactAttribute);

            //activity log
            _customerActivityService.InsertActivity("AddNewContactAttribute", contactAttribute.Id, _localizationService.GetResource("ActivityLog.AddNewContactAttribute"), contactAttribute.Name);
            return contactAttribute;
        }
        public virtual ContactAttribute UpdateContactAttributeModel(ContactAttribute contactAttribute, ContactAttributeModel model)
        {
            contactAttribute = model.ToEntity(contactAttribute);
            SaveConditionAttributes(contactAttribute, model);
            contactAttribute.CustomerRoles = model.SelectedCustomerRoleIds != null ? model.SelectedCustomerRoleIds.ToList() : new List<string>();
            contactAttribute.Locales = model.Locales.ToLocalizedProperty();
            contactAttribute.Stores = model.SelectedStoreIds != null ? model.SelectedStoreIds.ToList() : new List<string>();
            _contactAttributeService.UpdateContactAttribute(contactAttribute);

            //activity log
            _customerActivityService.InsertActivity("EditContactAttribute", contactAttribute.Id, _localizationService.GetResource("ActivityLog.EditContactAttribute"), contactAttribute.Name);
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
            var model = new ContactAttributeValueModel
            {
                ContactAttributeId = contactAttributeValue.ContactAttributeId,
                Name = contactAttributeValue.Name,
                ColorSquaresRgb = contactAttributeValue.ColorSquaresRgb,
                DisplayColorSquaresRgb = contactAttribute.AttributeControlType == AttributeControlType.ColorSquares,
                IsPreSelected = contactAttributeValue.IsPreSelected,
                DisplayOrder = contactAttributeValue.DisplayOrder,
            };
            return model;
        }
        public virtual ContactAttributeValue InsertContactAttributeValueModel(ContactAttribute contactAttribute, ContactAttributeValueModel model)
        {
            var cav = new ContactAttributeValue
            {
                ContactAttributeId = model.ContactAttributeId,
                Name = model.Name,
                ColorSquaresRgb = model.ColorSquaresRgb,
                IsPreSelected = model.IsPreSelected,
                DisplayOrder = model.DisplayOrder,
            };
            cav.Locales = model.Locales.ToLocalizedProperty();
            contactAttribute.ContactAttributeValues.Add(cav);
            _contactAttributeService.UpdateContactAttribute(contactAttribute);
            return cav;
        }

        public virtual ContactAttributeValue UpdateContactAttributeValueModel(ContactAttribute contactAttribute, ContactAttributeValue contactAttributeValue, ContactAttributeValueModel model)
        {
            contactAttributeValue.Name = model.Name;
            contactAttributeValue.ColorSquaresRgb = model.ColorSquaresRgb;
            contactAttributeValue.IsPreSelected = model.IsPreSelected;
            contactAttributeValue.DisplayOrder = model.DisplayOrder;
            contactAttributeValue.Locales = model.Locales.ToLocalizedProperty();
            _contactAttributeService.UpdateContactAttribute(contactAttribute);
            return contactAttributeValue;
        }
    }
}
