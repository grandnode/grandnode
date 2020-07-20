using Grand.Framework.Mvc.Models;
using System.Collections.Generic;

namespace Grand.Api.DTOs.Catalog
{
    public partial class SpecificationAttributeDto : BaseApiEntityModel
    {
        public SpecificationAttributeDto()
        {
            SpecificationAttributeOptions = new List<SpecificationAttributeOptionDto>();
        }
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
        public IList<SpecificationAttributeOptionDto> SpecificationAttributeOptions { get; set; }

    }
    public partial class SpecificationAttributeOptionDto : BaseApiEntityModel
    {
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
        public string ColorSquaresRgb { get; set; }
    }
}
