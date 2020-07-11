using AutoMapper;
using Grand.Domain.Catalog;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Models.Catalog;


namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class ProductReviewProfile : Profile, IMapperProfile
    {
        public ProductReviewProfile()
        {
            CreateMap<ProductReviewModel, ProductReview>()
                .ForMember(dest => dest.HelpfulYesTotal, mo => mo.Ignore())
                .ForMember(dest => dest.HelpfulNoTotal, mo => mo.Ignore())
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.ProductReviewHelpfulnessEntries, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}