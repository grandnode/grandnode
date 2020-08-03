using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Services.Common;
using Grand.Services.Directory;
using Grand.Web.Areas.Admin.Models.Common;
using System;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Extensions
{
    public static class AddressMappingExtensions
    {
        public static async Task<AddressModel> ToModel(this Address entity, ICountryService countryService = null, IStateProvinceService stateProvinceService = null)
        {
            var address = entity.MapTo<Address, AddressModel>();
            if (!string.IsNullOrEmpty(address.CountryId) && countryService != null)
            {
                address.CountryName = (await countryService.GetCountryById(address.CountryId))?.Name;
            }
            if (!string.IsNullOrEmpty(address.StateProvinceId) && stateProvinceService != null)
            {
                address.StateProvinceName = (await stateProvinceService.GetStateProvinceById(address.StateProvinceId))?.Name;
            }

            return address;
        }

        public static Address ToEntity(this AddressModel model)
        {
            return model.MapTo<AddressModel, Address>();
        }

        public static Address ToEntity(this AddressModel model, Address destination)
        {
            return model.MapTo(destination);
        }
        public static async Task PrepareCustomAddressAttributes(this AddressModel model,
            Address address,
            IAddressAttributeService addressAttributeService,
            IAddressAttributeParser addressAttributeParser)
        {
            //this method is very similar to the same one in Grand.Web project
            if (addressAttributeService == null)
                throw new ArgumentNullException("addressAttributeService");

            if (addressAttributeParser == null)
                throw new ArgumentNullException("addressAttributeParser");

            var attributes = await addressAttributeService.GetAllAddressAttributes();
            foreach (var attribute in attributes)
            {
                var attributeModel = new AddressModel.AddressAttributeModel {
                    Id = attribute.Id,
                    Name = attribute.Name,
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                };

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = attribute.AddressAttributeValues;
                    foreach (var attributeValue in attributeValues)
                    {
                        var attributeValueModel = new AddressModel.AddressAttributeValueModel {
                            Id = attributeValue.Id,
                            Name = attributeValue.Name,
                            IsPreSelected = attributeValue.IsPreSelected
                        };
                        attributeModel.Values.Add(attributeValueModel);
                    }
                }

                //set already selected attributes
                var selectedAddressAttributes = address != null ? address.CustomAttributes : null;
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.Checkboxes:
                        {
                            if (!String.IsNullOrEmpty(selectedAddressAttributes))
                            {
                                //clear default selection
                                foreach (var item in attributeModel.Values)
                                    item.IsPreSelected = false;

                                //select new values
                                var selectedValues = await addressAttributeParser.ParseAddressAttributeValues(selectedAddressAttributes);
                                foreach (var attributeValue in selectedValues)
                                    if (attributeModel.Id == attributeValue.AddressAttributeId)
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
                            if (!String.IsNullOrEmpty(selectedAddressAttributes))
                            {
                                var enteredText = addressAttributeParser.ParseValues(selectedAddressAttributes, attribute.Id);
                                if (enteredText.Count > 0)
                                    attributeModel.DefaultValue = enteredText[0];
                            }
                        }
                        break;
                    case AttributeControlType.ColorSquares:
                    case AttributeControlType.Datepicker:
                    case AttributeControlType.FileUpload:
                    case AttributeControlType.ImageSquares:
                    default:
                        //not supported attribute control types
                        break;
                }

                model.CustomAddressAttributes.Add(attributeModel);
            }
        }
    }
}