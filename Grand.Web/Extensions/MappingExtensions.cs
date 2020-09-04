using Grand.Domain.Catalog;
using Grand.Domain.Common;
using Grand.Domain.Courses;
using Grand.Domain.Customers;
using Grand.Domain.Forums;
using Grand.Domain.Localization;
using Grand.Domain.Polls;
using Grand.Domain.Topics;
using Grand.Services.Localization;
using Grand.Services.Seo;
using Grand.Web.Models.Boards;
using Grand.Web.Models.Catalog;
using Grand.Web.Models.Common;
using Grand.Web.Models.Course;
using Grand.Web.Models.Polls;
using Grand.Web.Models.Topics;
using Grand.Web.Models.Vendors;
using Microsoft.AspNetCore.Http;
using System;
using System.Globalization;
using System.Linq;

namespace Grand.Web.Extensions
{
    public static class MappingExtensions
    {
        //category
        public static CategoryModel ToModel(this Category entity, Language language)
        {
            if (entity == null)
                return null;

            var model = new CategoryModel {
                Id = entity.Id,
                ParentCategoryId = entity.ParentCategoryId,
                Name = entity.GetLocalized(x => x.Name, language.Id),
                Description = entity.GetLocalized(x => x.Description, language.Id),
                BottomDescription = entity.GetLocalized(x => x.BottomDescription, language.Id),
                MetaKeywords = entity.GetLocalized(x => x.MetaKeywords, language.Id),
                MetaDescription = entity.GetLocalized(x => x.MetaDescription, language.Id),
                MetaTitle = entity.GetLocalized(x => x.MetaTitle, language.Id),
                SeName = entity.GetSeName(language.Id),
                Flag = entity.GetLocalized(x => x.Flag, language.Id),
                FlagStyle = entity.FlagStyle,
                Icon = entity.Icon,
                GenericAttributes = entity.GenericAttributes
            };
            return model;
        }

        //manufacturer
        public static ManufacturerModel ToModel(this Manufacturer entity, Language language)
        {
            if (entity == null)
                return null;

            var model = new ManufacturerModel
            {
                Id = entity.Id,
                Name = entity.GetLocalized(x => x.Name, language.Id),
                Description = entity.GetLocalized(x => x.Description, language.Id),
                BottomDescription = entity.GetLocalized(x => x.BottomDescription, language.Id),
                MetaKeywords = entity.GetLocalized(x => x.MetaKeywords, language.Id),
                MetaDescription = entity.GetLocalized(x => x.MetaDescription, language.Id),
                MetaTitle = entity.GetLocalized(x => x.MetaTitle, language.Id),
                SeName = entity.GetSeName(language.Id),
                Icon = entity.Icon,
                GenericAttributes = entity.GenericAttributes
            };
            return model;
        }

        //course
        public static CourseModel ToModel(this Course entity, Language language)
        {
            if (entity == null)
                return null;

            var model = new CourseModel {
                Id = entity.Id,
                Name = entity.GetLocalized(x => x.Name, language.Id),
                Description = entity.GetLocalized(x => x.Description, language.Id),
                ShortDescription = entity.GetLocalized(x => x.ShortDescription, language.Id),
                MetaKeywords = entity.GetLocalized(x => x.MetaKeywords, language.Id),
                MetaDescription = entity.GetLocalized(x => x.MetaDescription, language.Id),
                MetaTitle = entity.GetLocalized(x => x.MetaTitle, language.Id),
                SeName = entity.GetSeName(language.Id),
                GenericAttributes = entity.GenericAttributes
            };
            return model;
        }

        //poll
        public static PollModel ToModel(this Poll entity, Language language, Customer customer)
        {
            if (entity == null)
                return null;

            var model = new PollModel {
                Id = entity.Id,
                Name = entity.GetLocalized(x => x.Name, language.Id)
            };
            model.AlreadyVoted = entity.
                PollAnswers.Any(x=>x.PollVotingRecords.Any(z => z.CustomerId == customer.Id));

            var answers = entity.PollAnswers.OrderBy(x => x.DisplayOrder);
            foreach (var answer in answers)
                model.TotalVotes += answer.NumberOfVotes;
            foreach (var pa in answers)
            {
                model.Answers.Add(new PollAnswerModel {
                    Id = pa.Id,
                    PollId = entity.Id,
                    Name = pa.GetLocalized(x => x.Name, language.Id),
                    NumberOfVotes = pa.NumberOfVotes,
                    PercentOfTotalVotes = model.TotalVotes > 0 ? ((Convert.ToDouble(pa.NumberOfVotes) / Convert.ToDouble(model.TotalVotes)) * Convert.ToDouble(100)) : 0,
                });
            }

            return model;

        }
        
        //topic
        public static TopicModel ToModel(this Topic entity, Language language)
        {
            var model = new TopicModel {
                Id = entity.Id,
                SystemName = entity.SystemName,
                IncludeInSitemap = entity.IncludeInSitemap,
                IsPasswordProtected = entity.IsPasswordProtected,
                Password = entity.Password,
                Title = entity.IsPasswordProtected ? "" : entity.GetLocalized(x => x.Title, language.Id),
                Body = entity.IsPasswordProtected ? "" : entity.GetLocalized(x => x.Body, language.Id),
                MetaKeywords = entity.GetLocalized(x => x.MetaKeywords, language.Id),
                MetaDescription = entity.GetLocalized(x => x.MetaDescription, language.Id),
                MetaTitle = entity.GetLocalized(x => x.MetaTitle, language.Id),
                SeName = entity.GetSeName(language.Id),
                TopicTemplateId = entity.TopicTemplateId,
                Published = entity.Published
            };
            return model;

        }

        //forum
        public static ForumRowModel ToModel(this Forum forum)
        {
            var forumModel = new ForumRowModel {
                Id = forum.Id,
                Name = forum.Name,
                SeName = forum.GetSeName(),
                Description = forum.Description,
                NumTopics = forum.NumTopics,
                NumPosts = forum.NumPosts,
                LastPostId = forum.LastPostId,
            };
            return forumModel;
        }

        public static Address ToEntity(this AddressModel model, bool trimFields = true)
        {
            if (model == null)
                return null;

            var entity = new Address();
            return ToEntity(model, entity, trimFields);
        }

        public static Address ToEntity(this AddressModel model, Address destination, bool trimFields = true)
        {
            if (model == null)
                return destination;

            if (trimFields)
            {
                if (model.FirstName != null)
                    model.FirstName = model.FirstName.Trim();
                if (model.LastName != null)
                    model.LastName = model.LastName.Trim();
                if (model.Email != null)
                    model.Email = model.Email.Trim();
                if (model.Company != null)
                    model.Company = model.Company.Trim();
                if (model.VatNumber != null)
                    model.VatNumber = model.VatNumber.Trim();
                if (model.City != null)
                    model.City = model.City.Trim();
                if (model.Address1 != null)
                    model.Address1 = model.Address1.Trim();
                if (model.Address2 != null)
                    model.Address2 = model.Address2.Trim();
                if (model.ZipPostalCode != null)
                    model.ZipPostalCode = model.ZipPostalCode.Trim();
                if (model.PhoneNumber != null)
                    model.PhoneNumber = model.PhoneNumber.Trim();
                if (model.FaxNumber != null)
                    model.FaxNumber = model.FaxNumber.Trim();
            }
            destination.FirstName = model.FirstName;
            destination.LastName = model.LastName;
            destination.Email = model.Email;
            destination.Company = model.Company;
            destination.VatNumber = model.VatNumber;
            destination.CountryId = !String.IsNullOrEmpty(model.CountryId) ? model.CountryId : "";
            destination.StateProvinceId = !String.IsNullOrEmpty(model.StateProvinceId) ? model.StateProvinceId : "";
            destination.City = model.City;
            destination.Address1 = model.Address1;
            destination.Address2 = model.Address2;
            destination.ZipPostalCode = model.ZipPostalCode;
            destination.PhoneNumber = model.PhoneNumber;
            destination.FaxNumber = model.FaxNumber;
            
            return destination;
        }

        public static Address ToEntity(this VendorAddressModel model, Address destination, bool trimFields = true)
        {
            if (model == null)
                return destination;

            if (trimFields)
            {
                if (model.Company != null)
                    model.Company = model.Company.Trim();
                if (model.City != null)
                    model.City = model.City.Trim();
                if (model.Address1 != null)
                    model.Address1 = model.Address1.Trim();
                if (model.Address2 != null)
                    model.Address2 = model.Address2.Trim();
                if (model.ZipPostalCode != null)
                    model.ZipPostalCode = model.ZipPostalCode.Trim();
                if (model.PhoneNumber != null)
                    model.PhoneNumber = model.PhoneNumber.Trim();
                if (model.FaxNumber != null)
                    model.FaxNumber = model.FaxNumber.Trim();
            }
            destination.Company = model.Company;
            destination.CountryId = !String.IsNullOrEmpty(model.CountryId) ? model.CountryId : "";
            destination.StateProvinceId = !String.IsNullOrEmpty(model.StateProvinceId) ? model.StateProvinceId : "";
            destination.City = model.City;
            destination.Address1 = model.Address1;
            destination.Address2 = model.Address2;
            destination.ZipPostalCode = model.ZipPostalCode;
            destination.PhoneNumber = model.PhoneNumber;
            destination.FaxNumber = model.FaxNumber;

            return destination;
        }

        public static void ParseReservationDates(this Product product, IFormCollection form,
            out DateTime? startDate, out DateTime? endDate)
        {
            startDate = null;
            endDate = null;

            string startControlId = string.Format("reservationDatepickerFrom_{0}", product.Id);
            string endControlId = string.Format("reservationDatepickerTo_{0}", product.Id);
            var ctrlStartDate = form[startControlId];
            var ctrlEndDate = form[endControlId];
            try
            {
                //currenly we support only this format (as in the \Views\Product\_RentalInfo.cshtml file)
                const string datePickerFormat = "MM/dd/yyyy";
                startDate = DateTime.ParseExact(ctrlStartDate, datePickerFormat, CultureInfo.InvariantCulture);
                endDate = DateTime.ParseExact(ctrlEndDate, datePickerFormat, CultureInfo.InvariantCulture);
            }
            catch
            {
            }
        }
    }
}