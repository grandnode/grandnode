using AutoMapper;
using Grand.Domain.Tax;
using Grand.Core.Infrastructure.Mapper;
using Grand.Admin.Models.Tax;

namespace Grand.Admin.Infrastructure.Mapper.Profiles
{
    public class TaxCategoryProfile : Profile, IMapperProfile
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