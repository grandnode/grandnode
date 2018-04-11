using Grand.Core.Domain.Knowledgebase;
using Grand.Services.Knowledgebase;
using Grand.Web.Models.Knowledgebase;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.ViewComponents
{
    public class KnowledgebaseCategories : ViewComponent
    {
        private readonly IKnowledgebaseService _knowledgebaseService;
        private readonly KnowledgebaseSettings _knowledgebaseSettings;

        public KnowledgebaseCategories(IKnowledgebaseService knowledgebaseService, KnowledgebaseSettings knowledgebaseSettings)
        {
            this._knowledgebaseService = knowledgebaseService;
            this._knowledgebaseSettings = knowledgebaseSettings;
        }

        public IViewComponentResult Invoke()
        {
            if (!_knowledgebaseSettings.Enabled)
                return Content("");

            var model = new KnowledgebaseHomePageModel();

            var categories = _knowledgebaseService.GetPublicKnowledgebaseCategories();

            foreach (var category in categories)
            {
                if (string.IsNullOrEmpty(category.ParentCategoryId))
                {
                    var newNode = new KnowledgebaseCategoryModel
                    {
                        Id = category.Id,
                        Name = category.Name,
                        Children = new List<KnowledgebaseCategoryModel>()
                    };

                    FillChildNodes(newNode, categories);

                    model.Categories.Add(newNode);
                }
            }

            return View(model);
        }

        public void FillChildNodes(KnowledgebaseCategoryModel parentNode, List<KnowledgebaseCategory> nodes)
        {
            var children = nodes.Where(x => x.ParentCategoryId == parentNode.Id);
            foreach (var child in children)
            {
                var newNode = new KnowledgebaseCategoryModel
                {
                    Id = child.Id,
                    Name = child.Name,
                    Children = new List<KnowledgebaseCategoryModel>()
                };

                FillChildNodes(newNode, nodes);

                parentNode.Children.Add(newNode);
            }
        }
    }
}
