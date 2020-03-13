using MediatR;
using System.Collections.Generic;

namespace Grand.Web.Commands.Models.Newsletter
{
    public class SubscriptionCategoryCommandModel : IRequest<(string message, bool success)>
    {
        public IDictionary<string, string> Values { get; set; }
    }
}
