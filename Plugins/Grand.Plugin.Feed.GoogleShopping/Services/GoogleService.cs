using Grand.Core.Data;
using Grand.Plugin.Feed.GoogleShopping.Domain;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            this._gpRepository = gpRepository;
        }

        #endregion

        #region Utilties

        private string GetEmbeddedFileContent(string resourceName)
        {
            string fullResourceName = string.Format("Grand.Plugin.Feed.GoogleShopping.Files.{0}", resourceName);
            var assem = this.GetType().Assembly;
            using (var stream = assem.GetManifestResourceStream(fullResourceName))
            using (var reader = new StreamReader(stream))
                return reader.ReadToEnd();
        }

        #endregion

        #region Methods

        public virtual void DeleteGoogleProduct(GoogleProductRecord googleProductRecord)
        {
            if (googleProductRecord == null)
                throw new ArgumentNullException("googleProductRecord");

            _gpRepository.Delete(googleProductRecord);
        }

        public virtual IList<GoogleProductRecord> GetAll()
        {
            var query = from gp in _gpRepository.Table
                        orderby gp.Id
                        select gp;
            var records = query.ToList();
            return records;
        }

        public virtual GoogleProductRecord GetById(string googleProductRecordId)
        {
            return _gpRepository.GetById(googleProductRecordId);
        }

        public virtual GoogleProductRecord GetByProductId(string productId)
        {
            var query = from gp in _gpRepository.Table
                        where gp.ProductId == productId
                        orderby gp.Id
                        select gp;
            var record = query.FirstOrDefault();
            return record;
        }

        public virtual void InsertGoogleProductRecord(GoogleProductRecord googleProductRecord)
        {
            if (googleProductRecord == null)
                throw new ArgumentNullException("googleProductRecord");

            _gpRepository.Insert(googleProductRecord);
        }

        public virtual void UpdateGoogleProductRecord(GoogleProductRecord googleProductRecord)
        {
            if (googleProductRecord == null)
                throw new ArgumentNullException("googleProductRecord");

            _gpRepository.Update(googleProductRecord);
        }

        public virtual IList<string> GetTaxonomyList()
        {
            var fileContent = GetEmbeddedFileContent("taxonomy.txt");
            if (String.IsNullOrEmpty((fileContent)))
                return new List<string>();

            //parse the file
            var result = fileContent.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            return result;
        }

        #endregion
    }
}
