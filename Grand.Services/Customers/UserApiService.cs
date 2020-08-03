using Grand.Domain;
using Grand.Domain.Data;
using Grand.Domain.Customers;
using Grand.Services.Events;
using MediatR;
using MongoDB.Driver.Linq;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Services.Customers
{
    public partial class UserApiService: IUserApiService
    {
        #region Fields

        private readonly IRepository<UserApi> _userRepository;
        private readonly IMediator _mediator;

        #endregion
        public UserApiService(IRepository<UserApi> userRepository, IMediator mediator)
        {
            _userRepository = userRepository;
            _mediator = mediator;
        }
        
        /// <summary>
        /// Get user api by id
        /// </summary>
        /// <param name="id">id</param>
        public virtual Task<UserApi> GetUserById(string id)
        {
            return _userRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Get user api by email
        /// </summary>
        /// <param name="id">id</param>
        public virtual Task<UserApi> GetUserByEmail(string email)
        {
            return _userRepository.Table.Where(x => x.Email == email.ToLowerInvariant()).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Insert user api
        /// </summary>
        /// <param name="userApi">User api</param>
        public virtual async Task InsertUserApi(UserApi userApi)
        {
            await _userRepository.InsertAsync(userApi);

            //event notification
            await _mediator.EntityInserted(userApi);
        }

        /// <summary>
        /// Update user api
        /// </summary>
        /// <param name="userApi">User api</param>
        public virtual async Task UpdateUserApi(UserApi userApi)
        {
            await _userRepository.UpdateAsync(userApi);

            //event notification
            await _mediator.EntityUpdated(userApi);
        }

        /// <summary>
        /// Delete user api
        /// </summary>
        /// <param name="userApi">User api</param>
        public virtual async Task DeleteUserApi(UserApi userApi)
        {
            await _userRepository.DeleteAsync(userApi);

            //event notification
            await _mediator.EntityDeleted(userApi);

        }

        /// <summary>
        /// Get users api
        /// </summary>
        /// <param name="email"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns>PagedList<UserApi></returns>
        public virtual async Task<IPagedList<UserApi>> GetUsers(string email = "", int pageIndex = 0, int pageSize = 2147483647)
        {
            var query = _userRepository.Table;
            if (!string.IsNullOrEmpty(email))
                query = query.Where(x => x.Email.Contains(email.ToLowerInvariant()));

            return await PagedList<UserApi>.Create(query, pageIndex, pageSize);
        }
    }
}
