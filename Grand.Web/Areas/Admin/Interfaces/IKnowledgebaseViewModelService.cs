using Grand.Core.Domain.Knowledgebase;
using Grand.Web.Areas.Admin.Models.Knowledgebase;
using System.Collections.Generic;

namespace Grand.Web.Areas.Admin.Interfaces
{
    public interface IKnowledgebaseViewModelService
    {
        List<TreeNode> PrepareTreeNode();
        void PrepareCategory(KnowledgebaseCategoryModel model);
        void PrepareCategory(KnowledgebaseArticleModel model);
        (IEnumerable<KnowledgebaseArticleGridModel> knowledgebaseArticleGridModels, int totalCount) PrepareKnowledgebaseArticleGridModel(string parentCategoryId, int pageIndex, int pageSize);
        (IEnumerable<KnowledgebaseCategoryModel.ActivityLogModel> activityLogModels, int totalCount) PrepareCategoryActivityLogModels(string categoryId, int pageIndex, int pageSize);
        (IEnumerable<KnowledgebaseArticleModel.ActivityLogModel> activityLogModels, int totalCount) PrepareArticleActivityLogModels(string articleId, int pageIndex, int pageSize);
        KnowledgebaseCategoryModel PrepareKnowledgebaseCategoryModel();
        KnowledgebaseCategory InsertKnowledgebaseCategoryModel(KnowledgebaseCategoryModel model);
        KnowledgebaseCategory UpdateKnowledgebaseCategoryModel(KnowledgebaseCategory knowledgebaseCategory, KnowledgebaseCategoryModel model);
        void DeleteKnowledgebaseCategoryModel(KnowledgebaseCategory knowledgebaseCategory);
        KnowledgebaseArticleModel PrepareKnowledgebaseArticleModel();
        KnowledgebaseArticle InsertKnowledgebaseArticleModel(KnowledgebaseArticleModel model);
        KnowledgebaseArticle UpdateKnowledgebaseArticleModel(KnowledgebaseArticle knowledgebaseArticle, KnowledgebaseArticleModel model);
        void DeleteKnowledgebaseArticle(KnowledgebaseArticle knowledgebaseArticle);
        void InsertKnowledgebaseRelatedArticle(KnowledgebaseArticleModel.AddRelatedArticleModel model);
        void DeleteKnowledgebaseRelatedArticle(KnowledgebaseArticleModel.AddRelatedArticleModel model);
    }
}
