using AutoMapper;
using Grand.Domain.Tax;
using Grand.Core.Mapper;
using Grand.Web.Areas.Admin.Models.Tax;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class TaxCategoryProfile : Profile, IAutoMapperProfile
    {
        public TaxCategoryProfile()
        {
            CreateMap<TaxCategory, TaxCategoryModel>();
            CreateMap<TaxCategoryModel, TaxCategory>()
                .ForMember(dest => dest.Id, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}