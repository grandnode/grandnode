using AutoMapper;
using Grand.Domain.Messages;
using Grand.Core.Infrastructure.Mapper;
using Grand.Admin.Models.Messages;

namespace Grand.Admin.Infrastructure.Mapper.Profiles
{
    public class ContactUsProfile : Profile, IMapperProfile
    {
        public ContactUsProfile()
        {
            CreateMap<ContactUs, ContactFormModel>()
                .ForMember(dest => dest.CreatedOn, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}