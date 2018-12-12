using Grand.Core;
using Grand.Core.Domain.Customers;

namespace Grand.Services.Customers
{
    public partial interface IUserApiService
    {
        /// <summary>
        /// Get user api by id
        /// </summary>
        /// <param name="id">id</param>
        UserApi GetUserById(string id);

        /// <summary>
        /// Get user api by email
        /// </summary>
        /// <param name="id">id</param>
        UserApi GetUserByEmail(string email);

        /// <summary>
        /// Insert user api
        /// </summary>
        /// <param name="userApi">User api</param>
        void InsertUserApi(UserApi userApi);

        /// <summary>
        /// Update user api
        /// </summary>
        /// <param name="userApi">User api</param>
        void UpdateUserApi(UserApi userApi);

        /// <summary>
        /// Delete user api
        /// </summary>
        /// <param name="userApi">User api</param>
        void DeleteUserApi(UserApi userApi);

        /// <summary>
        /// Get users api
        /// </summary>
        /// <param name="email"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns>PagedList<UserApi></returns>
        IPagedList<UserApi> GetUsers(string email = "", int pageIndex = 0, int pageSize = 2147483647);
    }
}
