using Grand.Domain.Data;
using Grand.Domain.Blogs;
using Grand.Domain.Catalog;
using Grand.Core.Events;
using Grand.Services.Blogs;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Grand.Services.Tests.Blogs
{
    [TestClass()]
    public class BlogServiceTests
    {
        private Mock<IRepository<BlogPost>> _blogPostRepositoryMock;
        private Mock<IRepository<BlogComment>> _blogCommentRepositoryMock;
        private Mock<IRepository<BlogCategory>> _blogCategoryRepositoryMock;
        private Mock<IRepository<BlogProduct>> _blogProductRepositoryMock;
        private Mock<IMediator> _mediatorMock;
        private CatalogSettings _settings;
        private BlogService _blogService;

        [TestInitialize()]
        public void Init()
        {
            _blogPostRepositoryMock = new Mock<IRepository<BlogPost>>();
            _blogCommentRepositoryMock = new Mock<IRepository<BlogComment>>();
            _blogCategoryRepositoryMock = new Mock<IRepository<BlogCategory>>();
            _blogProductRepositoryMock = new Mock<IRepository<BlogProduct>>();
            _mediatorMock = new Mock<IMediator>();
            _settings = new CatalogSettings();
            _blogService = new BlogService(_blogPostRepositoryMock.Object, _blogCommentRepositoryMock.Object,
                _blogCategoryRepositoryMock.Object, _blogProductRepositoryMock.Object,_settings,_mediatorMock.Object);
        }
        
        [TestMethod()]
        public void DeleteBlogPost_NullArgument_ThrowException()
        {
            Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _blogService.DeleteBlogPost(null), "blogPost");
        }

        [TestMethod()]
        public async Task DeleteBlogPost_ValidArgument()
        {
            await _blogService.DeleteBlogPost(new BlogPost());
            _blogPostRepositoryMock.Verify(c => c.DeleteAsync(It.IsAny<BlogPost>()), Times.Once);
            _mediatorMock.Verify(c => c.Publish(It.IsAny<EntityDeleted<BlogPost>>(), default(CancellationToken)), Times.Once);
        }

        [TestMethod()]
        public async Task GetBlogPostById()
        {
            await _blogService.GetBlogPostById("id");
            _blogPostRepositoryMock.Verify(c => c.GetByIdAsync(It.IsAny<string>()), Times.Once);
        }

        [TestMethod()]
        public void InsertBlogPost_NullArgument_ThrowException()
        {
            Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _blogService.InsertBlogPost(null ), "blogPost");
        }

        [TestMethod()]
        public async Task InsertBlogPost_ValiArgument__InvokeRepository()
        {
            await _blogService.InsertBlogPost(new BlogPost());
            _blogPostRepositoryMock.Verify(c => c.InsertAsync(It.IsAny<BlogPost>()), Times.Once);
            _mediatorMock.Verify(c => c.Publish(It.IsAny<EntityInserted<BlogPost>>(), default(CancellationToken)), Times.Once);
        }

        [TestMethod()]
        public async Task UpdateBlogPost_ValiArgument_InvokeRepository()
        {
            await _blogService.UpdateBlogPost(new BlogPost());
            _blogPostRepositoryMock.Verify(c => c.UpdateAsync(It.IsAny<BlogPost>()), Times.Once);
            _mediatorMock.Verify(c => c.Publish(It.IsAny<EntityUpdated<BlogPost>>(), default(CancellationToken)), Times.Once);
        }

        [TestMethod()]
        public void UpdateBlogPost_NullArgument_ThrowException()
        {
            Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _blogService.UpdateBlogPost(null), "blogPost");
        }

        [TestMethod()]
        public async Task InsertBlogProduct_ValiArgument__InvokeRepository()
        {
            await _blogService.InsertBlogProduct(new BlogProduct());
            _blogProductRepositoryMock.Verify(c => c.InsertAsync(It.IsAny<BlogProduct>()), Times.Once);
            _mediatorMock.Verify(c => c.Publish(It.IsAny<EntityInserted<BlogProduct>>(), default(CancellationToken)), Times.Once);
        }

        [TestMethod()]
        public void InsertBlogProductInsertBlogProduct_NullArgument_ThrowException()
        {
            Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _blogService.InsertBlogProduct(null), "blogProduct");
        }

        [TestMethod()]
        public async Task UpdateBlogProduct_ValiArgument_InvokeRepository()
        {
            await _blogService.UpdateBlogProduct(new BlogProduct());
            _blogProductRepositoryMock.Verify(c => c.UpdateAsync(It.IsAny<BlogProduct>()), Times.Once);
            _mediatorMock.Verify(c => c.Publish(It.IsAny<EntityUpdated<BlogProduct>>(), default(CancellationToken)), Times.Once);
        }

        [TestMethod()]
        public void UpdateBlogProduct_NullArgument_ThrowException()
        {
            Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _blogService.UpdateBlogProduct(null), "blogProduct");
        }

        [TestMethod()]
        public async Task DeleteBlogProduct_ValidArgument_InvokeRepository()
        {
            await _blogService.DeleteBlogProduct(new BlogProduct());
            _blogProductRepositoryMock.Verify(c => c.DeleteAsync(It.IsAny<BlogProduct>()), Times.Once);
            _mediatorMock.Verify(c => c.Publish(It.IsAny<EntityDeleted<BlogProduct>>(), default(CancellationToken)), Times.Once);
        }

        [TestMethod()]
        public void DeleteBlogProduct_NullArgument_ThrowException()
        {
            Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _blogService.DeleteBlogProduct(null), "blogProduct");
        }

        [TestMethod()]
        public async Task InsertBlogCategory_ValiArgument__InvokeRepository()
        {
            await _blogService.InsertBlogCategory(new BlogCategory());
            _blogCategoryRepositoryMock.Verify(c => c.InsertAsync(It.IsAny<BlogCategory>()), Times.Once);
            _mediatorMock.Verify(c => c.Publish(It.IsAny<EntityInserted<BlogCategory>>(), default(CancellationToken)), Times.Once);
        }

        [TestMethod()]
        public void InsertBlogCategory_NullArgument_ThrowException()
        {
            Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _blogService.InsertBlogCategory(null), "blogCategory");
        }

        [TestMethod()]
        public async Task UpdateBlogCategory_ValiArgument_InvokeRepository()
        {
            await _blogService.UpdateBlogCategory(new BlogCategory());
            _blogCategoryRepositoryMock.Verify(c => c.UpdateAsync(It.IsAny<BlogCategory>()), Times.Once);
            _mediatorMock.Verify(c => c.Publish(It.IsAny<EntityUpdated<BlogCategory>>(), default(CancellationToken)), Times.Once);
        }

        [TestMethod()]
        public void UpdateBlogCategory_NullArgument_ThrowException()
        {
            Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _blogService.UpdateBlogCategory(null), "blogCategory");
        }

        [TestMethod()]
        public async Task DeleteBlogCategory_ValidArgument_InvokeRepository()
        {
            await _blogService.DeleteBlogCategory(new BlogCategory());
            _blogCategoryRepositoryMock.Verify(c => c.DeleteAsync(It.IsAny<BlogCategory>()), Times.Once);
            _mediatorMock.Verify(c => c.Publish(It.IsAny<EntityDeleted<BlogCategory>>(), default(CancellationToken)), Times.Once);
        }

        [TestMethod()]
        public void DeleteBlogCategory_NullArgument_ThrowException()
        {
            Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _blogService.DeleteBlogCategory(null), "blogCategory");
        }

        [TestMethod()]
        public async Task DeleteBlogComments_ValidArgument_InvokeRepository()
        {
            await _blogService.DeleteBlogComment(new BlogComment());
            _blogCommentRepositoryMock.Verify(c => c.DeleteAsync(It.IsAny<BlogComment>()), Times.Once);
            //_mediatorMock.Verify(c => c.Publish(It.IsAny<EntityDeleted<BlogComment>>(), default(CancellationToken)), Times.Once);
        }

        [TestMethod()]
        public async Task InsertBlogComment_ValiArgument__InvokeRepository()
        {
            await _blogService.InsertBlogComment(new BlogComment());
            _blogCommentRepositoryMock.Verify(c => c.InsertAsync(It.IsAny<BlogComment>()), Times.Once);
            _mediatorMock.Verify(c => c.Publish(It.IsAny<EntityInserted<BlogComment>>(), default(CancellationToken)), Times.Once);
        }
    }
}
