using Grand.Domain.Data;
using Grand.Plugin.Feed.GoogleShopping.Domain;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Plugin.Feed.GoogleShopping.Services
{
    public partial class GoogleService : IGoogleService
    {
        #region Fields

        private readonly IRepository<GoogleProductRecord> _gpRepository;

        #endregion

        #region Ctor

        public GoogleService(IRepository<GoogleProductRecord> gpRepository)
        {
            _gpRepository = gpRepository;
        }

        #endregion

        #region Utilties

        private string GetEmbeddedFileContent(string resourceName)
        {
            string fullResourceName = string.Format("Grand.Plugin.Feed.GoogleShopping.Files.{0}", resourceName);
            var assem = GetType().Assembly;
            using (var stream = assem.GetManifestResourceStream(fullResourceName))
            using (var reader = new StreamReader(stream))
                return reader.ReadToEnd();
        }

        #endregion

        #region Methods

        public virtual async Task DeleteGoogleProduct(GoogleProductRecord googleProductRecord)
        {
            if (googleProductRecord == null)
                throw new ArgumentNullException("googleProductRecord");

            await _gpRepository.DeleteAsync(googleProductRecord);
        }

        public virtual async Task<IList<GoogleProductRecord>> GetAll()
        {
            var query = from gp in _gpRepository.Table
                        orderby gp.Id
                        select gp;
            return await query.ToListAsync();
        }

        public virtual Task<GoogleProductRecord> GetById(string googleProductRecordId)
        {
            return _gpRepository.GetByIdAsync(googleProductRecordId);
        }

        public virtual async Task<GoogleProductRecord> GetByProductId(string productId)
        {
            var query = from gp in _gpRepository.Table
                        where gp.ProductId == productId
                        orderby gp.Id
                        select gp;
            return await query.FirstOrDefaultAsync();
        }

        public virtual async Task InsertGoogleProductRecord(GoogleProductRecord googleProductRecord)
        {
            if (googleProductRecord == null)
                throw new ArgumentNullException("googleProductRecord");

            await _gpRepository.InsertAsync(googleProductRecord);
        }

        public virtual async Task UpdateGoogleProductRecord(GoogleProductRecord googleProductRecord)
        {
            if (googleProductRecord == null)
                throw new ArgumentNullException("googleProductRecord");

            await _gpRepository.UpdateAsync(googleProductRecord);
        }

        public virtual async Task<IList<string>> GetTaxonomyList()
        {
            var fileContent = GetEmbeddedFileContent("taxonomy.txt");
            if (String.IsNullOrEmpty((fileContent)))
                return new List<string>();

            //parse the file
            var result = fileContent.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            return await Task.FromResult(result);
        }

        #endregion
    }
}
