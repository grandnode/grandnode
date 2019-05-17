﻿using Grand.Framework.Controllers;
using Grand.Framework.Kendoui;
using Grand.Framework.Mvc;
using Grand.Framework.Mvc.Filters;
using Grand.Framework.Security.Authorization;
using Grand.Services.Blogs;
using Grand.Services.Localization;
using Grand.Services.Security;
using Grand.Services.Seo;
using Grand.Services.Stores;
using Grand.Web.Areas.Admin.Extensions;
using Grand.Web.Areas.Admin.Models.Blogs;
using Grand.Web.Areas.Admin.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Web.Areas.Admin.Controllers
{
    [PermissionAuthorize(PermissionSystemName.Blog)]
    public partial class BlogController : BaseAdminController
    {
        #region Fields

        private readonly IBlogService _blogService;
        private readonly IBlogViewModelService _blogViewModelService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreService _storeService;
        private readonly IUrlRecordService _urlRecordService;

        #endregion

        #region Constructors

        public BlogController(IBlogService blogService, IBlogViewModelService blogViewModelService, ILanguageService languageService, ILocalizationService localizationService,
            IStoreService storeService, IUrlRecordService urlRecordService)
        {
            this._blogService = blogService;
            this._blogViewModelService = blogViewModelService;
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._storeService = storeService;
            this._urlRecordService = urlRecordService;
        }

        #endregion

        #region Blog posts

        public IActionResult Index() => RedirectToAction("List");

        public IActionResult List() => View();

        [HttpPost]
        public async Task<IActionResult> List(DataSourceRequest command)
        {
            var blogPosts = await _blogViewModelService.PrepareBlogPostsModel(command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = blogPosts.blogPosts,
                Total = blogPosts.totalCount
            };
            return Json(gridModel);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.AllLanguages = await _languageService.GetAllLanguages(true);
            var model = await _blogViewModelService.PrepareBlogPostModel();
            //locales
            await AddLocales(_languageService, model.Locales);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(BlogPostModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var blogPost = await _blogViewModelService.InsertBlogPostModel(model);
                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Blog.BlogPosts.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = blogPost.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = await _languageService.GetAllLanguages(true);
            model = await _blogViewModelService.PrepareBlogPostModel(model);
            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var blogPost = await _blogService.GetBlogPostById(id);
            if (blogPost == null)
                //No blog post found with the specified id
                return RedirectToAction("List");

            ViewBag.AllLanguages = await _languageService.GetAllLanguages(true);
            var model = await _blogViewModelService.PrepareBlogPostModel(blogPost);
            //locales
            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Title = blogPost.GetLocalized(x => x.Title, languageId, false, false);
                locale.Body = blogPost.GetLocalized(x => x.Body, languageId, false, false);
                locale.BodyOverview = blogPost.GetLocalized(x => x.BodyOverview, languageId, false, false);
                locale.MetaKeywords = blogPost.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaDescription = blogPost.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaTitle = blogPost.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = blogPost.GetSeName(languageId, false, false);
            });
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(BlogPostModel model, bool continueEditing)
        {
            var blogPost = await _blogService.GetBlogPostById(model.Id);
            if (blogPost == null)
                //No blog post found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                blogPost = await _blogViewModelService.UpdateBlogPostModel(model, blogPost);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Blog.BlogPosts.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("Edit", new { id = blogPost.Id });
                }
                return RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = await _languageService.GetAllLanguages(true);

            model = await _blogViewModelService.PrepareBlogPostModel(model, blogPost);
            //locales
            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Title = blogPost.GetLocalized(x => x.Title, languageId, false, false);
                locale.Body = blogPost.GetLocalized(x => x.Body, languageId, false, false);
                locale.BodyOverview = blogPost.GetLocalized(x => x.BodyOverview, languageId, false, false);
                locale.MetaKeywords = blogPost.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaDescription = blogPost.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaTitle = blogPost.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = blogPost.GetSeName(languageId, false, false);
            });
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var blogPost = await _blogService.GetBlogPostById(id);
            if (blogPost == null)
                //No blog post found with the specified id
                return RedirectToAction("List");

            var urlRecord = await _urlRecordService.GetBySlug(blogPost.SeName);
            if (urlRecord == null)
            {
                //No url record found with the SeName
                return RedirectToAction("List");
            }

            if (ModelState.IsValid)
            {
                urlRecord.IsActive = false;
                await _blogService.DeleteBlogPost(blogPost, urlRecord);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Blog.BlogPosts.Deleted"));
                return RedirectToAction("List");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("Edit", new { id = id });
        }

        #endregion

        #region Categories
        public IActionResult CategoryList() => View();

        [HttpPost]
        public async Task<IActionResult> CategoryList(DataSourceRequest command)
        {
            var categories = await _blogService.GetAllBlogCategories();
            var gridModel = new DataSourceResult
            {
                Data = categories,
                Total = categories.Count
            };
            return Json(gridModel);
        }

        public async Task<IActionResult> CategoryCreate()
        {
            ViewBag.AllLanguages = await _languageService.GetAllLanguages(true);
            var model = new BlogCategoryModel();
            //locales
            await AddLocales(_languageService, model.Locales);
            //Stores
            await model.PrepareStoresMappingModel(null, false, _storeService);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> CategoryCreate(BlogCategoryModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var blogCategory = model.ToEntity();
                await _blogService.InsertBlogCategory(blogCategory);
                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Blog.BlogCategory.Added"));
                return continueEditing ? RedirectToAction("CategoryEdit", new { id = blogCategory.Id }) : RedirectToAction("CategoryList");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = await _languageService.GetAllLanguages(true);
            //locales
            await AddLocales(_languageService, model.Locales);
            //Stores
            await model.PrepareStoresMappingModel(null, true, _storeService);

            return View(model);
        }

        public async Task<IActionResult> CategoryEdit(string id)
        {
            var blogCategory = await _blogService.GetBlogCategoryById(id);
            if (blogCategory == null)
                //No blog post found with the specified id
                return RedirectToAction("CategoryList");

            ViewBag.AllLanguages = await _languageService.GetAllLanguages(true);
            var model = blogCategory.ToModel();
            //locales
            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = blogCategory.GetLocalized(x => x.Name, languageId, false, false);
            });
            //Store
            await model.PrepareStoresMappingModel(blogCategory, false, _storeService);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> CategoryEdit(BlogCategoryModel model, bool continueEditing)
        {
            var blogCategory = await _blogService.GetBlogCategoryById(model.Id);
            if (blogCategory == null)
                //No blog post found with the specified id
                return RedirectToAction("CategoryList");

            if (ModelState.IsValid)
            {
                blogCategory = model.ToEntity(blogCategory);
                await _blogService.UpdateBlogCategory(blogCategory);
                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Blog.BlogCategory.Updated"));
                if (continueEditing)
                {
                    //selected tab
                    SaveSelectedTabIndex();

                    return RedirectToAction("CategoryEdit", new { id = blogCategory.Id });
                }
                return RedirectToAction("CategoryList");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = await _languageService.GetAllLanguages(true);

            //locales
            await AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = blogCategory.GetLocalized(x => x.Name, languageId, false, false);
            });
            //Store
            await model.PrepareStoresMappingModel(blogCategory, true, _storeService);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CategoryDelete(string id)
        {
            var blogcategory = await _blogService.GetBlogCategoryById(id);
            if (blogcategory == null)
                //No blog post found with the specified id
                return RedirectToAction("CategoryList");

            if (ModelState.IsValid)
            {
                await _blogService.DeleteBlogCategory(blogcategory);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.Blog.BlogCategory.Deleted"));
                return RedirectToAction("CategoryList");
            }
            ErrorNotification(ModelState);
            return RedirectToAction("CategoryEdit", new { id = id });
        }

        [HttpPost]
        public async Task<IActionResult> CategoryPostList(string categoryId)
        {
            var blogCategory = await _blogService.GetBlogCategoryById(categoryId);
            if (blogCategory == null)
                return ErrorForKendoGridJson("blogCategory no exists");

            var gridModel = new DataSourceResult
            {
                Data =  blogCategory.BlogPosts.Select(x => new { Id = x.Id, BlogPostId = x.BlogPostId, Name = _blogService.GetBlogPostById(x.BlogPostId).Result?.Title }),
                Total = blogCategory.BlogPosts.Count
            };
            return Json(gridModel);
        }

        public async Task<IActionResult> CategoryPostDelete(string categoryId, string id)
        {
            var blogCategory = await _blogService.GetBlogCategoryById(categoryId);
            if (blogCategory == null)
                return ErrorForKendoGridJson("blogCategory no exists");
            if (ModelState.IsValid)
            {
                var post = blogCategory.BlogPosts.FirstOrDefault(x => x.Id == id);
                if (post != null)
                {
                    blogCategory.BlogPosts.Remove(post);
                    await _blogService.UpdateBlogCategory(blogCategory);
                }
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }

        public async Task<IActionResult> BlogPostAddPopup(string categoryId)
        {
            var model = new AddBlogPostCategoryModel();
            //stores
            model.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = " " });
            foreach (var s in await _storeService.GetAllStores())
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
            model.CategoryId = categoryId;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> BlogPostAddPopupList(DataSourceRequest command, AddBlogPostCategoryModel model)
        {
            var gridModel = new DataSourceResult();
            var posts = await _blogService.GetAllBlogPosts(storeId: model.SearchStoreId, blogPostName: model.SearchBlogTitle, pageIndex: command.Page - 1, pageSize: command.PageSize);
            gridModel.Data = posts.Select(x => new { Id = x.Id, Name = x.Title });
            gridModel.Total = posts.TotalCount;

            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public async Task<IActionResult> BlogPostAddPopup(AddBlogPostCategoryModel model)
        {
            if (model.SelectedBlogPostIds != null)
            {
                var blogCategory = await _blogService.GetBlogCategoryById(model.CategoryId);
                if (blogCategory != null)
                    foreach (string id in model.SelectedBlogPostIds)
                    {
                        var post = _blogService.GetBlogPostById(id);
                        if (post != null)
                        {
                            if (blogCategory.BlogPosts.Where(x => x.BlogPostId == id).Count() == 0)
                            {
                                blogCategory.BlogPosts.Add(new Core.Domain.Blogs.BlogCategoryPost() { BlogPostId = id });
                                await _blogService.UpdateBlogCategory(blogCategory);
                            }
                        }
                    }
            }
            ViewBag.RefreshPage = true;
            return View(model);
        }

        #endregion

        #region Comments

        public IActionResult Comments(string filterByBlogPostId)
        {
            ViewBag.FilterByBlogPostId = filterByBlogPostId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Comments(string filterByBlogPostId, DataSourceRequest command)
        {
            var model = await _blogViewModelService.PrepareBlogPostCommentsModel(filterByBlogPostId, command.Page, command.PageSize);
            var gridModel = new DataSourceResult
            {
                Data = model.blogComments,
                Total = model.totalCount,
            };
            return Json(gridModel);
        }

        public async Task<IActionResult> CommentDelete(string id)
        {
            var comment = await _blogService.GetBlogCommentById(id);
            if (comment == null)
                throw new ArgumentException("No comment found with the specified id");

            var blogPost = await _blogService.GetBlogPostById(comment.BlogPostId);
            if (ModelState.IsValid)
            {
                await _blogService.DeleteBlogComment(comment);
                //update totals
                var comments = await _blogService.GetBlogCommentsByBlogPostId(blogPost.Id);
                blogPost.CommentCount = comments.Count;
                await _blogService.UpdateBlogPost(blogPost);
                return new NullJsonResult();
            }
            return ErrorForKendoGridJson(ModelState);
        }
        #endregion

        #region Products

        [HttpPost]
        public async Task<IActionResult> Products(string blogPostId, DataSourceRequest command)
        {
            var model = await _blogViewModelService.PrepareBlogProductsModel(blogPostId, command.Page, command.PageSize);
            var gridModel = new DataSourceResult {
                Data = model.blogProducts,
                Total = model.totalCount,
            };
            return Json(gridModel);
        }

        public async Task<IActionResult> ProductAddPopup(string blogPostId)
        {
            var model = await _blogViewModelService.PrepareBlogModelAddProductModel(blogPostId);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ProductAddPopupList(DataSourceRequest command, BlogProductModel.AddProductModel model)
        {
            var products = await _blogViewModelService.PrepareProductModel(model, command.Page, command.PageSize);

            var gridModel = new DataSourceResult {
                Data = products.products.ToList(),
                Total = products.totalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public async Task<IActionResult> ProductAddPopup(string blogPostId, BlogProductModel.AddProductModel model)
        {
            if (model.SelectedProductIds != null)
            {
                await _blogViewModelService.InsertProductModel(blogPostId, model);
            }

            ViewBag.RefreshPage = true;
            return View(model);
        }

        public async Task<IActionResult> UpdateProduct(BlogProductModel model)
        {
            await _blogViewModelService.UpdateProductModel(model);
            return new NullJsonResult();
        }

        public async Task<IActionResult> DeleteProduct(string id)
        {
            await _blogViewModelService.DeleteProductModel(id);
            return new NullJsonResult();
        }

        #endregion
    }
}
