using Grand.Core.Configuration;
using Grand.Core.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Grand.Core
{
    /// <summary>
    /// Represents a common helper
    /// </summary>
    public partial class WebHelper : IWebHelper
    {
        #region Fields 

        private const string NullIpAddress = "::1";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HostingConfig _hostingConfig;
        private readonly IHostApplicationLifetime _applicationLifetime;

        #endregion

        #region Constructor

        /// <summary>
        /// Ctor
        /// </summary>        
        public WebHelper(
            IHttpContextAccessor httpContextAccessor,
            HostingConfig hostingConfig,
            IHostApplicationLifetime applicationLifetime
            )
        {
            _hostingConfig = hostingConfig;
            _httpContextAccessor = httpContextAccessor;
            _applicationLifetime = applicationLifetime;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Check whether current HTTP request is available
        /// </summary>
        /// <returns>True if available; otherwise false</returns>
        protected virtual bool IsRequestAvailable()
        {
            if (_httpContextAccessor == null || _httpContextAccessor.HttpContext == null)
                return false;

            try
            {
                if (_httpContextAccessor.HttpContext.Request == null)
                    return false;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        protected virtual bool IsIpAddressSet(IPAddress address)
        {
            return address != null && address.ToString() != NullIpAddress;
        }


        #endregion

        #region Methods

        /// <summary>
        /// Get URL referrer
        /// </summary>
        /// <returns>URL referrer</returns>
        public virtual string GetUrlReferrer()
        {
            if (!IsRequestAvailable())
                return string.Empty;

            //URL referrer is null in some case (for example, in IE 8)
            return _httpContextAccessor.HttpContext.Request.Headers[HeaderNames.Referer];
        }

        /// <summary>
        /// Get context IP address
        /// </summary>
        /// <returns>URL referrer</returns>
        public virtual string GetCurrentIpAddress()
        {
            if (!IsRequestAvailable())
                return string.Empty;

            var result = string.Empty;
            try
            {
                //first try to get IP address from the forwarded header
                if (_httpContextAccessor.HttpContext.Request.Headers != null)
                {
                    //the X-Forwarded-For (XFF) HTTP header field is a de facto standard for identifying the originating IP address of a client
                    //connecting to a web server through an HTTP proxy or load balancer
                    var forwardedHttpHeaderKey = "X-FORWARDED-FOR";
                    if (!string.IsNullOrEmpty(_hostingConfig.ForwardedHttpHeader))
                    {
                        //but in some cases server use other HTTP header
                        //in these cases an administrator can specify a custom Forwarded HTTP header (e.g. CF-Connecting-IP, X-FORWARDED-PROTO, etc)
                        forwardedHttpHeaderKey = _hostingConfig.ForwardedHttpHeader;
                    }

                    var forwardedHeader = _httpContextAccessor.HttpContext.Request.Headers[forwardedHttpHeaderKey];
                    if (!StringValues.IsNullOrEmpty(forwardedHeader))
                        result = forwardedHeader.FirstOrDefault();
                }

                //if this header not exists try get connection remote IP address
                if (string.IsNullOrEmpty(result) && _httpContextAccessor.HttpContext.Connection.RemoteIpAddress != null)
                    result = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            }
            catch { return string.Empty; }

            //some of the validation
            if (result != null && result.Equals("::1", StringComparison.OrdinalIgnoreCase))
                result = "127.0.0.1";

            //"TryParse" doesn't support IPv4 with port number
            if (IPAddress.TryParse(result ?? string.Empty, out IPAddress ip))
                //IP address is valid 
                result = ip.ToString();
            else if (!string.IsNullOrEmpty(result))
                //remove port
                result = result.Split(':').FirstOrDefault();

            return result;
        }


        /// <summary>
        /// Gets this page name
        /// </summary>
        /// <param name="includeQueryString">Value indicating whether to include query strings</param>
        /// <returns>Page name</returns>
        public virtual string GetThisPageUrl(bool includeQueryString)
        {
            bool useSsl = IsCurrentConnectionSecured();
            return GetThisPageUrl(includeQueryString, useSsl);
        }

        /// <summary>
        /// Gets this page name
        /// </summary>
        /// <param name="includeQueryString">Value indicating whether to include query strings</param>
        /// <param name="useSsl">Value indicating whether to get SSL protected page</param>
        /// <returns>Page name</returns>
        public virtual string GetThisPageUrl(bool includeQueryString, bool useSsl)
        {
            if (!IsRequestAvailable())
                return string.Empty;

            //get the host considering using SSL
            var url = GetStoreHost(useSsl).TrimEnd('/');

            //get full URL with or without query string
            url += includeQueryString ? GetRawUrl(_httpContextAccessor.HttpContext.Request)
                : $"{_httpContextAccessor.HttpContext.Request.PathBase}{_httpContextAccessor.HttpContext.Request.Path}";

            return url.ToLowerInvariant();
        }

        /// <summary>
        /// Gets a value indicating whether current connection is secured
        /// </summary>
        /// <returns>true - secured, false - not secured</returns>
        public virtual bool IsCurrentConnectionSecured()
        {
            if (!IsRequestAvailable())
                return false;

            //check whether hosting uses a load balancer
            //use HTTP_CLUSTER_HTTPS?
            if (_hostingConfig.UseHttpClusterHttps)
                return _httpContextAccessor.HttpContext.Request.Headers["HTTP_CLUSTER_HTTPS"].ToString().Equals("on", StringComparison.OrdinalIgnoreCase);

            //use HTTP_X_FORWARDED_PROTO?
            if (_hostingConfig.UseHttpXForwardedProto)
                return _httpContextAccessor.HttpContext.Request.Headers["X-Forwarded-Proto"].ToString().Equals("https", StringComparison.OrdinalIgnoreCase);

            return _httpContextAccessor.HttpContext.Request.IsHttps;
        }

        /// <summary>
        /// Gets store host location
        /// </summary>
        /// <param name="useSsl">Whether to get SSL secured URL</param>
        /// <returns>Store host location</returns>
        public virtual string GetStoreHost(bool useSsl)
        {
            if (!IsRequestAvailable())
                return string.Empty;

            //try to get host from the request HOST header
            var hostHeader = _httpContextAccessor.HttpContext.Request.Headers[HeaderNames.Host];
            if (StringValues.IsNullOrEmpty(hostHeader))
                return string.Empty;

            //add scheme to the URL
            var storeHost = $"{(useSsl ? Uri.UriSchemeHttps : Uri.UriSchemeHttp)}://{hostHeader.FirstOrDefault()}";

            //ensure that host is ended with slash
            storeHost = $"{storeHost.TrimEnd('/')}/";

            return storeHost;
        }

        /// <summary>
        /// Gets store location
        /// </summary>
        /// <param name="useSsl">Whether to get SSL secured URL; pass null to determine automatically</param>
        /// <returns>Store location</returns>
        public virtual string GetStoreLocation(bool? useSsl = null)
        {
            var storeLocation = string.Empty;

            //get store host
            var storeHost = GetStoreHost(useSsl ?? IsCurrentConnectionSecured());
            if (!string.IsNullOrEmpty(storeHost))
            {
                //add application path base if exists
                storeLocation = IsRequestAvailable() ? $"{storeHost.TrimEnd('/')}{_httpContextAccessor.HttpContext.Request.PathBase}" : storeHost;
            }

            //if host is empty (it is possible only when HttpContext is not available), use URL of a store entity configured in admin area
            if (string.IsNullOrEmpty(storeHost) && DataSettingsHelper.DatabaseIsInstalled())
            {
                var currentStore = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IStoreContext>().CurrentStore;
                if (currentStore != null)
                    storeLocation = !currentStore.SslEnabled ? currentStore.Url : currentStore.SecureUrl;
                else
                    throw new Exception("Current store cannot be loaded");
            }

            //ensure that URL is ended with slash
            storeLocation = $"{storeLocation.TrimEnd('/')}/";

            return storeLocation;
        }

        /// <summary>
        /// Returns true if the requested resource is one of the typical resources that needn't be processed by the cms engine.
        /// </summary>
        /// <returns>True if the request targets a static resource file.</returns>
        public virtual bool IsStaticResource()
        {
            if (!IsRequestAvailable())
                return false;

            string path = _httpContextAccessor.HttpContext.Request.Path;

            //a little workaround. FileExtensionContentTypeProvider contains most of static file extensions. So we can use it
            //source: https://github.com/aspnet/StaticFiles/blob/dev/src/Microsoft.AspNetCore.StaticFiles/FileExtensionContentTypeProvider.cs
            //if it can return content type, then it's a static file
            var contentTypeProvider = new FileExtensionContentTypeProvider();
            return contentTypeProvider.TryGetContentType(path, out string _);
        }

        /// <summary>
        /// Modifies query string
        /// </summary>
        /// <param name="url">Url to modify</param>
        /// <param name="key">Query parameter key to add/remove</param>
        /// <param name="value">Query parameter values to add</param>
        /// <returns>New url</returns>
        public virtual string ModifyQueryString(string url, string key, string value)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            if (string.IsNullOrEmpty(key))
                return url;

            var uri = new Uri(url);
            var baseUri = uri.GetComponents(UriComponents.Scheme | UriComponents.Host | UriComponents.Port | UriComponents.Path, UriFormat.UriEscaped);

            var query = QueryHelpers.ParseQuery(uri.Query);

            var items = query.SelectMany(x => x.Value, (col, val) =>
                new KeyValuePair<string, string>(col.Key, val)).ToList();

            items.RemoveAll(x => x.Key == key);

            var qb = new QueryBuilder(items);

            if (!string.IsNullOrEmpty(value))
                qb.Add(key, value);

            var returnUrl = baseUri + qb.ToQueryString();
            return returnUrl;
        }

        /// <summary>
        /// Gets query string value by name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">Parameter name</param>
        /// <returns>Query string value</returns>
        public virtual T QueryString<T>(string name)
        {
            if (!IsRequestAvailable())
                return default(T);

            if (StringValues.IsNullOrEmpty(_httpContextAccessor.HttpContext.Request.Query[name]))
                return default(T);

            return CommonHelper.To<T>(_httpContextAccessor.HttpContext.Request.Query[name].ToString());
        }

        /// <summary>
        /// Restart application domain
        /// </summary>
        public virtual void RestartAppDomain()
        {
            _applicationLifetime.StopApplication();
        }

        /// <summary>
        /// Gets a value that indicates whether the client is being redirected to a new location
        /// </summary>
        public virtual bool IsRequestBeingRedirected {
            get {
                var response = _httpContextAccessor.HttpContext.Response;
                int[] redirectionStatusCodes = { 301, 302 };
                return redirectionStatusCodes.Contains(response.StatusCode);
            }
        }


        /// <summary>
        /// Gets or sets a value that indicates whether the client is being redirected to a new location using POST
        /// </summary>
        public virtual bool IsPostBeingDone {
            get {
                if (_httpContextAccessor.HttpContext.Items["grand.IsPOSTBeingDone"] == null)
                    return false;

                return Convert.ToBoolean(_httpContextAccessor.HttpContext.Items["grand.IsPOSTBeingDone"]);
            }
            set {
                _httpContextAccessor.HttpContext.Items["grand.IsPOSTBeingDone"] = value;
            }
        }

        /// <summary>
        /// Gets whether the specified http request uri references the local host.
        /// </summary>
        /// <param name="req">Http request</param>
        /// <returns>True, if http request uri references to the local host</returns>
        public virtual bool IsLocalRequest(HttpRequest req)
        {
            //source: https://stackoverflow.com/a/41242493/7860424
            var connection = req.HttpContext.Connection;
            if (IsIpAddressSet(connection.RemoteIpAddress))
            {
                //We have a remote address set up
                return IsIpAddressSet(connection.LocalIpAddress)
                    //Is local is same as remote, then we are local
                    ? connection.RemoteIpAddress.Equals(connection.LocalIpAddress)
                    //else we are remote if the remote IP address is not a loopback address
                    : IPAddress.IsLoopback(connection.RemoteIpAddress);
            }

            return true;
        }

        /// <summary>
        /// Get the raw path and full query of request
        /// </summary>
        /// <param name="request">Http request</param>
        /// <returns>Raw URL</returns>
        public virtual string GetRawUrl(HttpRequest request)
        {
            //first try to get the raw target from request feature
            //note: value has not been UrlDecoded
            var rawUrl = request.HttpContext.Features.Get<IHttpRequestFeature>()?.RawTarget;

            //or compose raw URL manually
            if (string.IsNullOrEmpty(rawUrl))
                rawUrl = $"{request.PathBase}{request.Path}{request.QueryString}";

            return rawUrl;
        }

        #endregion
    }
}