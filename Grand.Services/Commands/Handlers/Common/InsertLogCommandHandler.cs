using Grand.Services.Commands.Models.Common;
using Grand.Services.Logging;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Commands.Handlers.Common
{
    public class InsertLogCommandHandler : IRequestHandler<InsertLogCommand, bool>
    {
        private readonly ILogger _logger;

        public InsertLogCommandHandler(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<bool> Handle(InsertLogCommand request, CancellationToken cancellationToken)
        {
            await _logger.InsertLog(request.LogLevel, request.ShortMessage, request.FullMessage);
            return true;
        }
    }
}
