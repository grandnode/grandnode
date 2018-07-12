using Grand.Core.Domain.Knowledgebase;
using Grand.Services.Knowledgebase;
using Grand.Services.Localization;
using Grand.Web.Models.Knowledgebase;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Components
{
    public class KnowledgebaseHomepageArticles : ViewComponent
    {
        private readonly IKnowledgebaseService _knowledgebaseService;
        private readonly KnowledgebaseSettings _knowledgebaseSettings;

        public KnowledgebaseHomepageArticles(IKnowledgebaseService knowledgebaseService, KnowledgebaseSettings knowledgebaseSettings)
        {
            this._knowledgebaseService = knowledgebaseService;
            this._knowledgebaseSettings = knowledgebaseSettings;
        }

        public IViewComponentResult Invoke(KnowledgebaseHomePageModel model)
        {
            if (!_knowledgebaseSettings.Enabled)
                return Content("");

            var articles = _knowledgebaseService.GetHomepageKnowledgebaseArticles();

            foreach (var article in articles)
            {
                var a = new KnowledgebaseItemModel
                {
                    Id = article.Id,
                    Name = article.GetLocalized(y => y.Name),
                    SeName = article.GetLocalized(y => y.SeName),
                    IsArticle = true
                };

                model.Items.Add(a);
            }

            return View(model);
        }
    }
}
