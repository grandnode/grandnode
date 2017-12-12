using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Grand.Core;
using Grand.Core.Domain.Stores;
using Grand.Services.Stores;

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
            this._httpContextAccessor = httpContextAccessor;
            this._storeService = storeService;
        }

        /// <summary>
        /// Gets or sets the current store
        /// </summary>
        public virtual Store CurrentStore
        {
            get
            {
                if (_cachedStore != null)
                    return _cachedStore;

                //try to determine the current store by HOST header
                string host = _httpContextAccessor.HttpContext?.Request?.Headers[HeaderNames.Host];

                var allStores = _storeService.GetAllStores();
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

                _cachedStore = _cachedStore ?? throw new Exception("No store could be loaded");

                return _cachedStore;
            }
            set
            {
                SetStoreCookie(value.Id);
                _cachedStore = value;
            }

        }

        /// <summary>
        /// Set store cookie
        /// </summary>
        /// <param name="customerGuid">Guid of the customer</param>
        protected virtual void SetStoreCookie(string storeId)
        {
            if (_httpContextAccessor.HttpContext == null || _httpContextAccessor.HttpContext.Response == null)
                return;

            var store = _storeService.GetStoreById(storeId);
            if (store == null)
                return;

            //delete current cookie value
            _httpContextAccessor.HttpContext.Response.Cookies.Delete(STORE_COOKIE_NAME);

            //get date of cookie expiration
            var cookieExpires = 24 * 365; //TODO make configurable
            var cookieExpiresDate = DateTime.Now.AddHours(cookieExpires);

            //set new cookie value
            var options = new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = true,
                Expires = cookieExpiresDate
            };
            _httpContextAccessor.HttpContext.Response.Cookies.Append(STORE_COOKIE_NAME, storeId, options);
        }


    }
}
