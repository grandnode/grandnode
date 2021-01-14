using AutoMapper;
using Grand.Domain.Topics;
using Grand.Core.Infrastructure.Mapper;
using Grand.Admin.Models.Templates;

namespace Grand.Admin.Infrastructure.Mapper.Profiles
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