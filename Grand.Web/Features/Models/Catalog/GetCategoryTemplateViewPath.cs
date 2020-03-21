using MediatR;

namespace Grand.Web.Features.Models.Catalog
{
    public class GetCategoryTemplateViewPath : IRequest<string>
    {
        public string TemplateId { get; set; }
    }
}
