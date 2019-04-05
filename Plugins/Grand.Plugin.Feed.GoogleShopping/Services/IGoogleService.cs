using Grand.Plugin.Feed.GoogleShopping.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grand.Plugin.Feed.GoogleShopping.Services
{
    public partial interface IGoogleService
    {
        Task DeleteGoogleProduct(GoogleProductRecord googleProductRecord);

        Task<IList<GoogleProductRecord>> GetAll();

        Task<GoogleProductRecord> GetById(string googleProductRecordId);

        Task<GoogleProductRecord> GetByProductId(string productId);

        Task InsertGoogleProductRecord(GoogleProductRecord googleProductRecord);

        Task UpdateGoogleProductRecord(GoogleProductRecord googleProductRecord);

        Task<IList<string>> GetTaxonomyList();
    }
}
