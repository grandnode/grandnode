using MediatR;

namespace Grand.Web.Features.Models.Catalog
{
    public class GetManufacturerTemplateViewPath : IRequest<string>
    {
        public string TemplateId { get; set; }
    }
}
