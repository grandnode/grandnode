using Grand.Domain.Blogs;
using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Courses;
using Grand.Domain.Customers;
using Grand.Domain.Directory;
using Grand.Domain.Discounts;
using Grand.Domain.Documents;
using Grand.Domain.Forums;
using Grand.Domain.Knowledgebase;
using Grand.Domain.Localization;
using Grand.Domain.Logging;
using Grand.Domain.Media;
using Grand.Domain.Messages;
using Grand.Domain.News;
using Grand.Domain.Orders;
using Grand.Domain.Polls;
using Grand.Domain.Shipping;
using Grand.Domain.Stores;
using Grand.Domain.Tax;
using Grand.Domain.Topics;
using Grand.Domain.Vendors;
using Grand.Core.Infrastructure.Mapper;
using Grand.Core.Plugins;
using Grand.Services.Authentication.External;
using Grand.Services.Cms;
using Grand.Services.Common;
using Grand.Services.Directory;
using Grand.Services.Helpers;
using Grand.Services.Payments;
using Grand.Services.Shipping;
using Grand.Services.Tax;
using Grand.Admin.Models.Blogs;
using Grand.Admin.Models.Catalog;
using Grand.Admin.Models.Cms;
using Grand.Admin.Models.Common;
using Grand.Admin.Models.Courses;
using Grand.Admin.Models.Customers;
using Grand.Admin.Models.Directory;
using Grand.Admin.Models.Discounts;
using Grand.Admin.Models.Documents;
using Grand.Admin.Models.ExternalAuthentication;
using Grand.Admin.Models.Forums;
using Grand.Admin.Models.Knowledgebase;
using Grand.Admin.Models.Localization;
using Grand.Admin.Models.Logging;
using Grand.Admin.Models.Messages;
using Grand.Admin.Models.News;
using Grand.Admin.Models.Orders;
using Grand.Admin.Models.Payments;
using Grand.Admin.Models.Plugins;
using Grand.Admin.Models.Polls;
using Grand.Admin.Models.Settings;
using Grand.Admin.Models.Shipping;
using Grand.Admin.Models.Stores;
using Grand.Admin.Models.Tax;
using Grand.Admin.Models.Templates;
using Grand.Admin.Models.Topics;
using Grand.Admin.Models.Vendors;
using System;
using System.Threading.Tasks;

namespace Grand.Admin.Extensions
{
    public static class AddressAttributeMappingExtensions
    {
        //attributes
        public static AddressAttributeModel ToModel(this AddressAttribute entity)
        {
            return entity.MapTo<AddressAttribute, AddressAttributeModel>();
        }

        public static AddressAttribute ToEntity(this AddressAttributeModel model)
        {
            return model.MapTo<AddressAttributeModel, AddressAttribute>();
        }

        public static AddressAttribute ToEntity(this AddressAttributeModel model, AddressAttribute destination)
        {
            return model.MapTo(destination);
        }

        //attributes value
        public static AddressAttributeValueModel ToModel(this AddressAttributeValue entity)
        {
            return entity.MapTo<AddressAttributeValue, AddressAttributeValueModel>();
        }
        public static AddressAttributeValue ToEntity(this AddressAttributeValueModel model)
        {
            return model.MapTo<AddressAttributeValueModel, AddressAttributeValue>();
        }

        public static AddressAttributeValue ToEntity(this AddressAttributeValueModel model, AddressAttributeValue destination)
        {
            return model.MapTo(destination);
        }
    }
}