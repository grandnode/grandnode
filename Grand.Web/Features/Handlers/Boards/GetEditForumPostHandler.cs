using Grand.Domain.Customers;
using Grand.Domain.Forums;
using Grand.Core.Html;
using Grand.Services.Customers;
using Grand.Services.Forums;
using Grand.Services.Seo;
using Grand.Web.Features.Models.Boards;
using Grand.Web.Models.Boards;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Web.Features.Handlers.Boards
{
    public class GetEditForumPostHandler : IRequestHandler<GetEditForumPost, EditForumPostModel>
    {
        private readonly IForumService _forumService;
        private readonly ICustomerService _customerService;
        private readonly ForumSettings _forumSettings;
        private readonly CustomerSettings _customerSettings;

        public GetEditForumPostHandler(
            IForumService forumService,
            ICustomerService customerService,
            ForumSettings forumSettings,
            CustomerSettings customerSettings)
        {
            _forumService = forumService;
            _customerService = customerService;
            _forumSettings = forumSettings;
            _customerSettings = customerSettings;
        }

        public async Task<EditForumPostModel> Handle(GetEditForumPost request, CancellationToken cancellationToken)
        {
            var model = new EditForumPostModel {
                Id = "",
                ForumTopicId = request.ForumTopic.Id,
                IsEdit = false,
                ForumEditor = _forumSettings.ForumEditor,
                ForumName = request.Forum.Name,
                ForumTopicSubject = request.ForumTopic.Subject,
                ForumTopicSeName = request.ForumTopic.GetSeName(),
                IsCustomerAllowedToSubscribe = _forumService.IsCustomerAllowedToSubscribe(request.Customer),
                Subscribed = false,
            };

            //subscription            
            if (model.IsCustomerAllowedToSubscribe)
            {
                var forumSubscription = (await _forumService.GetAllSubscriptions(request.Customer.Id,
                    "", request.ForumTopic.Id, 0, 1)).FirstOrDefault();
                model.Subscribed = forumSubscription != null;
            }

            // Insert the quoted text
            string text = string.Empty;
            if (!String.IsNullOrEmpty(request.Quote))
            {
                var quotePost = await _forumService.GetPostById(request.Quote);
                if (quotePost != null && quotePost.TopicId == request.ForumTopic.Id)
                {
                    var quotePostText = quotePost.Text;
                    var customer = await _customerService.GetCustomerById(quotePost.CustomerId);
                    switch (_forumSettings.ForumEditor)
                    {
                        case EditorType.SimpleTextBox:
                            text = String.Format("{0}:\n{1}\n", customer.FormatUserName(_customerSettings.CustomerNameFormat), quotePostText);
                            break;
                        case EditorType.BBCodeEditor:
                            text = String.Format("[quote={0}]{1}[/quote]", customer.FormatUserName(_customerSettings.CustomerNameFormat), BBCodeHelper.RemoveQuotes(quotePostText));
                            break;
                    }
                    model.Text = text;
                }
            }
            return model;
        }
    }
}
