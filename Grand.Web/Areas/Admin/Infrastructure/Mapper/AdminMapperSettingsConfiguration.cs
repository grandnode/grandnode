using AutoMapper;
using Grand.Core.Domain.Blogs;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Forums;
using Grand.Core.Domain.Media;
using Grand.Core.Domain.News;
using Grand.Core.Domain.Orders;
using Grand.Core.Domain.Shipping;
using Grand.Core.Domain.Tax;
using Grand.Core.Domain.Vendors;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Models.Settings;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper
{
    public class AdminMapperSettingsConfiguration : Profile, IMapperProfile
    {
        public AdminMapperSettingsConfiguration()
        {

            CreateMap<AddressSettings, CustomerUserSettingsModel.AddressSettingsModel>()
                .ForMember(dest => dest.GenericAttributes, mo => mo.Ignore());
            CreateMap<CustomerUserSettingsModel.AddressSettingsModel, AddressSettings>();

        }

        public int Order
        {
            get { return 0; }
        }
    }
}