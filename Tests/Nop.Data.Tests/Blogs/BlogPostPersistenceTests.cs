using System;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Tests;
using NUnit.Framework;

namespace Nop.Data.Tests.Blogs
{
    [TestFixture]
    public class BlogPostPersistenceTests : PersistenceTest
    {
        [Test]
        public void Can_save_and_load_blogPost()
        {
            var blogPost = new BlogPost
            {
                Title = "Title 1",
                Body = "Body 1",
                BodyOverview = "BodyOverview 1",
                AllowComments = true,
                CommentCount = 1,
                Tags = "Tags 1",
                StartDateUtc = new DateTime(2010, 01, 01),
                EndDateUtc = new DateTime(2010, 01, 02),
                CreatedOnUtc = new DateTime(2010, 01, 03),
                MetaTitle = "MetaTitle 1",
                MetaDescription = "MetaDescription 1",
                MetaKeywords = "MetaKeywords 1",
                LimitedToStores = true,
                LanguageId = 1
            };

            var fromDb = SaveAndLoadEntity(blogPost);
            fromDb.ShouldNotBeNull();
            fromDb.Title.ShouldEqual("Title 1");
            fromDb.Body.ShouldEqual("Body 1");
            fromDb.BodyOverview.ShouldEqual("BodyOverview 1");
            fromDb.AllowComments.ShouldEqual(true);
            fromDb.CommentCount.ShouldEqual(1);
            fromDb.Tags.ShouldEqual("Tags 1");
            fromDb.StartDateUtc.ShouldEqual(new DateTime(2010, 01, 01));
            fromDb.EndDateUtc.ShouldEqual(new DateTime(2010, 01, 02));
            fromDb.CreatedOnUtc.ShouldEqual(new DateTime(2010, 01, 03));
            fromDb.MetaTitle.ShouldEqual("MetaTitle 1");
            fromDb.MetaDescription.ShouldEqual("MetaDescription 1");
            fromDb.MetaKeywords.ShouldEqual("MetaKeywords 1");
            fromDb.LimitedToStores.ShouldEqual(true);

        }

        [Test]
        public void Can_save_and_load_blogPost_with_blogComments()
        {
            var blogPost = new BlogPost
            {
                Title = "Title 1",
                Body = "Body 1",
                AllowComments = true,
                CreatedOnUtc = new DateTime(2010, 01, 01),
                LanguageId = 1
            };
            
            var fromDb = SaveAndLoadEntity(blogPost);
            fromDb.ShouldNotBeNull();

        }

        protected Customer GetTestCustomer()
        {
            return new Customer
            {
                CustomerGuid = Guid.NewGuid(),
                CreatedOnUtc = new DateTime(2010, 01, 01),
                LastActivityDateUtc = new DateTime(2010, 01, 02)
            };
        }
    }
}
