using Grand.Core;
using Grand.Domain.Stores;
using Grand.Services.Stores;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Framework
{
    /// <summary>
    /// Store context for web application
    /// </summary>
    public partial class WebStoreContext : IStoreContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IStoreService _storeService;
        private Store _cachedStore;

        #region Const
        private const string STORE_COOKIE_NAME = ".Grand.Store";
        #endregion


        protected virtual string GetStoreCookie()
        {
            if (_httpContextAccessor.HttpContext == null || _httpContextAccessor.HttpContext.Request == null)
                return null;

            return _httpContextAccessor.HttpContext.Request.Cookies[STORE_COOKIE_NAME];
        }



        public WebStoreContext(IHttpContextAccessor httpContextAccessor, IStoreService storeService)
        {
            _httpContextAccessor = httpContextAccessor;
            _storeService = storeService;
        }

        /// <summary>
        /// Gets or sets the current store
        /// </summary>
        public virtual Store CurrentStore
        {
            get
            {
                return _cachedStore;
            }

        }

        /// <summary>
        /// Set the current store by Middleware
        /// </summary>
        /// <returns></returns>
        public virtual async Task<Store> SetCurrentStore()
        {

            //try to determine the current store by HOST header
            string host = _httpContextAccessor.HttpContext?.Request?.Headers[HeaderNames.Host];

            var allStores = await _storeService.GetAllStores();
            var stores = allStores.Where(s => s.ContainsHostValue(host));
            if (stores.Count() == 0)
            {
                _cachedStore = allStores.FirstOrDefault();
            }
            else if (stores.Count() == 1)
            {
                _cachedStore = stores.FirstOrDefault();
            }
            else if (stores.Count() > 1)
            {
                var cookie = GetStoreCookie();
                if (!string.IsNullOrEmpty(cookie))
                {
                    var storecookie = stores.FirstOrDefault(x => x.Id == cookie);
                    if (storecookie != null)
                        _cachedStore = storecookie;
                    else
                        _cachedStore = stores.FirstOrDefault();
                }
                else
                    _cachedStore = stores.FirstOrDefault();
            }
            return _cachedStore ?? throw new Exception("No store could be loaded");
        }
        /// <summary>
        /// Set store cookie
        /// </summary>
        /// <param name="storeId">Store ident</param>
        public virtual async Task SetStoreCookie(string storeId)
        {
            if (_httpContextAccessor.HttpContext == null || _httpContextAccessor.HttpContext.Response == null)
                return;

            var store = await _storeService.GetStoreById(storeId);
            if (store == null)
                return;

            //delete current cookie value
            _httpContextAccessor.HttpContext.Response.Cookies.Delete(STORE_COOKIE_NAME);

            //get date of cookie expiration
            var cookieExpiresDate = DateTime.UtcNow.AddHours(CommonHelper.CookieAuthExpires);

            //set new cookie value
            var options = new CookieOptions
            {
                HttpOnly = true,
                Expires = cookieExpiresDate
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append(STORE_COOKIE_NAME, storeId, options);
        }


    }
}
