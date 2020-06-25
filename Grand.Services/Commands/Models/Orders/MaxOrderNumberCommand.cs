using MediatR;

namespace Grand.Services.Commands.Models.Orders
{
    public class MaxOrderNumberCommand : IRequest<int?>
    {
        public int? OrderNumber { get; set; }
    }
}
