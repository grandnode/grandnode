using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Grand.Core;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Forums;
using Grand.Services.Customers;
using Grand.Services.Forums;
using Grand.Services.Localization;
using Grand.Services.Seo;
using Grand.Framework.Security;
using Grand.Web.Models.Boards;
using Grand.Core.Infrastructure;
using Grand.Framework.Mvc;
using Grand.Web.Services;
using Grand.Framework.Mvc.Rss;
using Microsoft.AspNetCore.Http;

namespace Grand.Web.Controllers
{
    public partial class BoardsController : BasePublicController
    {
        #region Fields

        private readonly IBoardsWebService _boardsWebService;
        private readonly IForumService _forumService;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ForumSettings _forumSettings;

        #endregion

        #region Constructors

        public BoardsController(IBoardsWebService boardsWebService,
            IForumService forumService,
            ILocalizationService localizationService,
            IWebHelper webHelper,
            IWorkContext workContext,
            IStoreContext storeContext,
            ForumSettings forumSettings)
        {
            this._boardsWebService = boardsWebService;
            this._forumService = forumService;
            this._localizationService = localizationService;
            this._webHelper = webHelper;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._forumSettings = forumSettings;
        }
        #endregion

        #region Methods

        public virtual IActionResult Index()
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }
            var model = _boardsWebService.PrepareBoardsIndex();
            return View(model);
        }

       
        public virtual IActionResult ActiveDiscussions(string forumId = "", int pageNumber = 1)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }
            var model = _boardsWebService.PrepareActiveDiscussions(forumId, pageNumber);
            return View(model);
        }

        public virtual IActionResult ActiveDiscussionsRss(string forumId = "")
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }

            if (!_forumSettings.ActiveDiscussionsFeedEnabled)
            {
                return RedirectToRoute("Boards");
            }

            var topics = _forumService.GetActiveTopics(forumId, 0, _forumSettings.ActiveDiscussionsFeedCount);
            string url = Url.RouteUrl("ActiveDiscussionsRSS", null, _webHelper.IsCurrentConnectionSecured() ? "https" : "http");

            var feedTitle = _localizationService.GetResource("Forum.ActiveDiscussionsFeedTitle");
            var feedDescription = _localizationService.GetResource("Forum.ActiveDiscussionsFeedDescription");

            var feed = new RssFeed(
                                    string.Format(feedTitle, _storeContext.CurrentStore.GetLocalized(x => x.Name)),
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

        public virtual IActionResult ForumGroup(string id)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }

            var forumGroup = _forumService.GetForumGroupById(id);
            if (forumGroup == null)
                return RedirectToRoute("Boards");

            var model = _boardsWebService.PrepareForumGroup(forumGroup);
            return View(model);
        }

        public virtual IActionResult Forum(string id, int pageNumber = 1)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }

            var forum = _forumService.GetForumById(id);

            if (forum != null)
            {
                var model = _boardsWebService.PrepareForumPage(forum, pageNumber);
                return View(model);
            }
            return RedirectToRoute("Boards");
        }

        public virtual IActionResult ForumRss(string id)
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
            var forum = _forumService.GetForumById(id);

            if (forum != null)
            {
                //Order by newest topic posts & limit the number of topics to return
                var topics = _forumService.GetAllTopics(forum.Id, "", string.Empty,
                     ForumSearchType.All, 0, 0, topicLimit);

                string url = Url.RouteUrl("ForumRSS", new { id = forum.Id }, _webHelper.IsCurrentConnectionSecured() ? "https" : "http");
                var feedTitle = _localizationService.GetResource("Forum.ForumFeedTitle");
                var feedDescription = _localizationService.GetResource("Forum.ForumFeedDescription");

                var feed = new RssFeed(
                                        string.Format(feedTitle, _storeContext.CurrentStore.GetLocalized(x => x.Name), forum.Name),
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
        public virtual IActionResult ForumWatch(string id)
        {
            string watchTopic = _localizationService.GetResource("Forum.WatchForum");
            string unwatchTopic = _localizationService.GetResource("Forum.UnwatchForum");
            string returnText = watchTopic;

            var forum = _forumService.GetForumById(id);
            if (forum == null)
            {
                return Json(new { Subscribed = false, Text = returnText, Error = true });
            }

            if (!_forumService.IsCustomerAllowedToSubscribe(_workContext.CurrentCustomer))
            {
                return Json(new { Subscribed = false, Text = returnText, Error = true });
            }

            var forumSubscription = _forumService.GetAllSubscriptions(_workContext.CurrentCustomer.Id,
                forum.Id, "", 0, 1).FirstOrDefault();

            bool subscribed;
            if (forumSubscription == null)
            {
                forumSubscription = new ForumSubscription
                {
                    SubscriptionGuid = Guid.NewGuid(),
                    CustomerId = _workContext.CurrentCustomer.Id,
                    ForumId = forum.Id,
                    CreatedOnUtc = DateTime.UtcNow
                };
                _forumService.InsertSubscription(forumSubscription);
                subscribed = true;
                returnText = unwatchTopic;
            }
            else
            {
                _forumService.DeleteSubscription(forumSubscription);
                subscribed = false;
            }

            return Json(new { Subscribed = subscribed, Text = returnText, Error = false });
        }

        public virtual IActionResult Topic(string id, int pageNumber = 1)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }

            var forumTopic = _forumService.GetTopicById(id);

            if (forumTopic != null)
            {
                var model = _boardsWebService.PrepareForumTopicPage(forumTopic, pageNumber);
                if (model == null && pageNumber > 1)
                {
                    return RedirectToRoute("TopicSlug", new {id = forumTopic.Id, slug = forumTopic.GetSeName()});
                }
                //update view count
                forumTopic.Views += 1;
                _forumService.UpdateTopic(forumTopic);
                return View(model);
            }

            return RedirectToRoute("Boards");
        }

        [HttpPost]
        public virtual IActionResult TopicWatch(string id)
        {
            string watchTopic = _localizationService.GetResource("Forum.WatchTopic");
            string unwatchTopic = _localizationService.GetResource("Forum.UnwatchTopic");
            string returnText = watchTopic;

            var forumTopic = _forumService.GetTopicById(id);
            if (forumTopic == null)
            {
                return Json(new { Subscribed = false, Text = returnText, Error = true });
            }

            if (!_forumService.IsCustomerAllowedToSubscribe(_workContext.CurrentCustomer))
            {
                return Json(new { Subscribed = false, Text = returnText, Error = true });
            }

            var forumSubscription = _forumService.GetAllSubscriptions(_workContext.CurrentCustomer.Id,
                "", forumTopic.Id, 0, 1).FirstOrDefault();

            bool subscribed;
            if (forumSubscription == null)
            {
                forumSubscription = new ForumSubscription
                {
                    SubscriptionGuid = Guid.NewGuid(),
                    CustomerId = _workContext.CurrentCustomer.Id,
                    TopicId = forumTopic.Id,
                    CreatedOnUtc = DateTime.UtcNow
                };
                _forumService.InsertSubscription(forumSubscription);
                subscribed = true;
                returnText = unwatchTopic;
            }
            else
            {
                _forumService.DeleteSubscription(forumSubscription);
                subscribed = false;
            }

            return Json(new { Subscribed = subscribed, Text = returnText, Error = false });
        }

        public virtual IActionResult TopicMove(string id)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }

            var forumTopic = _forumService.GetTopicById(id);

            if (forumTopic == null)
            {
                return RedirectToRoute("Boards");
            }
            var model = _boardsWebService.PrepareTopicMove(forumTopic);
            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        public virtual IActionResult TopicMove(TopicMoveModel model)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }

            var forumTopic = _forumService.GetTopicById(model.Id);

            if (forumTopic == null)
            {
                return RedirectToRoute("Boards");
            }

            var newForumId = model.ForumSelected;
            var forum = _forumService.GetForumById(newForumId);

            if (forum != null && forumTopic.ForumId != newForumId)
            {
                _forumService.MoveTopic(forumTopic.Id, newForumId);
            }

            return RedirectToRoute("TopicSlug", new { id = forumTopic.Id, slug = forumTopic.GetSeName() });
        }

        [HttpPost]
        [PublicAntiForgery]
        public virtual IActionResult TopicDelete(string id)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return Json(new
                {
                    redirect = Url.RouteUrl("HomePage"),
                });
            }

            var forumTopic = _forumService.GetTopicById(id);
            if (forumTopic != null)
            {
                if (!_forumService.IsCustomerAllowedToDeleteTopic(_workContext.CurrentCustomer, forumTopic))
                {
                    return new ChallengeResult();
                }
                var forum = _forumService.GetForumById(forumTopic.ForumId);

                _forumService.DeleteTopic(forumTopic);

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

        public virtual IActionResult TopicCreate(string id)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }
            var forum = _forumService.GetForumById(id);
            if (forum == null)
            {
                return RedirectToRoute("Boards");
            }
            if (_forumService.IsCustomerAllowedToCreateTopic(_workContext.CurrentCustomer, forum) == false)
            {
                return new ChallengeResult();
            }
            var model = _boardsWebService.PrepareEditForumTopic(forum);
            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        public virtual IActionResult TopicCreate(EditForumTopicModel model)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }

            var forum = _forumService.GetForumById(model.ForumId);

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
                        topicType = (ForumTopicType) Enum.ToObject(typeof (ForumTopicType), model.TopicTypeId);
                    }

                    //forum topic
                    var forumTopic = new ForumTopic
                    {
                        ForumId = forum.Id,
                        ForumGroupId = forum.ForumGroupId,
                        CustomerId = _workContext.CurrentCustomer.Id,
                        TopicTypeId = (int) topicType,
                        Subject = subject,
                        CreatedOnUtc = nowUtc,
                        UpdatedOnUtc = nowUtc
                    };
                    _forumService.InsertTopic(forumTopic, true);
                    if(!_workContext.CurrentCustomer.IsHasForumTopic)
                    {
                        _workContext.CurrentCustomer.IsHasForumTopic = true;
                        EngineContext.Current.Resolve<ICustomerService>().UpdateHasForumTopic(_workContext.CurrentCustomer.Id);
                    }
                    //forum post
                    var forumPost = new ForumPost
                    {
                        TopicId = forumTopic.Id,
                        ForumId = forum.Id,
                        ForumGroupId = forum.ForumGroupId,
                        CustomerId = _workContext.CurrentCustomer.Id,
                        Text = text,
                        IPAddress = ipAddress,
                        CreatedOnUtc = nowUtc,
                        UpdatedOnUtc = nowUtc
                    };
                    _forumService.InsertPost(forumPost, false);

                    //update forum topic
                    forumTopic.NumPosts = 1;
                    forumTopic.LastPostId = forumPost.Id;
                    forumTopic.LastPostCustomerId = forumPost.CustomerId;
                    forumTopic.LastPostTime = forumPost.CreatedOnUtc;
                    forumTopic.UpdatedOnUtc = nowUtc;
                    _forumService.UpdateTopic(forumTopic);

                    //subscription                
                    if (_forumService.IsCustomerAllowedToSubscribe(_workContext.CurrentCustomer))
                    {
                        if (model.Subscribed)
                        {
                            var forumSubscription = new ForumSubscription
                            {
                                SubscriptionGuid = Guid.NewGuid(),
                                CustomerId = _workContext.CurrentCustomer.Id,
                                TopicId = forumTopic.Id,
                                CreatedOnUtc = nowUtc
                            };

                            _forumService.InsertSubscription(forumSubscription);
                        }
                    }

                    return RedirectToRoute("TopicSlug", new {id = forumTopic.Id, slug = forumTopic.GetSeName()});
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            // redisplay form
            model.TopicPriorities = _boardsWebService.ForumTopicTypesList();
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

        public virtual IActionResult TopicEdit(string id)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }

            var forumTopic = _forumService.GetTopicById(id);

            if (forumTopic == null)
            {
                return RedirectToRoute("Boards");
            }

            if (!_forumService.IsCustomerAllowedToEditTopic(_workContext.CurrentCustomer, forumTopic))
            {
                return new ChallengeResult();
            }

            var forum = _forumService.GetForumById(forumTopic.ForumId);
            if (forum == null)
            {
                return RedirectToRoute("Boards");
            }

            var model = _boardsWebService.PrepareEditForumTopic(forum);
            var firstPost = forumTopic.GetFirstPost(_forumService);
            model.Text = firstPost.Text;
            model.Subject = forumTopic.Subject;
            if (model.IsCustomerAllowedToSubscribe)
            {
                var forumSubscription = _forumService.GetAllSubscriptions(_workContext.CurrentCustomer.Id,
                    "", forumTopic.Id, 0, 1).FirstOrDefault();
                model.Subscribed = forumSubscription != null;
            }
            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        public virtual IActionResult TopicEdit(EditForumTopicModel model)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }

            var forumTopic = _forumService.GetTopicById(model.Id);

            if (forumTopic == null)
            {
                return RedirectToRoute("Boards");
            }
            var forum = _forumService.GetForumById(forumTopic.ForumId);
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
                        topicType = (ForumTopicType) Enum.ToObject(typeof (ForumTopicType), model.TopicTypeId);
                    }

                    //forum topic
                    forumTopic.TopicTypeId = (int) topicType;
                    forumTopic.Subject = subject;
                    forumTopic.UpdatedOnUtc = nowUtc;
                    _forumService.UpdateTopic(forumTopic);

                    //forum post                
                    var firstPost = forumTopic.GetFirstPost(_forumService);
                    if (firstPost != null)
                    {
                        firstPost.Text = text;
                        firstPost.UpdatedOnUtc = nowUtc;
                        _forumService.UpdatePost(firstPost);
                    }
                    else
                    {
                        //error (not possible)
                        firstPost = new ForumPost
                        {
                            TopicId = forumTopic.Id,
                            ForumId = forum.Id,
                            ForumGroupId = forum.ForumGroupId,
                            CustomerId = forumTopic.CustomerId,
                            Text = text,
                            IPAddress = ipAddress,
                            UpdatedOnUtc = nowUtc
                        };

                        _forumService.InsertPost(firstPost, false);
                    }

                    //subscription
                    if (_forumService.IsCustomerAllowedToSubscribe(_workContext.CurrentCustomer))
                    {
                        var forumSubscription = _forumService.GetAllSubscriptions(_workContext.CurrentCustomer.Id,
                            "", forumTopic.Id, 0, 1).FirstOrDefault();
                        if (model.Subscribed)
                        {
                            if (forumSubscription == null)
                            {
                                forumSubscription = new ForumSubscription
                                {
                                    SubscriptionGuid = Guid.NewGuid(),
                                    CustomerId = _workContext.CurrentCustomer.Id,
                                    TopicId = forumTopic.Id,
                                    CreatedOnUtc = nowUtc
                                };

                                _forumService.InsertSubscription(forumSubscription);
                            }
                        }
                        else
                        {
                            if (forumSubscription != null)
                            {
                                _forumService.DeleteSubscription(forumSubscription);
                            }
                        }
                    }

                    // redirect to the topic page with the topic slug
                    return RedirectToRoute("TopicSlug", new {id = forumTopic.Id, slug = forumTopic.GetSeName()});
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            // redisplay form
            model.TopicPriorities = _boardsWebService.ForumTopicTypesList();
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
        [PublicAntiForgery]
        public virtual IActionResult PostDelete(string id)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return Json(new
                {
                    redirect = Url.RouteUrl("HomePage"),
                });
            }

            var forumPost = _forumService.GetPostById(id);

            if (forumPost != null)
            {
                if (!_forumService.IsCustomerAllowedToDeletePost(_workContext.CurrentCustomer, forumPost))
                {
                    return new ChallengeResult();
                }

                var forumTopic = _forumService.GetTopicById(forumPost.TopicId);
                var forumId =  forumTopic.ForumId;
                var forumSlug = _forumService.GetForumById(forumTopic.ForumId).GetSeName();

                _forumService.DeletePost(forumPost);

                //get topic one more time because it can be deleted (first or only post deleted)
                forumTopic = _forumService.GetTopicById(forumPost.TopicId);
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

        public virtual IActionResult PostCreate(string id, string quote)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }

            var forumTopic = _forumService.GetTopicById(id);

            if (forumTopic == null)
            {
                return RedirectToRoute("Boards");
            }

            if (!_forumService.IsCustomerAllowedToCreatePost(_workContext.CurrentCustomer, forumTopic))
            {
                return new ChallengeResult();
            }

            var forum = _forumService.GetForumById(forumTopic.ForumId);
            if (forum == null)
            {
                return RedirectToRoute("Boards");
            }
            var model = _boardsWebService.PrepareEditForumPost(forum, forumTopic, quote);
            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        public virtual IActionResult PostCreate(EditForumPostModel model)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }

            var forumTopic = _forumService.GetTopicById(model.ForumTopicId);
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

                    var forumPost = new ForumPost
                    {
                        TopicId = forumTopic.Id,
                        ForumId = forumTopic.ForumId,
                        ForumGroupId = forumTopic.ForumGroupId,
                        CustomerId = _workContext.CurrentCustomer.Id,
                        Text = text,
                        IPAddress = ipAddress,
                        CreatedOnUtc = nowUtc,
                        UpdatedOnUtc = nowUtc
                    };
                    _forumService.InsertPost(forumPost, true);
                    if (!_workContext.CurrentCustomer.IsHasForumPost)
                    {
                        _workContext.CurrentCustomer.IsHasForumPost = true;
                        EngineContext.Current.Resolve<ICustomerService>().UpdateHasForumPost(_workContext.CurrentCustomer.Id);
                    }

                    //subscription
                    if (_forumService.IsCustomerAllowedToSubscribe(_workContext.CurrentCustomer))
                    {
                        var forumSubscription = _forumService.GetAllSubscriptions(_workContext.CurrentCustomer.Id,
                            "", forumPost.TopicId, 0, 1).FirstOrDefault();
                        if (model.Subscribed)
                        {
                            if (forumSubscription == null)
                            {
                                forumSubscription = new ForumSubscription
                                {
                                    SubscriptionGuid = Guid.NewGuid(),
                                    CustomerId = _workContext.CurrentCustomer.Id,
                                    TopicId = forumPost.TopicId,
                                    CreatedOnUtc = nowUtc
                                };

                                _forumService.InsertSubscription(forumSubscription);
                            }
                        }
                        else
                        {
                            if (forumSubscription != null)
                            {
                                _forumService.DeleteSubscription(forumSubscription);
                            }
                        }
                    }

                    int pageSize =_forumSettings.PostsPageSize > 0 ? _forumSettings.PostsPageSize : 10;

                    int pageIndex = (_forumService.CalculateTopicPageIndex(forumPost.TopicId, pageSize, forumPost.Id) + 1);
                    var url = string.Empty;
                    var _forumTopic = _forumService.GetTopicById(forumPost.TopicId);
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
            var forum = _forumService.GetForumById(forumTopic.ForumId);
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

        public virtual IActionResult PostEdit(string id)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }

            var forumPost = _forumService.GetPostById(id);

            if (forumPost == null)
            {
                return RedirectToRoute("Boards");
            }
            if (!_forumService.IsCustomerAllowedToEditPost(_workContext.CurrentCustomer, forumPost))
            {
                return new ChallengeResult();
            }
            var forumTopic = _forumService.GetTopicById(forumPost.TopicId);
            if (forumTopic == null)
            {
                return RedirectToRoute("Boards");
            }
            var forum = _forumService.GetForumById(forumTopic.ForumId);
            if (forum == null)
            {
                return RedirectToRoute("Boards");
            }

            var model = new EditForumPostModel
            {
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
                var forumSubscription = _forumService.GetAllSubscriptions(_workContext.CurrentCustomer.Id,
                    "", forumTopic.Id, 0, 1).FirstOrDefault();
                model.Subscribed = forumSubscription != null;
            }

            return View(model);
        }

        [HttpPost]
        [PublicAntiForgery]
        public virtual IActionResult PostEdit(EditForumPostModel model)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }

            var forumPost = _forumService.GetPostById(model.Id);
            if (forumPost == null)
            {
                return RedirectToRoute("Boards");
            }

            if (!_forumService.IsCustomerAllowedToEditPost(_workContext.CurrentCustomer, forumPost))
            {
                return new ChallengeResult();
            }

            var forumTopic = _forumService.GetTopicById(forumPost.TopicId);
            if (forumTopic == null)
            {
                return RedirectToRoute("Boards");
            }

            var forum = _forumService.GetForumById(forumTopic.ForumId);
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
                    _forumService.UpdatePost(forumPost);

                    //subscription
                    if (_forumService.IsCustomerAllowedToSubscribe(_workContext.CurrentCustomer))
                    {
                        var forumSubscription = _forumService.GetAllSubscriptions(_workContext.CurrentCustomer.Id,
                            "", forumPost.TopicId, 0, 1).FirstOrDefault();
                        if (model.Subscribed)
                        {
                            if (forumSubscription == null)
                            {
                                forumSubscription = new ForumSubscription
                                {
                                    SubscriptionGuid = Guid.NewGuid(),
                                    CustomerId = _workContext.CurrentCustomer.Id,
                                    TopicId = forumPost.TopicId,
                                    CreatedOnUtc = nowUtc
                                };
                                _forumService.InsertSubscription(forumSubscription);
                            }
                        }
                        else
                        {
                            if (forumSubscription != null)
                            {
                                _forumService.DeleteSubscription(forumSubscription);
                            }
                        }
                    }

                    int pageSize = _forumSettings.PostsPageSize > 0 ? _forumSettings.PostsPageSize : 10;
                    int pageIndex = (_forumService.CalculateTopicPageIndex(forumPost.TopicId, pageSize, forumPost.Id) + 1);
                    var url = string.Empty;
                    var forumtopic = _forumService.GetTopicById(forumPost.TopicId);
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

        public virtual IActionResult Search(string searchterms, bool? adv, string forumId,
            string within, string limitDays, int pageNumber = 1)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }
            var model = _boardsWebService.PrepareSearch(searchterms, adv, forumId, within, limitDays, pageNumber);
            return View(model);
        }


        public virtual IActionResult CustomerForumSubscriptions(int? pageNumber)
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
            var model = _boardsWebService.PrepareCustomerForumSubscriptions(pageIndex);
            return View(model);
        }
        [HttpPost, ActionName("CustomerForumSubscriptions")]
        public virtual IActionResult CustomerForumSubscriptionsPOST(IFormCollection formCollection)
        {
            foreach (var key in formCollection.Keys)
            {
                var value = formCollection[key];

                if (value.Equals("on") && key.StartsWith("fs", StringComparison.OrdinalIgnoreCase))
                {
                    var id = key.Replace("fs", "").Trim();
                    var forumSubscription = _forumService.GetSubscriptionById(id);
                    if (forumSubscription != null && forumSubscription.CustomerId == _workContext.CurrentCustomer.Id)
                    {
                        _forumService.DeleteSubscription(forumSubscription);
                    }
                }
            }

            return RedirectToRoute("CustomerForumSubscriptions");
        }

        [HttpPost]
        public virtual IActionResult PostVote(string postId, bool isUp)
        {
            if (!_forumSettings.AllowPostVoting)
                return new NullJsonResult();

            var forumPost = _forumService.GetPostById(postId);
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

            var forumPostVote = _forumService.GetPostVote(postId, _workContext.CurrentCustomer.Id);
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
                    _forumService.DeletePostVote(forumPostVote);
                    forumPost = _forumService.GetPostById(postId);
                    return Json(new { VoteCount = forumPost.VoteCount });
                }
            }

            if (_forumService.GetNumberOfPostVotes(_workContext.CurrentCustomer.Id, DateTime.UtcNow.AddDays(-1)) >= _forumSettings.MaxVotesPerDay)
                return Json(new
                {
                    Error = string.Format(_localizationService.GetResource("Forum.Votes.MaxVotesReached"), _forumSettings.MaxVotesPerDay),
                    VoteCount = forumPost.VoteCount
                });


            _forumService.InsertPostVote(new ForumPostVote
            {
                CustomerId = _workContext.CurrentCustomer.Id,
                ForumPostId = postId,
                IsUp = isUp,
                CreatedOnUtc = DateTime.UtcNow
            });
            forumPost = _forumService.GetPostById(postId);
            return Json(new { VoteCount = forumPost.VoteCount, IsUp = isUp });
        }

        #endregion
    }
}
