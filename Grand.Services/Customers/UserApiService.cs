using Grand.Core;
using Grand.Core.Data;
using Grand.Core.Domain.Customers;
using Grand.Services.Events;
using MongoDB.Driver.Linq;
using System.Linq;

namespace Grand.Services.Customers
{
    public partial class UserApiService: IUserApiService
    {
        #region Fields

        private readonly IRepository<UserApi> _userRepository;
        private readonly IEventPublisher _eventPublisher;

        #endregion
        public UserApiService(IRepository<UserApi> userRepository, IEventPublisher eventPublisher)
        {
            this._userRepository = userRepository;
            this._eventPublisher = eventPublisher;
        }
        
        /// <summary>
        /// Get user api by id
        /// </summary>
        /// <param name="id">id</param>
        public virtual UserApi GetUserById(string id)
        {
            return _userRepository.GetById(id);
        }

        /// <summary>
        /// Get user api by email
        /// </summary>
        /// <param name="id">id</param>
        public virtual UserApi GetUserByEmail(string email)
        {
            return _userRepository.Table.Where(x => x.Email == email.ToLowerInvariant()).FirstOrDefault();
        }

        /// <summary>
        /// Insert user api
        /// </summary>
        /// <param name="userApi">User api</param>
        public virtual void InsertUserApi(UserApi userApi)
        {
            _userRepository.Insert(userApi);

            //event notification
            _eventPublisher.EntityInserted(userApi);
        }

        /// <summary>
        /// Update user api
        /// </summary>
        /// <param name="userApi">User api</param>
        public virtual void UpdateUserApi(UserApi userApi)
        {
            _userRepository.Update(userApi);

            //event notification
            _eventPublisher.EntityUpdated(userApi);
        }

        /// <summary>
        /// Delete user api
        /// </summary>
        /// <param name="userApi">User api</param>
        public virtual void DeleteUserApi(UserApi userApi)
        {
            _userRepository.Delete(userApi);
        }

        /// <summary>
        /// Get users api
        /// </summary>
        /// <param name="email"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns>PagedList<UserApi></returns>
        public virtual IPagedList<UserApi> GetUsers(string email = "", int pageIndex = 0, int pageSize = 2147483647)
        {
            var query = _userRepository.Table;
            if (!string.IsNullOrEmpty(email))
                query = query.Where(x => x.Email.Contains(email.ToLowerInvariant()));

            var users = new PagedList<UserApi>(query, pageIndex, pageSize);
            return users;
        }
    }
}
