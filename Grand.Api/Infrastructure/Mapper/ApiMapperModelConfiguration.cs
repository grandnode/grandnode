using AutoMapper;
using Grand.Api.DTOs.Catalog;
using Grand.Api.DTOs.Common;
using Grand.Api.DTOs.Customers;
using Grand.Core.Domain.Catalog;
using Grand.Core.Domain.Common;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Media;
using Grand.Core.Infrastructure.Mapper;
using Grand.Services.Common;
using System;
using System.Linq;

namespace Grand.Api.Infrastructure.Mapper
{
    public class ApiMapperModelConfiguration : Profile, IMapperProfile
    {
        public ApiMapperModelConfiguration()
        {
            
            #region Specification attribute 

            CreateMap<SpecificationAttributeDto, SpecificationAttribute>()
                .ForMember(dest => dest.GenericAttributes, mo => mo.Ignore());

            CreateMap<SpecificationAttribute, SpecificationAttributeDto>();

            CreateMap<SpecificationAttributeOption, SpecificationAttributeOptionDto>();

            CreateMap<SpecificationAttributeOptionDto, SpecificationAttributeOption>()
                .ForMember(dest => dest.Locales, mo => mo.Ignore());

            #endregion

            #region Picture

            CreateMap<PictureDto, Picture>()
                .ForMember(dest => dest.GenericAttributes, mo=> mo.Ignore());

            CreateMap<Picture, PictureDto>()
                .ForMember(dest => dest.PictureBinary, mo => mo.Ignore());

            #endregion

            #region Tier prices

            CreateMap<ProductTierPriceDto, TierPrice>();

            CreateMap<TierPrice, ProductTierPriceDto>();

            #endregion
        }

        public int Order => 1;
    }
}
