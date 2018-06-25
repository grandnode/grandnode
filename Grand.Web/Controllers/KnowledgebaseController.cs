using Grand.Core.Domain.Knowledgebase;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security;
using Grand.Services.Knowledgebase;
using Grand.Web.Models.Knowledgebase;
using Microsoft.AspNetCore.Mvc;
using Grand.Services.Localization;

namespace Grand.Web.Controllers
{
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

            var category = _knowledgebaseService.GetPublicKnowledgebaseCategory(categoryId);
            if (category == null)
                return RedirectToAction("List");

            var model = new KnowledgebaseHomePageModel();
            var articles = _knowledgebaseService.GetPublicKnowledgebaseArticlesByCategory(categoryId);
            articles.ForEach(x => model.Articles.Add(new KnowledgebaseArticleModel
            {
                Content = x.GetLocalized(y => y.Content),
                Name = x.GetLocalized(y => y.Name),
                Id = x.Id,
                ParentCategoryId = x.ParentCategoryId,
                SeName = x.GetLocalized(y => y.SeName)
            }));

            model.CurrentCategoryId = categoryId;

            model.CurrentCategoryDescription = category.GetLocalized(y => y.Description);
            model.CurrentCategoryMetaDescription = category.GetLocalized(y => y.MetaDescription);
            model.CurrentCategoryMetaKeywords = category.GetLocalized(y => y.MetaKeywords);
            model.CurrentCategoryMetaTitle = category.GetLocalized(y => y.MetaTitle);
            model.CurrentCategoryName = category.GetLocalized(y => y.Name);
            model.CurrentCategorySeName = category.GetLocalized(y => y.SeName);

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
                    ParentCategoryId = x.ParentCategoryId,
                    SeName = x.GetLocalized(y => y.SeName)
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
            var article = _knowledgebaseService.GetPublicKnowledgebaseArticle(articleId);
            if (article == null)
                return RedirectToAction("List");

            model.Content = article.GetLocalized(y => y.Content);
            model.Name = article.GetLocalized(y => y.Name);
            model.Id = article.Id;
            model.ParentCategoryId = article.ParentCategoryId;
            model.SeName = article.GetLocalized(y => y.SeName);

            foreach (var id in article.RelatedArticles)
            {
                var a = _knowledgebaseService.GetPublicKnowledgebaseArticle(id);
                if (a != null)
                    model.RelatedArticles.Add(new KnowledgebaseArticleModel
                    {
                        SeName = a.SeName,
                        Id = a.Id,
                        Name = a.Name
                    });
            }

            return View("Article", model);
        }
    }
}
