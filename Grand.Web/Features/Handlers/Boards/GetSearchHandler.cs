using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grand.Core;
using Grand.Domain.Forums;
using Grand.Services.Forums;
using Grand.Services.Localization;
using Grand.Web.Features.Models.Boards;
using Grand.Web.Models.Boards;
using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Grand.Web.Features.Handlers.Boards
{
    public class GetSearchHandler : IRequestHandler<GetSearch, SearchModel>
    {
        private readonly ILocalizationService _localizationService;
        private readonly IForumService _forumService;
        private readonly IMediator _mediator;
        private readonly ForumSettings _forumSettings;

        public GetSearchHandler(
            ILocalizationService localizationService, 
            IForumService forumService, 
            IMediator mediator, 
            ForumSettings forumSettings)
        {
            _localizationService = localizationService;
            _forumService = forumService;
            _mediator = mediator;
            _forumSettings = forumSettings;
        }

        public async Task<SearchModel> Handle(GetSearch request, CancellationToken cancellationToken)
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
                new SelectListItem {
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
                        new SelectListItem {
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

            string forumIdSelected = request.ForumId;
            model.ForumIdSelected = forumIdSelected;

            int withinSelected;
            int.TryParse(request.Within, out withinSelected);
            model.WithinSelected = withinSelected;

            int limitDaysSelected;
            int.TryParse(request.LimitDays, out limitDaysSelected);
            model.LimitDaysSelected = limitDaysSelected;

            int searchTermMinimumLength = _forumSettings.ForumSearchTermMinimumLength;

            model.ShowAdvancedSearch = request.Adv.GetValueOrDefault();
            model.SearchResultsVisible = false;
            model.NoResultsVisisble = false;
            model.PostsPageSize = _forumSettings.PostsPageSize;
            model.AllowPostVoting = _forumSettings.AllowPostVoting;

            try
            {
                if (!String.IsNullOrWhiteSpace(request.Searchterms))
                {
                    request.Searchterms = request.Searchterms.Trim();
                    model.SearchTerms = request.Searchterms;

                    if (request.Searchterms.Length < searchTermMinimumLength)
                    {
                        throw new GrandException(string.Format(_localizationService.GetResource("Forum.SearchTermMinimumLengthIsNCharacters"),
                            searchTermMinimumLength));
                    }

                    ForumSearchType searchWithin = 0;
                    int limitResultsToPrevious = 0;
                    if (request.Adv.GetValueOrDefault())
                    {
                        searchWithin = (ForumSearchType)withinSelected;
                        limitResultsToPrevious = limitDaysSelected;
                    }

                    if (_forumSettings.SearchResultsPageSize > 0)
                    {
                        pageSize = _forumSettings.SearchResultsPageSize;
                    }

                    var topics = await _forumService.GetAllTopics(forumIdSelected, "", request.Searchterms, searchWithin,
                        limitResultsToPrevious, request.PageNumber - 1, pageSize);
                    model.TopicPageSize = topics.PageSize;
                    model.TopicTotalRecords = topics.TotalCount;
                    model.TopicPageIndex = topics.PageIndex;
                    foreach (var topic in topics)
                    {
                        var topicModel = await _mediator.Send(new GetForumTopicRow() { Topic = topic });
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
            model.TopicPageIndex = request.PageNumber - 1;

            return model;
        }
    }
}
