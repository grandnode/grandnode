using Grand.Domain.Logging;
using MediatR;

namespace Grand.Services.Commands.Models.Common
{
    public class InsertLogCommand : IRequest<bool>
    {
        public LogLevel LogLevel { get; set; }
        public string ShortMessage { get; set; }
        public string FullMessage { get; set; }
    }
}
