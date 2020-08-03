using AutoMapper;
using Grand.Domain.Topics;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Models.Templates;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class TopicTemplateProfile : Profile, IMapperProfile
    {
        public TopicTemplateProfile()
        {
            CreateMap<TopicTemplate, TopicTemplateModel>();
            CreateMap<TopicTemplateModel, TopicTemplate>()
                .ForMember(dest => dest.Id, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}