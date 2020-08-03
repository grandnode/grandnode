using AutoMapper;
using Grand.Domain.Documents;
using Grand.Core.Infrastructure.Mapper;
using Grand.Web.Areas.Admin.Models.Documents;

namespace Grand.Web.Areas.Admin.Infrastructure.Mapper.Profiles
{
    public class DocumentTypeProfile : Profile, IMapperProfile
    {
        public DocumentTypeProfile()
        {
            CreateMap<DocumentType, DocumentTypeModel>();
            CreateMap<DocumentTypeModel, DocumentType>()
                .ForMember(dest => dest.Id, mo => mo.Ignore());
        }

        public int Order => 0;
    }
}