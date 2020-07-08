using Grand.Domain;
using Grand.Domain.Customers;
using System.Threading.Tasks;

namespace Grand.Services.Customers
{
    public partial interface IUserApiService
    {
        /// <summary>
        /// Get user api by id
        /// </summary>
        /// <param name="id">id</param>
        Task<UserApi> GetUserById(string id);

        /// <summary>
        /// Get user api by email
        /// </summary>
        /// <param name="id">id</param>
        Task<UserApi> GetUserByEmail(string email);

        /// <summary>
        /// Insert user api
        /// </summary>
        /// <param name="userApi">User api</param>
        Task InsertUserApi(UserApi userApi);

        /// <summary>
        /// Update user api
        /// </summary>
        /// <param name="userApi">User api</param>
        Task UpdateUserApi(UserApi userApi);

        /// <summary>
        /// Delete user api
        /// </summary>
        /// <param name="userApi">User api</param>
        Task DeleteUserApi(UserApi userApi);

        /// <summary>
        /// Get users api
        /// </summary>
        /// <param name="email"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns>PagedList<UserApi></returns>
        Task<IPagedList<UserApi>> GetUsers(string email = "", int pageIndex = 0, int pageSize = 2147483647);
    }
}
