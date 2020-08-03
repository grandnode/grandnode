using Grand.Domain.Orders;
using Grand.Services.Commands.Models.Orders;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Orders
{
    public class PrepareOrderCodeCommandHandler : IRequestHandler<PrepareOrderCodeCommand, string>
    {
        private readonly OrderSettings _orderSettings;

        public PrepareOrderCodeCommandHandler(OrderSettings orderSettings)
        {
            _orderSettings = orderSettings;
        }

        public async Task<string> Handle(PrepareOrderCodeCommand request, CancellationToken cancellationToken)
        {
            var length = _orderSettings.LengthCode;
            if (length == 0)
                length = 8;

            var code = RandomString(length);
            return await Task.FromResult(code);
        }

        private string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        private static Random random = new Random();
    }
}
