using Grand.Domain;
using Grand.Domain.Data;
using Grand.Domain.Vendors;
using Grand.Services.Events;
using MediatR;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Vendors
{
    /// <summary>
    /// Vendor service
    /// </summary>
    public partial class VendorService : IVendorService
    {
        #region Fields

        private readonly IRepository<Vendor> _vendorRepository;
        private readonly IRepository<VendorReview> _vendorReviewRepository;
        private readonly IMediator _mediator;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="vendorRepository">Vendor repository</param>
        /// <param name="mediator">Mediator</param>
        public VendorService(IRepository<Vendor> vendorRepository, IRepository<VendorReview> vendorReviewRepository,
            IMediator mediator)
        {
            _vendorRepository = vendorRepository;
            _vendorReviewRepository = vendorReviewRepository;
            _mediator = mediator;
        }

        #endregion

        #region Methods
        
        /// <summary>
        /// Gets a vendor by vendor identifier
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <returns>Vendor</returns>
        public virtual Task<Vendor> GetVendorById(string vendorId)
        {
            return _vendorRepository.GetByIdAsync(vendorId);
        }

        /// <summary>
        /// Delete a vendor
        /// </summary>
        /// <param name="vendor">Vendor</param>
        public virtual async Task DeleteVendor(Vendor vendor)
        {
            if (vendor == null)
                throw new ArgumentNullException("vendor");

            vendor.Deleted = true;
            await UpdateVendor(vendor);
        }

        /// <summary>
        /// Gets all vendors
        /// </summary>
        /// <param name="name">Vendor name</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Vendors</returns>
        public virtual async Task<IPagedList<Vendor>> GetAllVendors(string name = "",
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            var query = _vendorRepository.Table;

            if (!String.IsNullOrWhiteSpace(name))
                query = query.Where(v => v.Name.ToLower().Contains(name.ToLower()));
            if (!showHidden)
                query = query.Where(v => v.Active);
            query = query.Where(v => !v.Deleted);
            query = query.OrderBy(v => v.DisplayOrder).ThenBy(v => v.Name);
            return await PagedList<Vendor>.Create(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Inserts a vendor
        /// </summary>
        /// <param name="vendor">Vendor</param>
        public virtual async Task InsertVendor(Vendor vendor)
        {
            if (vendor == null)
                throw new ArgumentNullException("vendor");

            await _vendorRepository.InsertAsync(vendor);

            //event notification
            await _mediator.EntityInserted(vendor);
        }

        /// <summary>
        /// Updates the vendor
        /// </summary>
        /// <param name="vendor">Vendor</param>
        public virtual async Task UpdateVendor(Vendor vendor)
        {
            if (vendor == null)
                throw new ArgumentNullException("vendor");

            await _vendorRepository.UpdateAsync(vendor);

            //event notification
            await _mediator.EntityUpdated(vendor);
        }


        /// <summary>
        /// Gets a vendor note note
        /// </summary>
        /// <param name="vendorNoteId">The vendor note identifier</param>
        /// <returns>Vendor note</returns>
        public virtual async Task<VendorNote> GetVendorNoteById(string vendorId, string vendorNoteId)
        {
            if (String.IsNullOrEmpty(vendorNoteId))
                return null;
            var vendor =  await _vendorRepository.GetByIdAsync(vendorId);
            if(vendor == null)
                return null;

            return vendor.VendorNotes.FirstOrDefault(x => x.Id == vendorNoteId);
        }

        public virtual async Task InsertVendorNote(VendorNote vendorNote)
        {
            if (vendorNote == null)
                throw new ArgumentNullException("vendorNote");

            var updatebuilder = Builders<Vendor>.Update;
            var update = updatebuilder.AddToSet(p => p.VendorNotes, vendorNote);
            await _vendorRepository.Collection.UpdateOneAsync(new BsonDocument("_id", vendorNote.VendorId), update);

            //event notification
            await _mediator.EntityInserted(vendorNote);
        }

        /// <summary>
        /// Deletes a vendor note
        /// </summary>
        /// <param name="vendorNote">The vendor note</param>
        public virtual async Task DeleteVendorNote(VendorNote vendorNote)
        {
            if (vendorNote == null)
                throw new ArgumentNullException("vendorNote");

            var updatebuilder = Builders<Vendor>.Update;
            var update = updatebuilder.Pull(p => p.VendorNotes, vendorNote);
            await _vendorRepository.Collection.UpdateOneAsync(new BsonDocument("_id", vendorNote.VendorId), update);

            //event notification
            await _mediator.EntityDeleted(vendorNote);
        }

        /// <summary>
        /// Gets a vendor mapping 
        /// </summary>
        /// <param name="discountId">Discount id mapping identifier</param>
        /// <returns>vendor mapping</returns>
        public virtual async Task<IList<Vendor>> GetAllVendorsByDiscount(string discountId)
        {
            var query = from c in _vendorRepository.Table
                        where c.AppliedDiscounts.Any(x => x == discountId)
                        select c;
            return await query.ToListAsync();
        }

        #region Vendor reviews

        /// <summary>
        /// Gets all vendor reviews
        /// </summary>
        /// <param name="customerId">Customer identifier; "" to load all records</param>
        /// <param name="approved">A value indicating whether to content is approved; null to load all records</param> 
        /// <param name="fromUtc">Item creation from; null to load all records</param>
        /// <param name="toUtc">Item item creation to; null to load all records</param>
        /// <param name="message">Search title or review text; null to load all records</param>
        /// <returns>Reviews</returns>
        public virtual async Task<IPagedList<VendorReview>> GetAllVendorReviews(string customerId, bool? approved,
            DateTime? fromUtc = null, DateTime? toUtc = null,
            string message = null, string vendorId = "", int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from p in _vendorReviewRepository.Table
                        select p;

            if (approved.HasValue)
                query = query.Where(c => c.IsApproved == approved.Value);
            if (!String.IsNullOrEmpty(customerId))
                query = query.Where(c => c.CustomerId == customerId);
            if (fromUtc.HasValue)
                query = query.Where(c => fromUtc.Value <= c.CreatedOnUtc);
            if (toUtc.HasValue)
                query = query.Where(c => toUtc.Value >= c.CreatedOnUtc);
            if (!String.IsNullOrEmpty(message))
                query = query.Where(c => c.Title.Contains(message) || c.ReviewText.Contains(message));
            if (!String.IsNullOrEmpty(vendorId))
                query = query.Where(c => c.VendorId == vendorId);
            query = query.OrderByDescending(c => c.CreatedOnUtc);
            return await PagedList<VendorReview>.Create(query, pageIndex, pageSize);
        }

        public virtual async Task<int> RatingSumVendor(string vendorId, string storeId)
        {
            var query = from p in _vendorReviewRepository.Table
                        where p.VendorId == vendorId && p.IsApproved
                        group p by true into g
                        select new { Sum = g.Sum(x => x.Rating) };
            var content = await query.ToListAsync();
            return content.Count > 0 ? content.FirstOrDefault().Sum : 0;
        }

        public virtual async Task<int> TotalReviewsVendor(string vendorId, string storeId)
        {
            var query = from p in _vendorReviewRepository.Table
                        where p.VendorId == vendorId && p.IsApproved 
                        group p by true into g
                        select new { Count = g.Count() };
            var content = await query.ToListAsync();
            return content.Count > 0 ? content.FirstOrDefault().Count : 0;
        }


        /// <summary>
        /// Update vendor review totals
        /// </summary>
        /// <param name="vendor">Vendor</param>
        public virtual async Task UpdateVendorReviewTotals(Vendor vendor)
        {
            if (vendor == null)
                throw new ArgumentNullException("vendor");

            int approvedRatingSum = 0;
            int notApprovedRatingSum = 0;
            int approvedTotalReviews = 0;
            int notApprovedTotalReviews = 0;
            var reviews = _vendorReviewRepository.Table.Where(x => x.VendorId == vendor.Id);
            foreach (var pr in reviews)
            {
                if (pr.IsApproved)
                {
                    approvedRatingSum += pr.Rating;
                    approvedTotalReviews++;
                }
                else
                {
                    notApprovedRatingSum += pr.Rating;
                    notApprovedTotalReviews++;
                }
            }

            vendor.ApprovedRatingSum = approvedRatingSum;
            vendor.NotApprovedRatingSum = notApprovedRatingSum;
            vendor.ApprovedTotalReviews = approvedTotalReviews;
            vendor.NotApprovedTotalReviews = notApprovedTotalReviews;

            var filter = Builders<Vendor>.Filter.Eq("Id", vendor.Id);
            var update = Builders<Vendor>.Update
                    .Set(x => x.ApprovedRatingSum, vendor.ApprovedRatingSum)
                    .Set(x => x.NotApprovedRatingSum, vendor.NotApprovedRatingSum)
                    .Set(x => x.ApprovedTotalReviews, vendor.ApprovedTotalReviews)
                    .Set(x => x.NotApprovedTotalReviews, vendor.NotApprovedTotalReviews);

            await _vendorRepository.Collection.UpdateOneAsync(filter, update);
           
            //event notification
            await _mediator.EntityUpdated(vendor);
        }

        public virtual async Task UpdateVendorReview(VendorReview vendorreview)
        {
            if (vendorreview == null)
                throw new ArgumentNullException("vendorreview");

            var builder = Builders<VendorReview>.Filter;
            var filter = builder.Eq(x => x.Id, vendorreview.Id);
            var update = Builders<VendorReview>.Update
                .Set(x => x.Title, vendorreview.Title)
                .Set(x => x.ReviewText, vendorreview.ReviewText)
                .Set(x => x.IsApproved, vendorreview.IsApproved)
                .Set(x => x.HelpfulYesTotal, vendorreview.HelpfulYesTotal)
                .Set(x => x.HelpfulNoTotal, vendorreview.HelpfulNoTotal);

            await _vendorReviewRepository.Collection.UpdateManyAsync(filter, update);

            //event notification
            await _mediator.EntityUpdated(vendorreview);
        }

        /// <summary>
        /// Inserts a vendor review
        /// </summary>
        /// <param name="vendorPicture">Vendor picture</param>
        public virtual async Task InsertVendorReview(VendorReview vendorReview)
        {
            if (vendorReview == null)
                throw new ArgumentNullException("vendorPicture");

            await _vendorReviewRepository.InsertAsync(vendorReview);

            //event notification
            await _mediator.EntityInserted(vendorReview);
        }

        /// <summary>
        /// Deletes a vendor review
        /// </summary>
        /// <param name="vendorReview">Vendor review</param>
        public virtual async Task DeleteVendorReview(VendorReview vendorReview)
        {
            if (vendorReview == null)
                throw new ArgumentNullException("vendorReview");

            await _vendorReviewRepository.DeleteAsync(vendorReview);

            //event notification
            await _mediator.EntityDeleted(vendorReview);
        }

        /// <summary>
        /// Gets vendor review
        /// </summary>
        /// <param name="vendorReviewId">Vendor review identifier</param>
        /// <returns>Vendor review</returns>
        public virtual Task<VendorReview> GetVendorReviewById(string vendorReviewId)
        {
            return _vendorReviewRepository.GetByIdAsync(vendorReviewId);
        }


        /// <summary>
        /// Search vendors
        /// </summary>
        /// <param name="vendorId">Vendor identifier; "" to load all records</param>
        /// <param name="keywords">Keywords</param>
        /// <returns>Vendors</returns>
        public virtual async Task<IList<Vendor>> SearchVendors(
            string vendorId = "",
            string keywords = null)
        {
            //vendors
            var builder = Builders<Vendor>.Filter;
            var filter = FilterDefinition<Vendor>.Empty;

            //searching by keyword
            if (!string.IsNullOrWhiteSpace(keywords))
            {
                filter &= builder.Where(p =>
                        p.Name.ToLower().Contains(keywords.ToLower())
                        ||
                        p.Locales.Any(x => x.LocaleKey == "Name" && x.LocaleValue != null && x.LocaleValue.ToLower().Contains(keywords.ToLower()))
                        );
            }

            //vendor filtering
            if (!string.IsNullOrEmpty(vendorId))
            {
                filter &= builder.Where(x => x.Id == vendorId);
            }
            return await _vendorRepository.Collection.Find(filter).ToListAsync();
        }

        #endregion

        #endregion
    }
}