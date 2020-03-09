using AutoMapper;
using Grand.Api.Infrastructure.Mapper;
using Grand.Core.Infrastructure.Mapper;

namespace Grand.Api.Tests.Helpers
{
    public static class Mapper
    {
        public static void Run()
        {
            var addressmapper = new AddressProfile();
            var categorymapper = new CategoryProfile();
            var customermapper = new CustomerProfile();
            var customerRolemapper = new CustomerRoleProfile();
            var manufacturermapper = new ManufacturerProfile();
            var picturemapper = new PictureProfile();
            var productAttributemapper = new ProductAttributeProfile();
            var productmapper = new ProductProfile();
            var specificationAttributemapper = new SpecificationAttributeProfile();
            var tierPricemapper = new TierPriceProfile();

            var config = new MapperConfiguration(cfg => {                
                cfg.AddProfile(addressmapper.GetType());
                cfg.AddProfile(categorymapper.GetType());
                cfg.AddProfile(customermapper.GetType());
                cfg.AddProfile(customerRolemapper.GetType());
                cfg.AddProfile(manufacturermapper.GetType());
                cfg.AddProfile(picturemapper.GetType());
                cfg.AddProfile(productAttributemapper.GetType());
                cfg.AddProfile(productmapper.GetType());
                cfg.AddProfile(specificationAttributemapper.GetType());
                cfg.AddProfile(tierPricemapper.GetType());

            });
            AutoMapperConfiguration.Init(config);
        }
    }
}
