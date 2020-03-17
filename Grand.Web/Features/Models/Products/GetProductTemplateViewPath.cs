using MediatR;

namespace Grand.Web.Features.Models.Products
{
    public class GetProductTemplateViewPath : IRequest<string>
    {
        public string ProductTemplateId { get; set; }
    }
}
