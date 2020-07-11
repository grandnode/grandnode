using AutoMapper;
using Grand.Domain.Forums;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Models.Forums;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class ForumGroupProfile : Profile, IMapperProfile
    {
        public ForumGroupProfile()
        {
            CreateMap<ForumGroup, ForumGroupModel>()
                .ForMember(dest => dest.CreatedOn, mo => mo.Ignore());

            CreateMap<ForumGroupModel, ForumGroup>()
                .ForMember(dest => dest.Id, mo => mo.Ignore())
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore());

            CreateMap<Forum, ForumModel>()
                .ForMember(dest => dest.CreatedOn, mo => mo.Ignore())
                .ForMember(dest => dest.ForumGroups, mo => mo.Ignore());

            CreateMap<ForumModel, Forum>()
                .ForMember(dest => dest.Id, mo => mo.Ignore())
                .ForMember(dest => dest.NumTopics, mo => mo.Ignore())
                .ForMember(dest => dest.NumPosts, mo => mo.Ignore())
                .ForMember(dest => dest.LastTopicId, mo => mo.Ignore())
                .ForMember(dest => dest.LastPostId, mo => mo.Ignore())
                .ForMember(dest => dest.LastPostCustomerId, mo => mo.Ignore())
                .ForMember(dest => dest.LastPostTime, mo => mo.Ignore())
                .ForMember(dest => dest.CreatedOnUtc, mo => mo.Ignore())
                .ForMember(dest => dest.UpdatedOnUtc, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}