using Grand.Domain;
using Grand.Domain.Knowledgebase;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Services.Knowledgebase
{
    public interface IKnowledgebaseService
    {
        /// <summary>
        /// Gets knowledgebase category
        /// </summary>
        /// <param name="id"></param>
        /// <returns>knowledgebase category</returns>
        Task<KnowledgebaseCategory> GetKnowledgebaseCategory(string id);

        /// <summary>
        /// Gets public knowledgebase category
        /// </summary>
        /// <param name="id"></param>
        /// <returns>knowledgebase category</returns>
        Task<KnowledgebaseCategory> GetPublicKnowledgebaseCategory(string id);

        /// <summary>
        /// Gets knowledgebase categories
        /// </summary>
        /// <returns>List of knowledgebase categories</returns>
        Task<List<KnowledgebaseCategory>> GetKnowledgebaseCategories();

        /// <summary>
        /// Gets public(published etc) knowledgebase categories
        /// </summary>
        /// <returns>List of public knowledgebase categories</returns>
        Task<List<KnowledgebaseCategory>> GetPublicKnowledgebaseCategories();

        /// <summary>
        /// Inserts knowledgebase category
        /// </summary>
        /// <param name="kc"></param>
        Task InsertKnowledgebaseCategory(KnowledgebaseCategory kc);

        /// <summary>
        /// Updates knowledgebase category
        /// </summary>
        /// <param name="kc"></param>
        Task UpdateKnowledgebaseCategory(KnowledgebaseCategory kc);

        /// <summary>
        /// Deletes knowledgebase category
        /// </summary>
        /// <param name="kc"></param>
        Task DeleteKnowledgebaseCategory(KnowledgebaseCategory kc);

        /// <summary>
        /// Gets knowledgebase article
        /// </summary>
        /// <param name="id"></param>
        /// <returns>knowledgebase article</returns>
        Task<KnowledgebaseArticle> GetKnowledgebaseArticle(string id);

        /// <summary>
        /// Gets public knowledgebase article
        /// </summary>
        /// <param name="id"></param>
        /// <returns>knowledgebase article</returns>
        Task<KnowledgebaseArticle> GetPublicKnowledgebaseArticle(string id);

        /// <summary>
        /// Gets knowledgebase articles
        /// </summary>
        /// <returns>List of knowledgebase articles</returns>
        /// <param name="storeId">Store ident</param>
        Task<List<KnowledgebaseArticle>> GetKnowledgebaseArticles(string storeId = "");

        /// <summary>
        /// Gets public(published etc) knowledgebase articles
        /// </summary>
        /// <returns>List of public knowledgebase articles</returns>
        Task<List<KnowledgebaseArticle>> GetPublicKnowledgebaseArticles();

        /// <summary>
        /// Gets homepage knowledgebase articles
        /// </summary>
        /// <returns>List of homepage knowledgebase articles</returns>
        Task<List<KnowledgebaseArticle>> GetHomepageKnowledgebaseArticles();

        /// <summary>
        /// Gets public(published etc) knowledgebase articles for category id
        /// </summary>
        /// <returns>List of public knowledgebase articles</returns>
        Task<List<KnowledgebaseArticle>> GetPublicKnowledgebaseArticlesByCategory(string categoryId);

        /// <summary>
        /// Gets public(published etc) knowledgebase articles for keyword
        /// </summary>
        /// <returns>List of public knowledgebase articles</returns>
        Task<List<KnowledgebaseArticle>> GetPublicKnowledgebaseArticlesByKeyword(string keyword);

        /// <summary>
        /// Gets public(published etc) knowledgebase categories for keyword
        /// </summary>
        /// <returns>List of public knowledgebase categories</returns>
        Task<List<KnowledgebaseCategory>> GetPublicKnowledgebaseCategoriesByKeyword(string keyword);

        /// <summary>
        /// Inserts knowledgebase article
        /// </summary>
        /// <param name="ka"></param>
        Task InsertKnowledgebaseArticle(KnowledgebaseArticle ka);

        /// <summary>
        /// Updates knowledgebase article
        /// </summary>
        /// <param name="ka"></param>
        Task UpdateKnowledgebaseArticle(KnowledgebaseArticle ka);

        /// <summary>
        /// Deletes knowledgebase article
        /// </summary>
        /// <param name="ka"></param>
        Task DeleteKnowledgebaseArticle(KnowledgebaseArticle ka);

        /// <summary>
        /// Gets knowledgebase articles by category id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>IPagedList<KnowledgebaseArticle></returns>
        Task<IPagedList<KnowledgebaseArticle>> GetKnowledgebaseArticlesByCategoryId(string id, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets knowledgebase articles by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns>IPagedList<KnowledgebaseArticle></returns>
        Task<IPagedList<KnowledgebaseArticle>> GetKnowledgebaseArticlesByName(string name, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets related knowledgebase articles
        /// </summary>
        /// <param name="name"></param>
        /// <returns>IPagedList<KnowledgebaseArticle></returns>
        Task<IPagedList<KnowledgebaseArticle>> GetRelatedKnowledgebaseArticles(string articleId, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Inserts an article comment
        /// </summary>
        /// <param name="articleComment">Article comment</param>
        Task InsertArticleComment(KnowledgebaseArticleComment articleComment);

        /// <summary>
        /// Gets all comments
        /// </summary>
        /// <param name="customerId">Customer identifier; "" to load all records</param>
        /// <returns>Comments</returns>
        Task<IList<KnowledgebaseArticleComment>> GetAllComments(string customerId);

        /// <summary>
        /// Gets an article comment
        /// </summary>
        /// <param name="articleId">Article identifier</param>
        /// <returns>Article comment</returns>
        Task<KnowledgebaseArticleComment> GetArticleCommentById(string articleId);

        /// <summary>
        /// Get article comments by identifiers
        /// </summary>
        /// <param name="commentIds"Article comment identifiers</param>
        /// <returns>Article comments</returns>
        Task<IList<KnowledgebaseArticleComment>> GetArticleCommentsByIds(string[] commentIds);
        Task<IList<KnowledgebaseArticleComment>> GetArticleCommentsByArticleId(string articleId);
        Task DeleteArticleComment(KnowledgebaseArticleComment articleComment);
    }
}
