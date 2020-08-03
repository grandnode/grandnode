using System.Threading;
using System.Threading.Tasks;
using Grand.Domain.Blogs;
using Grand.Core.Events;
using Grand.Services.Seo;
using MediatR;

namespace Grand.Services.Events
{
    public class BlogPostDeletedEventHandler : INotificationHandler<EntityDeleted<BlogPost>>
    {
        private readonly IUrlRecordService _urlRecordService;
        
        public BlogPostDeletedEventHandler(IUrlRecordService urlRecordService)
        {
            _urlRecordService = urlRecordService;
        }
        public async Task Handle(EntityDeleted<BlogPost> notification, CancellationToken cancellationToken)
        {
            var urlToDelete = await _urlRecordService.GetBySlug(notification.Entity.SeName);
            await _urlRecordService.DeleteUrlRecord(urlToDelete);
        }
    }
}
