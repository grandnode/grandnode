using AutoMapper;
using Grand.Domain.Vendors;
using Grand.Core.Infrastructure.Mapper;
using Grand.Services.Seo;
using Grand.Web.Areas.Admin.Models.Vendors;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class VendorProfile : Profile, IMapperProfile
    {
        public VendorProfile()
        {
            CreateMap<Vendor, VendorModel>()
                .ForMember(dest => dest.AssociatedCustomers, mo => mo.Ignore())
                .ForMember(dest => dest.AddVendorNoteMessage, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableDiscounts, mo => mo.Ignore())
                .ForMember(dest => dest.AvailableStores, mo => mo.Ignore())
                .ForMember(dest => dest.SelectedDiscountIds, mo => mo.Ignore())
                .ForMember(dest => dest.SeName, mo => mo.MapFrom(src => src.GetSeName("", true, false)));

            CreateMap<VendorModel, Vendor>()
                .ForMember(dest => dest.Id, mo => mo.Ignore())
                .ForMember(dest => dest.Locales, mo => mo.Ignore())
                .ForMember(dest => dest.Deleted, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}