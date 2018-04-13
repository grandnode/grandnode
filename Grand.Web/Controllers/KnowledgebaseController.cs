using Grand.Core.Domain.Knowledgebase;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security;
using Grand.Services.Knowledgebase;
using Grand.Web.Models.Knowledgebase;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grand.Services.Localization;

namespace Grand.Web.Controllers
{
    [HttpsRequirement(SslRequirement.No)]
    public class KnowledgebaseController : BasePublicController
    {
        private readonly KnowledgebaseSettings _knowledgebaseSettings;
        private readonly IKnowledgebaseService _knowledgebaseService;

        public KnowledgebaseController(KnowledgebaseSettings knowledgebaseSettings, IKnowledgebaseService knowledgebaseService)
        {
            this._knowledgebaseSettings = knowledgebaseSettings;
            this._knowledgebaseService = knowledgebaseService;
        }

        public IActionResult List()
        {
            if (!_knowledgebaseSettings.Enabled)
                return RedirectToRoute("HomePage");

            var model = new KnowledgebaseHomePageModel();

            return View("List", model);
        }

        public IActionResult ArticlesByCategory(string categoryId)
        {
            if (!_knowledgebaseSettings.Enabled)
                return RedirectToRoute("HomePage");

            var model = new KnowledgebaseHomePageModel();
            var articles = _knowledgebaseService.GetPublicKnowledgebaseArticlesByCategory(categoryId);
            articles.ForEach(x => model.Articles.Add(new KnowledgebaseArticleModel
            {
                Content = x.GetLocalized(y => y.Content),
                Name = x.GetLocalized(y => y.Name),
                Id = x.Id,
                ParentCategoryId = x.ParentCategoryId
            }));

            model.CurrentCategoryId = categoryId;
            model.CurrentCategoryDescription = _knowledgebaseService.GetKnowledgebaseCategory(categoryId).GetLocalized(y => y.Description);

            return View("List", model);
        }

        public IActionResult ArticlesByKeyword(string keyword)
        {
            if (!_knowledgebaseSettings.Enabled)
                return RedirectToRoute("HomePage");

            var model = new KnowledgebaseHomePageModel();

            if (!string.IsNullOrEmpty(keyword))
            {
                var articles = _knowledgebaseService.GetPublicKnowledgebaseArticlesByKeyword(keyword);
                articles.ForEach(x => model.Articles.Add(new KnowledgebaseArticleModel
                {
                    Content = x.GetLocalized(y => y.Content),
                    Name = x.GetLocalized(y => y.Name),
                    Id = x.Id,
                    ParentCategoryId = x.ParentCategoryId
                }));
            }

            model.CurrentCategoryId = "[NONE]";

            return View("List", model);
        }

        public IActionResult KnowledgebaseArticle(string articleId)
        {
            if (!_knowledgebaseSettings.Enabled)
                return RedirectToRoute("HomePage");

            var model = new KnowledgebaseArticleModel();
            var article = _knowledgebaseService.GetKnowledgebaseArticle(articleId);
            model.Content = article.GetLocalized(y => y.Content);
            model.Name = article.GetLocalized(y => y.Name);
            model.Id = article.Id;
            model.ParentCategoryId = article.ParentCategoryId;

            return View("Article", model);
        }
    }
}
