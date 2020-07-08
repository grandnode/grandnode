using Grand.Core;
using Grand.Domain.Customers;
using Grand.Domain.Forums;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Rss;
using Grand.Services.Customers;
using Grand.Services.Forums;
using Grand.Services.Localization;
using Grand.Services.Seo;
using Grand.Web.Features.Models.Boards;
using Grand.Web.Models.Boards;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Controllers
{
    public partial class BoardsController : BasePublicController
    {
        #region Fields

        private readonly IForumService _forumService;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IMediator _mediator;
        private readonly ForumSettings _forumSettings;

        #endregion

        #region Constructors

        public BoardsController(
            IForumService forumService,
            ILocalizationService localizationService,
            IWebHelper webHelper,
            IWorkContext workContext,
            IStoreContext storeContext,
            IMediator mediator,
            ForumSettings forumSettings)
        {
            _forumService = forumService;
            _localizationService = localizationService;
            _webHelper = webHelper;
            _workContext = workContext;
            _storeContext = storeContext;
            _mediator = mediator;
            _forumSettings = forumSettings;
        }
        #endregion

        #region Methods

        public virtual async Task<IActionResult> Index()
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }
            var model = await _mediator.Send(new GetBoardsIndex());
            return View(model);
        }


        public virtual async Task<IActionResult> ActiveDiscussions(string forumId = "", int pageNumber = 1)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }
            var model = await _mediator.Send(new GetForumActiveDiscussions() { ForumId = forumId, PageNumber = pageNumber });
            return View(model);
        }

        public virtual async Task<IActionResult> ActiveDiscussionsRss(string forumId = "")
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }

            if (!_forumSettings.ActiveDiscussionsFeedEnabled)
            {
                return RedirectToRoute("Boards");
            }

            var topics = await _forumService.GetActiveTopics(forumId, 0, _forumSettings.ActiveDiscussionsFeedCount);
            string url = Url.RouteUrl("ActiveDiscussionsRSS", null, _webHelper.IsCurrentConnectionSecured() ? "https" : "http");

            var feedTitle = _localizationService.GetResource("Forum.ActiveDiscussionsFeedTitle");
            var feedDescription = _localizationService.GetResource("Forum.ActiveDiscussionsFeedDescription");

            var feed = new RssFeed(
                                    string.Format(feedTitle, _storeContext.CurrentStore.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id)),
                                    feedDescription,
                                    new Uri(url),
                                    DateTime.UtcNow);

            var items = new List<RssItem>();

            var viewsText = _localizationService.GetResource("Forum.Views");
            var repliesText = _localizationService.GetResource("Forum.Replies");

            foreach (var topic in topics)
            {
                string topicUrl = Url.RouteUrl("TopicSlug", new { id = topic.Id, slug = topic.GetSeName() }, _webHelper.IsCurrentConnectionSecured() ? "https" : "http");
                string content = String.Format("{2}: {0}, {3}: {1}", topic.NumReplies.ToString(), topic.Views.ToString(), repliesText, viewsText);

                items.Add(new RssItem(topic.Subject, content, new Uri(topicUrl),
                    String.Format("urn:store:{0}:activeDiscussions:topic:{1}", _storeContext.CurrentStore.Id, topic.Id), (topic.LastPostTime ?? topic.UpdatedOnUtc)));
            }
            feed.Items = items;

            return new RssActionResult(feed, _webHelper.GetThisPageUrl(false));
        }

        public virtual async Task<IActionResult> ForumGroup(string id)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }

            var forumGroup = await _forumService.GetForumGroupById(id);
            if (forumGroup == null)
                return RedirectToRoute("Boards");

            var model = await _mediator.Send(new GetForumGroup() { ForumGroup = forumGroup });
            return View(model);
        }

        public virtual async Task<IActionResult> Forum(string id, int pageNumber = 1)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }
            var forum = await _forumService.GetForumById(id);

            if (forum != null)
            {
                var model = await _mediator.Send(new GetForumPage() {
                    Customer = _workContext.CurrentCustomer,
                    Forum = forum,
                    PageNumber = pageNumber
                });
                return View(model);
            }
            return RedirectToRoute("Boards");
        }

        public virtual async Task<IActionResult> ForumRss(string id)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }

            if (!_forumSettings.ForumFeedsEnabled)
            {
                return RedirectToRoute("Boards");
            }

            int topicLimit = _forumSettings.ForumFeedCount;
            var forum = await _forumService.GetForumById(id);

            if (forum != null)
            {
                //Order by newest topic posts & limit the number of topics to return
                var topics = await _forumService.GetAllTopics(forum.Id, "", string.Empty,
                     ForumSearchType.All, 0, 0, topicLimit);

                string url = Url.RouteUrl("ForumRSS", new { id = forum.Id }, _webHelper.IsCurrentConnectionSecured() ? "https" : "http");
                var feedTitle = _localizationService.GetResource("Forum.ForumFeedTitle");
                var feedDescription = _localizationService.GetResource("Forum.ForumFeedDescription");

                var feed = new RssFeed(
                                        string.Format(feedTitle, _storeContext.CurrentStore.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id), forum.Name),
                                        feedDescription,
                                        new Uri(url),
                                        DateTime.UtcNow);

                var items = new List<RssItem>();

                var viewsText = _localizationService.GetResource("Forum.Views");
                var repliesText = _localizationService.GetResource("Forum.Replies");

                foreach (var topic in topics)
                {
                    string topicUrl = Url.RouteUrl("TopicSlug", new { id = topic.Id, slug = topic.GetSeName() }, _webHelper.IsCurrentConnectionSecured() ? "https" : "http");
                    string content = string.Format("{2}: {0}, {3}: {1}", topic.NumReplies.ToString(), topic.Views.ToString(), repliesText, viewsText);

                    items.Add(new RssItem(topic.Subject, content, new Uri(topicUrl), String.Format("urn:store:{0}:forum:topic:{1}", _storeContext.CurrentStore.Id, topic.Id),
                        (topic.LastPostTime ?? topic.UpdatedOnUtc)));
                }

                feed.Items = items;

                return new RssActionResult(feed, _webHelper.GetThisPageUrl(false));
            }

            return new RssActionResult(new RssFeed(new Uri(_webHelper.GetStoreLocation())), _webHelper.GetThisPageUrl(false));
        }

        [HttpPost]
        public virtual async Task<IActionResult> ForumWatch(string id)
        {
            string watchTopic = _localizationService.GetResource("Forum.WatchForum");
            string unwatchTopic = _localizationService.GetResource("Forum.UnwatchForum");
            string returnText = watchTopic;

            var forum = await _forumService.GetForumById(id);
            if (forum == null)
            {
                return Json(new { Subscribed = false, Text = returnText, Error = true });
            }

            if (!_forumService.IsCustomerAllowedToSubscribe(_workContext.CurrentCustomer))
            {
                return Json(new { Subscribed = false, Text = returnText, Error = true });
            }

            var forumSubscription = (await _forumService.GetAllSubscriptions(_workContext.CurrentCustomer.Id,
                forum.Id, "", 0, 1)).FirstOrDefault();

            bool subscribed;
            if (forumSubscription == null)
            {
                forumSubscription = new ForumSubscription {
                    SubscriptionGuid = Guid.NewGuid(),
                    CustomerId = _workContext.CurrentCustomer.Id,
                    ForumId = forum.Id,
                    CreatedOnUtc = DateTime.UtcNow
                };
                await _forumService.InsertSubscription(forumSubscription);
                subscribed = true;
                returnText = unwatchTopic;
            }
            else
            {
                await _forumService.DeleteSubscription(forumSubscription);
                subscribed = false;
            }

            return Json(new { Subscribed = subscribed, Text = returnText, Error = false });
        }

        public virtual async Task<IActionResult> Topic(string id, int pageNumber = 1)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }

            var forumTopic = await _forumService.GetTopicById(id);

            if (forumTopic != null)
            {
                var model = await _mediator.Send(new GetForumTopicPage() {
                    Customer = _workContext.CurrentCustomer,
                    ForumTopic = forumTopic,
                    PageNumber = pageNumber
                });
                if (model == null && pageNumber > 1)
                {
                    return RedirectToRoute("TopicSlug", new { id = forumTopic.Id, slug = forumTopic.GetSeName() });
                }
                //update view count
                forumTopic.Views += 1;
                await _forumService.UpdateTopic(forumTopic);
                return View(model);
            }

            return RedirectToRoute("Boards");
        }

        [HttpPost]
        public virtual async Task<IActionResult> TopicWatch(string id)
        {
            string watchTopic = _localizationService.GetResource("Forum.WatchTopic");
            string unwatchTopic = _localizationService.GetResource("Forum.UnwatchTopic");
            string returnText = watchTopic;

            var forumTopic = await _forumService.GetTopicById(id);
            if (forumTopic == null)
            {
                return Json(new { Subscribed = false, Text = returnText, Error = true });
            }

            if (!_forumService.IsCustomerAllowedToSubscribe(_workContext.CurrentCustomer))
            {
                return Json(new { Subscribed = false, Text = returnText, Error = true });
            }

            var forumSubscription = (await _forumService.GetAllSubscriptions(_workContext.CurrentCustomer.Id,
                "", forumTopic.Id, 0, 1)).FirstOrDefault();

            bool subscribed;
            if (forumSubscription == null)
            {
                forumSubscription = new ForumSubscription {
                    SubscriptionGuid = Guid.NewGuid(),
                    CustomerId = _workContext.CurrentCustomer.Id,
                    TopicId = forumTopic.Id,
                    CreatedOnUtc = DateTime.UtcNow
                };
                await _forumService.InsertSubscription(forumSubscription);
                subscribed = true;
                returnText = unwatchTopic;
            }
            else
            {
                await _forumService.DeleteSubscription(forumSubscription);
                subscribed = false;
            }

            return Json(new { Subscribed = subscribed, Text = returnText, Error = false });
        }

        public virtual async Task<IActionResult> TopicMove(string id)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }

            var forumTopic = await _forumService.GetTopicById(id);

            if (forumTopic == null)
            {
                return RedirectToRoute("Boards");
            }
            var model = await _mediator.Send(new GetTopicMove() { ForumTopic = forumTopic });
            return View(model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public virtual async Task<IActionResult> TopicMove(TopicMoveModel model)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }

            var forumTopic = await _forumService.GetTopicById(model.Id);

            if (forumTopic == null)
            {
                return RedirectToRoute("Boards");
            }

            var newForumId = model.ForumSelected;
            var forum = await _forumService.GetForumById(newForumId);

            if (forum != null && forumTopic.ForumId != newForumId)
            {
                await _forumService.MoveTopic(forumTopic.Id, newForumId);
            }

            return RedirectToRoute("TopicSlug", new { id = forumTopic.Id, slug = forumTopic.GetSeName() });
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public virtual async Task<IActionResult> TopicDelete(string id)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return Json(new
                {
                    redirect = Url.RouteUrl("HomePage"),
                });
            }

            var forumTopic = await _forumService.GetTopicById(id);
            if (forumTopic != null)
            {
                if (!_forumService.IsCustomerAllowedToDeleteTopic(_workContext.CurrentCustomer, forumTopic))
                {
                    return new ChallengeResult();
                }
                var forum = await _forumService.GetForumById(forumTopic.ForumId);

                await _forumService.DeleteTopic(forumTopic);

                if (forum != null)
                {
                    return Json(new
                    {
                        redirect = Url.RouteUrl("ForumSlug", new { id = forum.Id, slug = forum.GetSeName() }),
                    });

                }
            }
            return Json(new
            {
                redirect = Url.RouteUrl("Boards"),
            });
        }

        public virtual async Task<IActionResult> TopicCreate(string id)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }
            var forum = await _forumService.GetForumById(id);
            if (forum == null)
            {
                return RedirectToRoute("Boards");
            }
            if (_forumService.IsCustomerAllowedToCreateTopic(_workContext.CurrentCustomer, forum) == false)
            {
                return new ChallengeResult();
            }
            var model = await _mediator.Send(new GetEditForumTopic() { Customer = _workContext.CurrentCustomer, Forum = forum });
            return View(model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public virtual async Task<IActionResult> TopicCreate(EditForumTopicModel model, [FromServices] ICustomerService customerService)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }

            var forum = await _forumService.GetForumById(model.ForumId);

            if (forum == null)
            {
                return RedirectToRoute("Boards");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (!_forumService.IsCustomerAllowedToCreateTopic(_workContext.CurrentCustomer, forum))
                    {
                        return new ChallengeResult();
                    }

                    string subject = model.Subject;
                    var maxSubjectLength = _forumSettings.TopicSubjectMaxLength;
                    if (maxSubjectLength > 0 && subject.Length > maxSubjectLength)
                    {
                        subject = subject.Substring(0, maxSubjectLength);
                    }

                    var text = model.Text;
                    var maxPostLength = _forumSettings.PostMaxLength;
                    if (maxPostLength > 0 && text.Length > maxPostLength)
                    {
                        text = text.Substring(0, maxPostLength);
                    }

                    var topicType = ForumTopicType.Normal;

                    string ipAddress = _webHelper.GetCurrentIpAddress();

                    var nowUtc = DateTime.UtcNow;

                    if (_forumService.IsCustomerAllowedToSetTopicPriority(_workContext.CurrentCustomer))
                    {
                        topicType = (ForumTopicType)Enum.ToObject(typeof(ForumTopicType), model.TopicTypeId);
                    }

                    //forum topic
                    var forumTopic = new ForumTopic {
                        ForumId = forum.Id,
                        ForumGroupId = forum.ForumGroupId,
                        CustomerId = _workContext.CurrentCustomer.Id,
                        TopicTypeId = (int)topicType,
                        Subject = subject,
                        CreatedOnUtc = nowUtc,
                        UpdatedOnUtc = nowUtc
                    };
                    await _forumService.InsertTopic(forumTopic, true);
                    if (!_workContext.CurrentCustomer.HasContributions)
                    {
                        await customerService.UpdateContributions(_workContext.CurrentCustomer);
                    }
                    //forum post
                    var forumPost = new ForumPost {
                        TopicId = forumTopic.Id,
                        ForumId = forum.Id,
                        ForumGroupId = forum.ForumGroupId,
                        CustomerId = _workContext.CurrentCustomer.Id,
                        Text = text,
                        IPAddress = ipAddress,
                        CreatedOnUtc = nowUtc,
                        UpdatedOnUtc = nowUtc
                    };
                    await _forumService.InsertPost(forumPost, false);

                    //update forum topic
                    forumTopic.NumPosts = 1;
                    forumTopic.LastPostId = forumPost.Id;
                    forumTopic.LastPostCustomerId = forumPost.CustomerId;
                    forumTopic.LastPostTime = forumPost.CreatedOnUtc;
                    forumTopic.UpdatedOnUtc = nowUtc;
                    await _forumService.UpdateTopic(forumTopic);

                    //subscription                
                    if (_forumService.IsCustomerAllowedToSubscribe(_workContext.CurrentCustomer))
                    {
                        if (model.Subscribed)
                        {
                            var forumSubscription = new ForumSubscription {
                                SubscriptionGuid = Guid.NewGuid(),
                                CustomerId = _workContext.CurrentCustomer.Id,
                                TopicId = forumTopic.Id,
                                CreatedOnUtc = nowUtc
                            };

                            await _forumService.InsertSubscription(forumSubscription);
                        }
                    }

                    return RedirectToRoute("TopicSlug", new { id = forumTopic.Id, slug = forumTopic.GetSeName() });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            // redisplay form
            model.TopicPriorities = await _mediator.Send(new GetTopicTypesList());
            model.IsEdit = false;
            model.ForumId = forum.Id;
            model.ForumName = forum.Name;
            model.ForumSeName = forum.GetSeName();
            model.Id = "";
            model.IsCustomerAllowedToSetTopicPriority = _forumService.IsCustomerAllowedToSetTopicPriority(_workContext.CurrentCustomer);
            model.IsCustomerAllowedToSubscribe = _forumService.IsCustomerAllowedToSubscribe(_workContext.CurrentCustomer);
            model.ForumEditor = _forumSettings.ForumEditor;

            return View(model);
        }

        public virtual async Task<IActionResult> TopicEdit(string id)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }

            var forumTopic = await _forumService.GetTopicById(id);

            if (forumTopic == null)
            {
                return RedirectToRoute("Boards");
            }

            if (!_forumService.IsCustomerAllowedToEditTopic(_workContext.CurrentCustomer, forumTopic))
            {
                return new ChallengeResult();
            }

            var forum = await _forumService.GetForumById(forumTopic.ForumId);
            if (forum == null)
            {
                return RedirectToRoute("Boards");
            }
            var model = await _mediator.Send(new GetEditForumTopic() { Customer = _workContext.CurrentCustomer, Forum = forum });
            var firstPost = await forumTopic.GetFirstPost(_forumService);
            model.Text = firstPost.Text;
            model.Subject = forumTopic.Subject;
            if (model.IsCustomerAllowedToSubscribe)
            {
                var forumSubscription = (await _forumService.GetAllSubscriptions(_workContext.CurrentCustomer.Id,
                    "", forumTopic.Id, 0, 1)).FirstOrDefault();
                model.Subscribed = forumSubscription != null;
            }
            return View(model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public virtual async Task<IActionResult> TopicEdit(EditForumTopicModel model)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }

            var forumTopic = await _forumService.GetTopicById(model.Id);

            if (forumTopic == null)
            {
                return RedirectToRoute("Boards");
            }
            var forum = await _forumService.GetForumById(forumTopic.ForumId);
            if (forum == null)
            {
                return RedirectToRoute("Boards");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (!_forumService.IsCustomerAllowedToEditTopic(_workContext.CurrentCustomer, forumTopic))
                    {
                        return new ChallengeResult();
                    }

                    string subject = model.Subject;
                    var maxSubjectLength = _forumSettings.TopicSubjectMaxLength;
                    if (maxSubjectLength > 0 && subject.Length > maxSubjectLength)
                    {
                        subject = subject.Substring(0, maxSubjectLength);
                    }

                    var text = model.Text;
                    var maxPostLength = _forumSettings.PostMaxLength;
                    if (maxPostLength > 0 && text.Length > maxPostLength)
                    {
                        text = text.Substring(0, maxPostLength);
                    }

                    var topicType = ForumTopicType.Normal;

                    string ipAddress = _webHelper.GetCurrentIpAddress();

                    DateTime nowUtc = DateTime.UtcNow;

                    if (_forumService.IsCustomerAllowedToSetTopicPriority(_workContext.CurrentCustomer))
                    {
                        topicType = (ForumTopicType)Enum.ToObject(typeof(ForumTopicType), model.TopicTypeId);
                    }

                    //forum topic
                    forumTopic.TopicTypeId = (int)topicType;
                    forumTopic.Subject = subject;
                    forumTopic.UpdatedOnUtc = nowUtc;
                    await _forumService.UpdateTopic(forumTopic);

                    //forum post                
                    var firstPost = await forumTopic.GetFirstPost(_forumService);
                    if (firstPost != null)
                    {
                        firstPost.Text = text;
                        firstPost.UpdatedOnUtc = nowUtc;
                        await _forumService.UpdatePost(firstPost);
                    }
                    else
                    {
                        //error (not possible)
                        firstPost = new ForumPost {
                            TopicId = forumTopic.Id,
                            ForumId = forum.Id,
                            ForumGroupId = forum.ForumGroupId,
                            CustomerId = forumTopic.CustomerId,
                            Text = text,
                            IPAddress = ipAddress,
                            UpdatedOnUtc = nowUtc
                        };

                        await _forumService.InsertPost(firstPost, false);
                    }

                    //subscription
                    if (_forumService.IsCustomerAllowedToSubscribe(_workContext.CurrentCustomer))
                    {
                        var forumSubscription = (await _forumService.GetAllSubscriptions(_workContext.CurrentCustomer.Id,
                            "", forumTopic.Id, 0, 1)).FirstOrDefault();
                        if (model.Subscribed)
                        {
                            if (forumSubscription == null)
                            {
                                forumSubscription = new ForumSubscription {
                                    SubscriptionGuid = Guid.NewGuid(),
                                    CustomerId = _workContext.CurrentCustomer.Id,
                                    TopicId = forumTopic.Id,
                                    CreatedOnUtc = nowUtc
                                };

                                await _forumService.InsertSubscription(forumSubscription);
                            }
                        }
                        else
                        {
                            if (forumSubscription != null)
                            {
                                await _forumService.DeleteSubscription(forumSubscription);
                            }
                        }
                    }

                    // redirect to the topic page with the topic slug
                    return RedirectToRoute("TopicSlug", new { id = forumTopic.Id, slug = forumTopic.GetSeName() });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            // redisplay form
            model.TopicPriorities = await _mediator.Send(new GetTopicTypesList());
            model.IsEdit = true;
            model.ForumName = forum.Name;
            model.ForumSeName = forum.GetSeName();
            model.ForumId = forum.Id;
            model.ForumEditor = _forumSettings.ForumEditor;

            model.IsCustomerAllowedToSetTopicPriority = _forumService.IsCustomerAllowedToSetTopicPriority(_workContext.CurrentCustomer);
            model.IsCustomerAllowedToSubscribe = _forumService.IsCustomerAllowedToSubscribe(_workContext.CurrentCustomer);

            return View(model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public virtual async Task<IActionResult> PostDelete(string id)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return Json(new
                {
                    redirect = Url.RouteUrl("HomePage"),
                });
            }

            var forumPost = await _forumService.GetPostById(id);

            if (forumPost != null)
            {
                if (!_forumService.IsCustomerAllowedToDeletePost(_workContext.CurrentCustomer, forumPost))
                {
                    return new ChallengeResult();
                }

                var forumTopic = await _forumService.GetTopicById(forumPost.TopicId);
                var forumId = forumTopic.ForumId;
                var forumSlug = (await _forumService.GetForumById(forumTopic.ForumId)).GetSeName();

                await _forumService.DeletePost(forumPost);

                //get topic one more time because it can be deleted (first or only post deleted)
                forumTopic = await _forumService.GetTopicById(forumPost.TopicId);
                if (forumTopic == null)
                {
                    return Json(new
                    {
                        redirect = Url.RouteUrl("ForumSlug", new { id = forumId, slug = forumSlug }),
                    });
                }
                return Json(new
                {
                    redirect = Url.RouteUrl("TopicSlug", new { id = forumTopic.Id, slug = forumTopic.GetSeName() }),
                });
            }

            return Json(new
            {
                redirect = Url.RouteUrl("Boards"),
            });
        }

        public virtual async Task<IActionResult> PostCreate(string id, string quote)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }

            var forumTopic = await _forumService.GetTopicById(id);

            if (forumTopic == null)
            {
                return RedirectToRoute("Boards");
            }

            if (!_forumService.IsCustomerAllowedToCreatePost(_workContext.CurrentCustomer, forumTopic))
            {
                return new ChallengeResult();
            }

            var forum = await _forumService.GetForumById(forumTopic.ForumId);
            if (forum == null)
            {
                return RedirectToRoute("Boards");
            }
            var model = await _mediator.Send(new GetEditForumPost() { 
                Customer = _workContext.CurrentCustomer,
                Forum = forum,
                ForumTopic = forumTopic,
                Quote = quote
            });
            return View(model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public virtual async Task<IActionResult> PostCreate(EditForumPostModel model, [FromServices] ICustomerService customerService)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }

            var forumTopic = await _forumService.GetTopicById(model.ForumTopicId);
            if (forumTopic == null)
            {
                return RedirectToRoute("Boards");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (!_forumService.IsCustomerAllowedToCreatePost(_workContext.CurrentCustomer, forumTopic))
                        return new ChallengeResult();

                    var text = model.Text;
                    var maxPostLength = _forumSettings.PostMaxLength;
                    if (maxPostLength > 0 && text.Length > maxPostLength)
                        text = text.Substring(0, maxPostLength);

                    string ipAddress = _webHelper.GetCurrentIpAddress();

                    DateTime nowUtc = DateTime.UtcNow;

                    var forumPost = new ForumPost {
                        TopicId = forumTopic.Id,
                        ForumId = forumTopic.ForumId,
                        ForumGroupId = forumTopic.ForumGroupId,
                        CustomerId = _workContext.CurrentCustomer.Id,
                        Text = text,
                        IPAddress = ipAddress,
                        CreatedOnUtc = nowUtc,
                        UpdatedOnUtc = nowUtc
                    };
                    await _forumService.InsertPost(forumPost, true);
                    if (!_workContext.CurrentCustomer.HasContributions)
                    {
                        await customerService.UpdateContributions(_workContext.CurrentCustomer);
                    }

                    //subscription
                    if (_forumService.IsCustomerAllowedToSubscribe(_workContext.CurrentCustomer))
                    {
                        var forumSubscription = (await _forumService.GetAllSubscriptions(_workContext.CurrentCustomer.Id,
                            "", forumPost.TopicId, 0, 1)).FirstOrDefault();
                        if (model.Subscribed)
                        {
                            if (forumSubscription == null)
                            {
                                forumSubscription = new ForumSubscription {
                                    SubscriptionGuid = Guid.NewGuid(),
                                    CustomerId = _workContext.CurrentCustomer.Id,
                                    TopicId = forumPost.TopicId,
                                    CreatedOnUtc = nowUtc
                                };

                                await _forumService.InsertSubscription(forumSubscription);
                            }
                        }
                        else
                        {
                            if (forumSubscription != null)
                            {
                                await _forumService.DeleteSubscription(forumSubscription);
                            }
                        }
                    }

                    int pageSize = _forumSettings.PostsPageSize > 0 ? _forumSettings.PostsPageSize : 10;

                    int pageIndex = (await _forumService.CalculateTopicPageIndex(forumPost.TopicId, pageSize, forumPost.Id) + 1);
                    var url = string.Empty;
                    var _forumTopic = await _forumService.GetTopicById(forumPost.TopicId);
                    if (pageIndex > 1)
                    {
                        url = Url.RouteUrl("TopicSlugPaged", new { id = forumPost.TopicId, slug = _forumTopic.GetSeName(), pageNumber = pageIndex });
                    }
                    else
                    {
                        url = Url.RouteUrl("TopicSlug", new { id = forumPost.TopicId, slug = _forumTopic.GetSeName() });
                    }
                    return Redirect($"{url}#{forumPost.Id}");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            // redisplay form
            var forum = await _forumService.GetForumById(forumTopic.ForumId);
            if (forum == null)
                return RedirectToRoute("Boards");

            model.IsEdit = false;
            model.ForumName = forum.Name;
            model.ForumTopicId = forumTopic.Id;
            model.ForumTopicSubject = forumTopic.Subject;
            model.ForumTopicSeName = forumTopic.GetSeName();
            model.Id = "";
            model.IsCustomerAllowedToSubscribe = _forumService.IsCustomerAllowedToSubscribe(_workContext.CurrentCustomer);
            model.ForumEditor = _forumSettings.ForumEditor;

            return View(model);
        }

        public virtual async Task<IActionResult> PostEdit(string id)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }

            var forumPost = await _forumService.GetPostById(id);

            if (forumPost == null)
            {
                return RedirectToRoute("Boards");
            }
            if (!_forumService.IsCustomerAllowedToEditPost(_workContext.CurrentCustomer, forumPost))
            {
                return new ChallengeResult();
            }
            var forumTopic = await _forumService.GetTopicById(forumPost.TopicId);
            if (forumTopic == null)
            {
                return RedirectToRoute("Boards");
            }
            var forum = await _forumService.GetForumById(forumTopic.ForumId);
            if (forum == null)
            {
                return RedirectToRoute("Boards");
            }

            var model = new EditForumPostModel {
                Id = forumPost.Id,
                ForumTopicId = forumTopic.Id,
                IsEdit = true,
                ForumEditor = _forumSettings.ForumEditor,
                ForumName = forum.Name,
                ForumTopicSubject = forumTopic.Subject,
                ForumTopicSeName = forumTopic.GetSeName(),
                IsCustomerAllowedToSubscribe = _forumService.IsCustomerAllowedToSubscribe(_workContext.CurrentCustomer),
                Subscribed = false,
                Text = forumPost.Text,
            };

            //subscription
            if (model.IsCustomerAllowedToSubscribe)
            {
                var forumSubscription = (await _forumService.GetAllSubscriptions(_workContext.CurrentCustomer.Id,
                    "", forumTopic.Id, 0, 1)).FirstOrDefault();
                model.Subscribed = forumSubscription != null;
            }

            return View(model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public virtual async Task<IActionResult> PostEdit(EditForumPostModel model)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }

            var forumPost = await _forumService.GetPostById(model.Id);
            if (forumPost == null)
            {
                return RedirectToRoute("Boards");
            }

            if (!_forumService.IsCustomerAllowedToEditPost(_workContext.CurrentCustomer, forumPost))
            {
                return new ChallengeResult();
            }

            var forumTopic = await _forumService.GetTopicById(forumPost.TopicId);
            if (forumTopic == null)
            {
                return RedirectToRoute("Boards");
            }

            var forum = await _forumService.GetForumById(forumTopic.ForumId);
            if (forum == null)
            {
                return RedirectToRoute("Boards");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    DateTime nowUtc = DateTime.UtcNow;

                    var text = model.Text;
                    var maxPostLength = _forumSettings.PostMaxLength;
                    if (maxPostLength > 0 && text.Length > maxPostLength)
                    {
                        text = text.Substring(0, maxPostLength);
                    }

                    forumPost.UpdatedOnUtc = nowUtc;
                    forumPost.Text = text;
                    await _forumService.UpdatePost(forumPost);

                    //subscription
                    if (_forumService.IsCustomerAllowedToSubscribe(_workContext.CurrentCustomer))
                    {
                        var forumSubscription = (await _forumService.GetAllSubscriptions(_workContext.CurrentCustomer.Id,
                            "", forumPost.TopicId, 0, 1)).FirstOrDefault();
                        if (model.Subscribed)
                        {
                            if (forumSubscription == null)
                            {
                                forumSubscription = new ForumSubscription {
                                    SubscriptionGuid = Guid.NewGuid(),
                                    CustomerId = _workContext.CurrentCustomer.Id,
                                    TopicId = forumPost.TopicId,
                                    CreatedOnUtc = nowUtc
                                };
                                await _forumService.InsertSubscription(forumSubscription);
                            }
                        }
                        else
                        {
                            if (forumSubscription != null)
                            {
                                await _forumService.DeleteSubscription(forumSubscription);
                            }
                        }
                    }

                    int pageSize = _forumSettings.PostsPageSize > 0 ? _forumSettings.PostsPageSize : 10;
                    int pageIndex = (await _forumService.CalculateTopicPageIndex(forumPost.TopicId, pageSize, forumPost.Id) + 1);
                    var url = string.Empty;
                    var forumtopic = await _forumService.GetTopicById(forumPost.TopicId);
                    if (pageIndex > 1)
                    {
                        url = Url.RouteUrl("TopicSlugPaged", new { id = forumPost.TopicId, slug = forumtopic.GetSeName(), page = pageIndex });
                    }
                    else
                    {
                        url = Url.RouteUrl("TopicSlug", new { id = forumPost.TopicId, slug = forumtopic.GetSeName() });
                    }
                    return Redirect(string.Format("{0}#{1}", url, forumPost.Id));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            //redisplay form
            model.IsEdit = true;
            model.ForumName = forum.Name;
            model.ForumTopicId = forumTopic.Id;
            model.ForumTopicSubject = forumTopic.Subject;
            model.ForumTopicSeName = forumTopic.GetSeName();
            model.Id = forumPost.Id;
            model.IsCustomerAllowedToSubscribe = _forumService.IsCustomerAllowedToSubscribe(_workContext.CurrentCustomer);
            model.ForumEditor = _forumSettings.ForumEditor;

            return View(model);
        }

        public virtual async Task<IActionResult> Search(string searchterms, bool? adv, string forumId,
            string within, string limitDays, int pageNumber = 1)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }
            var model = await _mediator.Send(new GetSearch() { 
                Searchterms = searchterms,
                Adv = adv,
                ForumId = forumId,
                Within =within,
                LimitDays = limitDays,
                PageNumber = pageNumber,
            });
            return View(model);
        }


        public virtual async Task<IActionResult> CustomerForumSubscriptions(int? pageNumber)
        {
            if (!_forumSettings.AllowCustomersToManageSubscriptions)
            {
                return RedirectToRoute("CustomerInfo");
            }

            int pageIndex = 0;
            if (pageNumber > 0)
            {
                pageIndex = pageNumber.Value - 1;
            }
            var model = await _mediator.Send(new GetCustomerForumSubscriptions() { 
                Customer = _workContext.CurrentCustomer,
                PageIndex = pageIndex
            });
            return View(model);
        }
        [HttpPost, ActionName("CustomerForumSubscriptions")]
        public virtual async Task<IActionResult> CustomerForumSubscriptionsPOST(IFormCollection formCollection)
        {
            foreach (var key in formCollection.Keys)
            {
                var value = formCollection[key];

                if (value.Equals("on") && key.StartsWith("fs", StringComparison.OrdinalIgnoreCase))
                {
                    var id = key.Replace("fs", "").Trim();
                    var forumSubscription = await _forumService.GetSubscriptionById(id);
                    if (forumSubscription != null && forumSubscription.CustomerId == _workContext.CurrentCustomer.Id)
                    {
                        await _forumService.DeleteSubscription(forumSubscription);
                    }
                }
            }

            return RedirectToRoute("CustomerForumSubscriptions");
        }

        [HttpPost]
        public virtual async Task<IActionResult> PostVote(string postId, bool isUp)
        {
            if (!_forumSettings.AllowPostVoting)
                return new NullJsonResult();

            var forumPost = await _forumService.GetPostById(postId);
            if (forumPost == null)
                return new NullJsonResult();

            if (!_workContext.CurrentCustomer.IsRegistered())
                return Json(new
                {
                    Error = _localizationService.GetResource("Forum.Votes.Login"),
                    VoteCount = forumPost.VoteCount
                });

            if (_workContext.CurrentCustomer.Id == forumPost.CustomerId)
                return Json(new
                {
                    Error = _localizationService.GetResource("Forum.Votes.OwnPost"),
                    VoteCount = forumPost.VoteCount
                });

            var forumPostVote = await _forumService.GetPostVote(postId, _workContext.CurrentCustomer.Id);
            if (forumPostVote != null)
            {
                if ((forumPostVote.IsUp && isUp) || (!forumPostVote.IsUp && !isUp))
                    return Json(new
                    {
                        Error = _localizationService.GetResource("Forum.Votes.AlreadyVoted"),
                        VoteCount = forumPost.VoteCount
                    });
                else
                {
                    await _forumService.DeletePostVote(forumPostVote);
                    forumPost = await _forumService.GetPostById(postId);
                    return Json(new { VoteCount = forumPost.VoteCount });
                }
            }

            if (await _forumService.GetNumberOfPostVotes(_workContext.CurrentCustomer.Id, DateTime.UtcNow.AddDays(-1)) >= _forumSettings.MaxVotesPerDay)
                return Json(new
                {
                    Error = string.Format(_localizationService.GetResource("Forum.Votes.MaxVotesReached"), _forumSettings.MaxVotesPerDay),
                    VoteCount = forumPost.VoteCount
                });


            await _forumService.InsertPostVote(new ForumPostVote {
                CustomerId = _workContext.CurrentCustomer.Id,
                ForumPostId = postId,
                IsUp = isUp,
                CreatedOnUtc = DateTime.UtcNow
            });
            forumPost = await _forumService.GetPostById(postId);
            return Json(new { VoteCount = forumPost.VoteCount, IsUp = isUp });
        }

        #endregion
    }
}
