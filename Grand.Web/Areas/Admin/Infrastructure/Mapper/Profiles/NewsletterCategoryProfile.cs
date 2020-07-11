using AutoMapper;
using Grand.Domain.Messages;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Messages;
using System.Collections.Generic;
using System.Linq;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class NewsletterCategoryProfile : Profile, IMapperProfile
    {
        public NewsletterCategoryProfile()
        {
            CreateMap<NewsletterCategory, NewsletterCategoryModel>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore());

            CreateMap<NewsletterCategoryModel, NewsletterCategory>()
                .ForMember(dest => dest.Locales, mo => mo.MapFrom(x => x.Locales.ToLocalizedProperty()))
                .ForMember(dest => dest.Stores, mo => mo.MapFrom(x => x.SelectedStoreIds != null ? x.SelectedStoreIds.ToList() : new List<string>()))
                .ForMember(dest => dest.Id, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}