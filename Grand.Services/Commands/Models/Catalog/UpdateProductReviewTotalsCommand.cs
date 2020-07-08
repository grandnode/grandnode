using Grand.Domain.Catalog;
using MediatR;

namespace Grand.Services.Commands.Models.Catalog
{
    public class UpdateProductReviewTotalsCommand : IRequest<bool>
    {
        public Product Product { get; set; }
    }
}
