using MediatR;

namespace Grand.Services.Commands.Models.Orders
{
    public class PrepareOrderCodeCommand : IRequest<string>
    {
    }
}
