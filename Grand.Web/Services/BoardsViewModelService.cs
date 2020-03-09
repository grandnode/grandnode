﻿using Grand.Core;
using Grand.Core.Domain.Customers;
using Grand.Core.Domain.Forums;
using Grand.Core.Domain.Media;
using Grand.Core.Html;
using Grand.Services.Common;
using Grand.Services.Customers;
using Grand.Services.Directory;
using Grand.Services.Forums;
using Grand.Services.Helpers;
using Grand.Services.Localization;
using Grand.Services.Media;
using Grand.Services.Seo;
using Grand.Web.Interfaces;
using Grand.Web.Models.Boards;
using Grand.Web.Models.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Services
{
    public partial class BoardsViewModelService : IBoardsViewModelService
    {
        private readonly IForumService _forumService;
        private readonly ILocalizationService _localizationService;
        private readonly IPictureService _pictureService;
        private readonly ICountryService _countryService;
        private readonly IWorkContext _workContext;
        private readonly ForumSettings _forumSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly MediaSettings _mediaSettings;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IServiceProvider _serviceProvider;

        public BoardsViewModelService(IForumService forumService,
            ILocalizationService localizationService,
            IPictureService pictureService,
            ICountryService countryService,
            IWorkContext workContext,
            ForumSettings forumSettings,
            CustomerSettings customerSettings,
            MediaSettings mediaSettings,
            IDateTimeHelper dateTimeHelper,
            IServiceProvider serviceProvider)
        {
            _forumService = forumService;
            _localizationService = localizationService;
            _pictureService = pictureService;
            _countryService = countryService;
            _workContext = workContext;
            _forumSettings = forumSettings;
            _customerSettings = customerSettings;
            _mediaSettings = mediaSettings;
            _dateTimeHelper = dateTimeHelper;
            _serviceProvider = serviceProvider;
        }

        public virtual async Task<BoardsIndexModel> PrepareBoardsIndex()
        {
            var forumGroups = await _forumService.GetAllForumGroups();

            var model = new BoardsIndexModel();
            foreach (var forumGroup in forumGroups)
            {

                var forumGroupModel = await PrepareForumGroup(forumGroup);
                model.ForumGroups.Add(forumGroupModel);
            }
            return model;
        }

        public virtual async Task<ActiveDiscussionsModel> PrepareActiveDiscussions()
        {
            var topics = await _forumService.GetActiveTopics("", 0, _forumSettings.HomePageActiveDiscussionsTopicCount);
            if (!topics.Any())
                return null;
            var model = new ActiveDiscussionsModel();
            foreach (var topic in topics)
            {
                var topicModel = await PrepareForumTopicRow(topic);
                model.ForumTopics.Add(topicModel);
            }
            model.ViewAllLinkEnabled = true;
            model.ActiveDiscussionsFeedEnabled = _forumSettings.ActiveDiscussionsFeedEnabled;
            model.PostsPageSize = _forumSettings.PostsPageSize;
            model.AllowPostVoting = _forumSettings.AllowPostVoting;

            return model;
        }
        public virtual async Task<ActiveDiscussionsModel> PrepareActiveDiscussions(string forumId = "", int pageNumber = 1)
        {
            var model = new ActiveDiscussionsModel();

            int pageSize = _forumSettings.ActiveDiscussionsPageSize > 0 ? _forumSettings.ActiveDiscussionsPageSize : 50;

            var topics = await _forumService.GetActiveTopics(forumId, (pageNumber - 1), pageSize);
            model.TopicPageSize = topics.PageSize;
            model.TopicTotalRecords = topics.TotalCount;
            model.TopicPageIndex = topics.PageIndex;
            foreach (var topic in topics)
            {
                var topicModel = await PrepareForumTopicRow(topic);
                model.ForumTopics.Add(topicModel);
            }
            model.ViewAllLinkEnabled = false;
            model.ActiveDiscussionsFeedEnabled = _forumSettings.ActiveDiscussionsFeedEnabled;
            model.PostsPageSize = _forumSettings.PostsPageSize;
            model.AllowPostVoting = _forumSettings.AllowPostVoting;
            return model;
        }

        public virtual async Task<ForumPageModel> PrepareForumPage(Forum forum, int pageNumber)
        {
            var model = new ForumPageModel();
            model.Id = forum.Id;
            model.Name = forum.Name;
            model.SeName = forum.GetSeName();
            model.Description = forum.Description;

            int pageSize = _forumSettings.TopicsPageSize > 0 ? _forumSettings.TopicsPageSize : 10;

            //subscription                
            if (_forumService.IsCustomerAllowedToSubscribe(_workContext.CurrentCustomer))
            {
                model.WatchForumText = _localizationService.GetResource("Forum.WatchForum");

                var forumSubscription = (await _forumService.GetAllSubscriptions(_workContext.CurrentCustomer.Id, forum.Id, "", 0, 1)).FirstOrDefault();
                if (forumSubscription != null)
                {
                    model.WatchForumText = _localizationService.GetResource("Forum.UnwatchForum");
                }
            }

            var topics = await _forumService.GetAllTopics(forum.Id, "", string.Empty,
                ForumSearchType.All, 0, (pageNumber - 1), pageSize);
            model.TopicPageSize = topics.PageSize;
            model.TopicTotalRecords = topics.TotalCount;
            model.TopicPageIndex = topics.PageIndex;
            foreach (var topic in topics)
            {
                var topicModel = await PrepareForumTopicRow(topic);
                model.ForumTopics.Add(topicModel);
            }
            model.IsCustomerAllowedToSubscribe = _forumService.IsCustomerAllowedToSubscribe(_workContext.CurrentCustomer);
            model.ForumFeedsEnabled = _forumSettings.ForumFeedsEnabled;
            model.PostsPageSize = _forumSettings.PostsPageSize;
            model.AllowPostVoting = _forumSettings.AllowPostVoting;
            return model;
        }

        public virtual async Task<ForumTopicRowModel> PrepareForumTopicRow(ForumTopic topic)
        {
            var customer = await _serviceProvider.GetRequiredService<ICustomerService>().GetCustomerById(topic.CustomerId);
            var topicModel = new ForumTopicRowModel
            {
                Id = topic.Id,
                Subject = topic.Subject,
                SeName = topic.GetSeName(),
                LastPostId = topic.LastPostId,
                NumPosts = topic.NumPosts,
                Views = topic.Views,
                NumReplies = topic.NumReplies,
                ForumTopicType = topic.ForumTopicType,
                CustomerId = topic.CustomerId,
                AllowViewingProfiles = _customerSettings.AllowViewingProfiles,
                CustomerName = customer.FormatUserName(_customerSettings.CustomerNameFormat),
                IsCustomerGuest = customer.IsGuest()
            };

            var forumPosts = await _forumService.GetAllPosts(topic.Id, "", string.Empty, 1, _forumSettings.PostsPageSize);
            topicModel.TotalPostPages = forumPosts.TotalPages;

            var firstPost = await topic.GetFirstPost(_forumService);
            topicModel.Votes = firstPost != null ? firstPost.VoteCount : 0;

            return topicModel;

        }
        public virtual ForumRowModel PrepareForumRow(Forum forum)
        {
            var forumModel = new ForumRowModel
            {
                Id = forum.Id,
                Name = forum.Name,
                SeName = forum.GetSeName(),
                Description = forum.Description,
                NumTopics = forum.NumTopics,
                NumPosts = forum.NumPosts,
                LastPostId = forum.LastPostId,
            };
            return forumModel;
        }
        public virtual async Task<ForumGroupModel> PrepareForumGroup(ForumGroup forumGroup)
        {
            var forumGroupModel = new ForumGroupModel
            {
                Id = forumGroup.Id,
                Name = forumGroup.Name,
                SeName = forumGroup.GetSeName(),
            };
            var forums = await _forumService.GetAllForumsByGroupId(forumGroup.Id);
            foreach (var forum in forums)
            {
                var forumModel = PrepareForumRow(forum);
                forumGroupModel.Forums.Add(forumModel);
            }
            return forumGroupModel;
        }
        public virtual IEnumerable<SelectListItem> ForumTopicTypesList()
        {
            var list = new List<SelectListItem>();

            list.Add(new SelectListItem
            {
                Text = _localizationService.GetResource("Forum.Normal"),
                Value = ((int)ForumTopicType.Normal).ToString()
            });

            list.Add(new SelectListItem
            {
                Text = _localizationService.GetResource("Forum.Sticky"),
                Value = ((int)ForumTopicType.Sticky).ToString()
            });

            list.Add(new SelectListItem
            {
                Text = _localizationService.GetResource("Forum.Announcement"),
                Value = ((int)ForumTopicType.Announcement).ToString()
            });

            return list;

        }
        public virtual async Task<IEnumerable<SelectListItem>> ForumGroupsForumsList()
        {
            var forumsList = new List<SelectListItem>();
            var separator = "--";
            var forumGroups = await _forumService.GetAllForumGroups();

            foreach (var fg in forumGroups)
            {
                // Add the forum group with Value of 0 so it won't be used as a target forum
                forumsList.Add(new SelectListItem { Text = fg.Name, Value = "" });

                var forums = await _forumService.GetAllForumsByGroupId(fg.Id);
                foreach (var f in forums)
                {
                    forumsList.Add(new SelectListItem { Text = string.Format("{0}{1}", separator, f.Name), Value = f.Id.ToString() });
                }
            }
            return forumsList;
        }
        public virtual async Task<ForumTopicPageModel> PrepareForumTopicPage(ForumTopic forumTopic, int pageNumber)
        {
            var posts = await _forumService.GetAllPosts(forumTopic.Id, "", string.Empty,
                pageNumber - 1, _forumSettings.PostsPageSize);

            //prepare model
            var model = new ForumTopicPageModel();
            model.Id = forumTopic.Id;
            model.Subject = forumTopic.Subject;
            model.SeName = forumTopic.GetSeName();
            var currentcustomer = _workContext.CurrentCustomer;
            model.IsCustomerAllowedToEditTopic = _forumService.IsCustomerAllowedToEditTopic(currentcustomer, forumTopic);
            model.IsCustomerAllowedToDeleteTopic = _forumService.IsCustomerAllowedToDeleteTopic(currentcustomer, forumTopic);
            model.IsCustomerAllowedToMoveTopic = _forumService.IsCustomerAllowedToMoveTopic(currentcustomer, forumTopic);
            model.IsCustomerAllowedToSubscribe = _forumService.IsCustomerAllowedToSubscribe(currentcustomer);

            if (model.IsCustomerAllowedToSubscribe)
            {
                model.WatchTopicText = _localizationService.GetResource("Forum.WatchTopic");

                var forumTopicSubscription = (await _forumService.GetAllSubscriptions(currentcustomer.Id,
                    "", forumTopic.Id, 0, 1)).FirstOrDefault();
                if (forumTopicSubscription != null)
                {
                    model.WatchTopicText = _localizationService.GetResource("Forum.UnwatchTopic");
                }
            }
            model.PostsPageIndex = posts.PageIndex;
            model.PostsPageSize = posts.PageSize;
            model.PostsTotalRecords = posts.TotalCount;
            foreach (var post in posts)
            {
                var customer = await _serviceProvider.GetRequiredService<ICustomerService>().GetCustomerById(post.CustomerId);
                var forumPostModel = new ForumPostModel
                {
                    Id = post.Id,
                    ForumTopicId = post.TopicId,
                    ForumTopicSeName = forumTopic.GetSeName(),
                    FormattedText = post.FormatPostText(),
                    IsCurrentCustomerAllowedToEditPost = _forumService.IsCustomerAllowedToEditPost(currentcustomer, post),
                    IsCurrentCustomerAllowedToDeletePost = _forumService.IsCustomerAllowedToDeletePost(currentcustomer, post),
                    CustomerId = post.CustomerId,
                    AllowViewingProfiles = _customerSettings.AllowViewingProfiles,
                    CustomerName = customer.FormatUserName(_customerSettings.CustomerNameFormat),
                    IsCustomerForumModerator = customer.IsForumModerator(),
                    IsCustomerGuest = customer.IsGuest(),
                    ShowCustomersPostCount = _forumSettings.ShowCustomersPostCount,
                    ForumPostCount = customer.GetAttributeFromEntity<int>(SystemCustomerAttributeNames.ForumPostCount),
                    ShowCustomersJoinDate = _customerSettings.ShowCustomersJoinDate,
                    CustomerJoinDate = customer.CreatedOnUtc,
                    AllowPrivateMessages = _forumSettings.AllowPrivateMessages,
                    SignaturesEnabled = _forumSettings.SignaturesEnabled,
                    FormattedSignature = (customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.Signature)).FormatForumSignatureText(),
                };
                //created on string
                if (_forumSettings.RelativeDateTimeFormattingEnabled)
                    forumPostModel.PostCreatedOnStr = post.CreatedOnUtc.ToString("f");
                else
                    forumPostModel.PostCreatedOnStr = _dateTimeHelper.ConvertToUserTime(post.CreatedOnUtc, DateTimeKind.Utc).ToString("f");
                //avatar
                if (_customerSettings.AllowCustomersToUploadAvatars)
                {
                    forumPostModel.CustomerAvatarUrl = await _pictureService.GetPictureUrl(
                        customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.AvatarPictureId),
                        _mediaSettings.AvatarPictureSize,
                        _customerSettings.DefaultAvatarEnabled,
                        defaultPictureType: PictureType.Avatar);
                }
                //location
                forumPostModel.ShowCustomersLocation = _customerSettings.ShowCustomersLocation;
                if (_customerSettings.ShowCustomersLocation)
                {
                    var countryId = customer.GetAttributeFromEntity<string>(SystemCustomerAttributeNames.CountryId);
                    var country = await _countryService.GetCountryById(countryId);
                    forumPostModel.CustomerLocation = country != null ? country.GetLocalized(x => x.Name, _workContext.WorkingLanguage.Id) : string.Empty;
                }

                if (_forumSettings.AllowPostVoting)
                {
                    forumPostModel.AllowPostVoting = true;
                    forumPostModel.VoteCount = post.VoteCount;
                    var postVote = await _forumService.GetPostVote(post.Id, _workContext.CurrentCustomer.Id);
                    if (postVote != null)
                        forumPostModel.VoteIsUp = postVote.IsUp;
                }
                // page number is needed for creating post link in _ForumPost partial view
                forumPostModel.CurrentTopicPage = pageNumber;
                model.ForumPostModels.Add(forumPostModel);
            }

            return model;
        }
        public virtual async Task<TopicMoveModel> PrepareTopicMove(ForumTopic forumTopic)
        {
            var model = new TopicMoveModel();
            model.ForumList = await ForumGroupsForumsList();
            model.Id = forumTopic.Id;
            model.TopicSeName = forumTopic.GetSeName();
            model.ForumSelected = forumTopic.ForumId;
            return model;
        }

        public virtual EditForumTopicModel PrepareEditForumTopic(Forum forum)
        {
            var model = new EditForumTopicModel();
            model.Id = "";
            model.IsEdit = false;
            model.ForumId = forum.Id;
            model.ForumName = forum.Name;
            model.ForumSeName = forum.GetSeName();
            model.ForumEditor = _forumSettings.ForumEditor;
            model.IsCustomerAllowedToSetTopicPriority = _forumService.IsCustomerAllowedToSetTopicPriority(_workContext.CurrentCustomer);
            model.TopicPriorities = ForumTopicTypesList();
            model.IsCustomerAllowedToSubscribe = _forumService.IsCustomerAllowedToSubscribe(_workContext.CurrentCustomer);
            model.Subscribed = false;
            return model;
        }
        public virtual async Task<EditForumPostModel> PrepareEditForumPost(Forum forum, ForumTopic forumTopic, string quote)
        {
            var model = new EditForumPostModel
            {
                Id = "",
                ForumTopicId = forumTopic.Id,
                IsEdit = false,
                ForumEditor = _forumSettings.ForumEditor,
                ForumName = forum.Name,
                ForumTopicSubject = forumTopic.Subject,
                ForumTopicSeName = forumTopic.GetSeName(),
                IsCustomerAllowedToSubscribe = _forumService.IsCustomerAllowedToSubscribe(_workContext.CurrentCustomer),
                Subscribed = false,
            };

            //subscription            
            if (model.IsCustomerAllowedToSubscribe)
            {
                var forumSubscription = (await _forumService.GetAllSubscriptions(_workContext.CurrentCustomer.Id,
                    "", forumTopic.Id, 0, 1)).FirstOrDefault();
                model.Subscribed = forumSubscription != null;
            }

            // Insert the quoted text
            string text = string.Empty;
            if (!String.IsNullOrEmpty(quote))
            {
                var quotePost = await _forumService.GetPostById(quote);
                if (quotePost != null && quotePost.TopicId == forumTopic.Id)
                {
                    var quotePostText = quotePost.Text;
                    var customer = await _serviceProvider.GetRequiredService<ICustomerService>().GetCustomerById(quotePost.CustomerId);
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

        public virtual async Task<LastPostModel> PrepareLastPost(string forumPostId, bool showTopic)
        {
            var post = await _forumService.GetPostById(forumPostId);
            var model = new LastPostModel();
            if (post != null)
            {
                var forumTopic = await _forumService.GetTopicById(post.TopicId);
                if (forumTopic != null)
                {
                    var customer = await _serviceProvider.GetRequiredService<ICustomerService>().GetCustomerById(post.CustomerId);
                    model.Id = post.Id;
                    model.ForumTopicId = post.TopicId;
                    model.ForumTopicSeName = forumTopic.GetSeName();
                    model.ForumTopicSubject = forumTopic.StripTopicSubject(_forumSettings);
                    model.CustomerId = post.CustomerId;
                    model.AllowViewingProfiles = _customerSettings.AllowViewingProfiles;
                    model.CustomerName = customer.FormatUserName(_customerSettings.CustomerNameFormat);
                    model.IsCustomerGuest = customer.IsGuest();
                    //created on string
                    if (_forumSettings.RelativeDateTimeFormattingEnabled)
                        model.PostCreatedOnStr = post.CreatedOnUtc.ToString("f");
                    else
                        model.PostCreatedOnStr = _dateTimeHelper.ConvertToUserTime(post.CreatedOnUtc, DateTimeKind.Utc).ToString("f");
                }
            }
            model.ShowTopic = showTopic;
            return model;
        }

        public virtual async Task<ForumBreadcrumbModel> PrepareForumBreadcrumb(string forumGroupId, string forumId, string forumTopicId)
        {
            var model = new ForumBreadcrumbModel();

            ForumTopic forumTopic = null;
            if (!String.IsNullOrEmpty(forumTopicId))
            {
                forumTopic = await _forumService.GetTopicById(forumTopicId);
                if (forumTopic != null)
                {
                    model.ForumTopicId = forumTopic.Id;
                    model.ForumTopicSubject = forumTopic.Subject;
                    model.ForumTopicSeName = forumTopic.GetSeName();
                }
            }

            Forum forum = await _forumService.GetForumById(forumTopic != null ? forumTopic.ForumId : (!String.IsNullOrEmpty(forumId) ? forumId : ""));
            if (forum != null)
            {
                model.ForumId = forum.Id;
                model.ForumName = forum.Name;
                model.ForumSeName = forum.GetSeName();
            }

            var forumGroup = await _forumService.GetForumGroupById(forum != null ? forum.ForumGroupId : (!String.IsNullOrEmpty(forumGroupId) ? forumGroupId : ""));
            if (forumGroup != null)
            {
                model.ForumGroupId = forumGroup.Id;
                model.ForumGroupName = forumGroup.Name;
                model.ForumGroupSeName = forumGroup.GetSeName();
            }

            return model;
        }

        public virtual async Task<CustomerForumSubscriptionsModel> PrepareCustomerForumSubscriptions(int pageIndex)
        {
            var pageSize = _forumSettings.ForumSubscriptionsPageSize;
            var list = await _forumService.GetAllSubscriptions(_workContext.CurrentCustomer.Id, "", "", pageIndex, pageSize);
            var model = new CustomerForumSubscriptionsModel();
            foreach (var forumSubscription in list)
            {
                var forumTopicId = forumSubscription.TopicId;
                var forumId = forumSubscription.ForumId;
                bool topicSubscription = false;
                var title = string.Empty;
                var slug = string.Empty;

                if (!String.IsNullOrEmpty(forumTopicId))
                {
                    topicSubscription = true;
                    var forumTopic = await _forumService.GetTopicById(forumTopicId);
                    if (forumTopic != null)
                    {
                        title = forumTopic.Subject;
                        slug = forumTopic.GetSeName();
                    }
                }
                else
                {
                    var forum = await _forumService.GetForumById(forumId);
                    if (forum != null)
                    {
                        title = forum.Name;
                        slug = forum.GetSeName();
                    }
                }

                model.ForumSubscriptions.Add(new CustomerForumSubscriptionsModel.ForumSubscriptionModel
                {
                    Id = forumSubscription.Id,
                    ForumTopicId = forumTopicId,
                    ForumId = forumSubscription.ForumId,
                    TopicSubscription = topicSubscription,
                    Title = title,
                    Slug = slug,
                });
            }

            model.PagerModel = new PagerModel(_localizationService)
            {
                PageSize = list.PageSize,
                TotalRecords = list.TotalCount,
                PageIndex = list.PageIndex,
                ShowTotalSummary = false,
                RouteActionName = "CustomerForumSubscriptionsPaged",
                UseRouteLinks = true,
                RouteValues = new ForumSubscriptionsRouteValues { pageNumber = pageIndex }
            };

            return model;
        }

        public virtual async Task<SearchModel> PrepareSearch(string searchterms, bool? adv, string forumId,
            string within, string limitDays, int pageNumber = 1)
        {
            int pageSize = 10;

            var model = new SearchModel();

            // Create the values for the "Limit results to previous" select list
            var limitList = new List<SelectListItem>
                            {
                                new SelectListItem
                                {
                                    Text = _localizationService.GetResource("Forum.Search.LimitResultsToPrevious.AllResults"),
                                    Value = "0"
                                },
                                new SelectListItem
                                {
                                    Text = _localizationService.GetResource("Forum.Search.LimitResultsToPrevious.1day"),
                                    Value = "1"
                                },
                                new SelectListItem
                                {
                                    Text = _localizationService.GetResource("Forum.Search.LimitResultsToPrevious.7days"),
                                    Value = "7"
                                },
                                new SelectListItem
                                {
                                    Text = _localizationService.GetResource("Forum.Search.LimitResultsToPrevious.2weeks"),
                                    Value = "14"
                                },
                                new SelectListItem
                                {
                                    Text = _localizationService.GetResource("Forum.Search.LimitResultsToPrevious.1month"),
                                    Value = "30"
                                },
                                new SelectListItem
                                {
                                    Text = _localizationService.GetResource("Forum.Search.LimitResultsToPrevious.3months"),
                                    Value = "92"
                                },
                                new SelectListItem
                                {
                                    Text= _localizationService.GetResource("Forum.Search.LimitResultsToPrevious.6months"),
                                    Value = "183"
                                },
                                new SelectListItem
                                {
                                    Text = _localizationService.GetResource("Forum.Search.LimitResultsToPrevious.1year"),
                                    Value = "365"
                                }
                            };
            model.LimitList = limitList;

            // Create the values for the "Search in forum" select list
            var forumsSelectList = new List<SelectListItem>();
            forumsSelectList.Add(
                new SelectListItem
                {
                    Text = _localizationService.GetResource("Forum.Search.SearchInForum.All"),
                    Value = "",
                    Selected = true,
                });
            var separator = "--";
            var forumGroups = await _forumService.GetAllForumGroups();
            foreach (var fg in forumGroups)
            {
                // Add the forum group with value as '-' so it can't be used as a target forum id
                forumsSelectList.Add(new SelectListItem { Text = fg.Name, Value = "-" });

                var forums = await _forumService.GetAllForumsByGroupId(fg.Id);
                foreach (var f in forums)
                {
                    forumsSelectList.Add(
                        new SelectListItem
                        {
                            Text = string.Format("{0}{1}", separator, f.Name),
                            Value = f.Id.ToString()
                        });
                }
            }
            model.ForumList = forumsSelectList;

            // Create the values for "Search within" select list            
            var withinList = new List<SelectListItem>
                                {
                                    new SelectListItem
                                    {
                                        Value = ((int)ForumSearchType.All).ToString(),
                                        Text = _localizationService.GetResource("Forum.Search.SearchWithin.All")
                                    },
                                    new SelectListItem
                                    {
                                        Value = ((int)ForumSearchType.TopicTitlesOnly).ToString(),
                                        Text = _localizationService.GetResource("Forum.Search.SearchWithin.TopicTitlesOnly")
                                    },
                                    new SelectListItem
                                    {
                                        Value = ((int)ForumSearchType.PostTextOnly).ToString(),
                                        Text = _localizationService.GetResource("Forum.Search.SearchWithin.PostTextOnly")
                                    }
                                };
            model.WithinList = withinList;

            string forumIdSelected = forumId;
            model.ForumIdSelected = forumIdSelected;

            int withinSelected;
            int.TryParse(within, out withinSelected);
            model.WithinSelected = withinSelected;

            int limitDaysSelected;
            int.TryParse(limitDays, out limitDaysSelected);
            model.LimitDaysSelected = limitDaysSelected;

            int searchTermMinimumLength = _forumSettings.ForumSearchTermMinimumLength;

            model.ShowAdvancedSearch = adv.GetValueOrDefault();
            model.SearchResultsVisible = false;
            model.NoResultsVisisble = false;
            model.PostsPageSize = _forumSettings.PostsPageSize;
            model.AllowPostVoting = _forumSettings.AllowPostVoting;

            try
            {
                if (!String.IsNullOrWhiteSpace(searchterms))
                {
                    searchterms = searchterms.Trim();
                    model.SearchTerms = searchterms;

                    if (searchterms.Length < searchTermMinimumLength)
                    {
                        throw new GrandException(string.Format(_localizationService.GetResource("Forum.SearchTermMinimumLengthIsNCharacters"),
                            searchTermMinimumLength));
                    }

                    ForumSearchType searchWithin = 0;
                    int limitResultsToPrevious = 0;
                    if (adv.GetValueOrDefault())
                    {
                        searchWithin = (ForumSearchType)withinSelected;
                        limitResultsToPrevious = limitDaysSelected;
                    }

                    if (_forumSettings.SearchResultsPageSize > 0)
                    {
                        pageSize = _forumSettings.SearchResultsPageSize;
                    }

                    var topics = await _forumService.GetAllTopics(forumIdSelected, "", searchterms, searchWithin,
                        limitResultsToPrevious, pageNumber - 1, pageSize);
                    model.TopicPageSize = topics.PageSize;
                    model.TopicTotalRecords = topics.TotalCount;
                    model.TopicPageIndex = topics.PageIndex;
                    foreach (var topic in topics)
                    {
                        var topicModel = await PrepareForumTopicRow(topic);
                        model.ForumTopics.Add(topicModel);
                    }

                    model.SearchResultsVisible = (topics.Any());
                    model.NoResultsVisisble = !(model.SearchResultsVisible);

                    return model;
                }
                model.SearchResultsVisible = false;
            }
            catch (Exception ex)
            {
                model.Error = ex.Message;
            }

            //some exception raised
            model.TopicPageSize = pageSize;
            model.TopicTotalRecords = 0;
            model.TopicPageIndex = pageNumber - 1;

            return model;
        }
    }
}