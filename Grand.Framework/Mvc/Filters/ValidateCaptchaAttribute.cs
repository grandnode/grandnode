using Grand.Framework.Security.Captcha;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using System.Linq;
using System.Threading.Tasks;

namespace Grand.Framework.Mvc.Filters
{
    /// <summary>
    /// Represents a filter attribute enabling CAPTCHA validation
    /// </summary>
    public class ValidateCaptchaAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// Create instance of the filter attribute 
        /// </summary>
        /// <param name="actionParameterName">The name of the action parameter to which the result will be passed</param>
        public ValidateCaptchaAttribute(string actionParameterName = "captchaValid") : base(typeof(ValidateCaptchaFilter))
        {
            this.Arguments = new object[] { actionParameterName };
        }

        #region Nested filter

        /// <summary>
        /// Represents a filter enabling CAPTCHA validation
        /// </summary>
        private class ValidateCaptchaFilter : IAsyncActionFilter
        {
            #region Constants

            private const string CHALLENGE_FIELD_KEY = "recaptcha_challenge_field";
            private const string RESPONSE_FIELD_KEY = "recaptcha_response_field";
            private const string G_RESPONSE_FIELD_KEY_V3 = "g-recaptcha-response-value";
            private const string G_RESPONSE_FIELD_KEY_V2 = "g-recaptcha-response";

            #endregion

            #region Fields

            private readonly string _actionParameterName;
            private readonly CaptchaSettings _captchaSettings;

            #endregion

            #region Ctor

            public ValidateCaptchaFilter(string actionParameterName, CaptchaSettings captchaSettings)
            {
                _actionParameterName = actionParameterName;
                _captchaSettings = captchaSettings;
            }

            #endregion

            #region Utilities

            /// <summary>
            /// Validate CAPTCHA
            /// </summary>
            /// <param name="context">A context for action filters</param>
            /// <returns>True if CAPTCHA is valid; otherwise false</returns>
            protected async Task<bool> ValidateCaptcha(ActionExecutingContext context)
            {
                var isValid = false;

                //get form values
                var form = await context.HttpContext.Request.ReadFormAsync();
                var captchaChallengeValue = form[CHALLENGE_FIELD_KEY];
                var captchaResponseValue = form[RESPONSE_FIELD_KEY];
                var gCaptchaResponseValue = string.Empty;
                foreach (var item in form.Keys)
                {
                    if (item.Contains(G_RESPONSE_FIELD_KEY_V3))
                        gCaptchaResponseValue = form[item];
                }

                if(string.IsNullOrEmpty(gCaptchaResponseValue))
                    gCaptchaResponseValue = form[G_RESPONSE_FIELD_KEY_V2];
                
                if ((!StringValues.IsNullOrEmpty(captchaChallengeValue) && !StringValues.IsNullOrEmpty(captchaResponseValue)) || !string.IsNullOrEmpty(gCaptchaResponseValue))
                {
                    //create CAPTCHA validator
                    var captchaValidtor = new GReCaptchaValidator(_captchaSettings.ReCaptchaVersion)
                    {
                        SecretKey = _captchaSettings.ReCaptchaPrivateKey,
                        RemoteIp = context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                        Response = !StringValues.IsNullOrEmpty(captchaResponseValue) ? captchaResponseValue.ToString() : gCaptchaResponseValue,
                        Challenge = captchaChallengeValue
                    };

                    //validate request
                    var recaptchaResponse = await captchaValidtor.Validate();
                    isValid = recaptchaResponse.IsValid;
                    if (!isValid)
                        foreach (var error in recaptchaResponse.ErrorCodes)
                        {
                            context.ModelState.AddModelError("", error);
                        }
                }

                return isValid;
            }

            #endregion

            #region Methods
            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                if (context == null || context.HttpContext == null || context.HttpContext.Request == null)
                {
                    await next();
                    return;
                }                

                //whether CAPTCHA is enabled
                if (_captchaSettings.Enabled && context.HttpContext != null && context.HttpContext.Request != null)
                {
                    //push the validation result as an action parameter
                    context.ActionArguments[_actionParameterName] = await ValidateCaptcha(context);
                }
                else
                    context.ActionArguments[_actionParameterName] = false;

                await next();

            }
            /// <summary>
            /// Called before the action executes, after model binding is complete
            /// </summary>
            /// <param name="context">A context for action filters</param>
            //public void OnActionExecuting(ActionExecutingContext context)
            //{
            //    if (context == null)
            //        return;

            //    //whether CAPTCHA is enabled
            //    if (_captchaSettings.Enabled && context.HttpContext != null && context.HttpContext.Request != null)
            //    {
            //        //push the validation result as an action parameter
            //        context.ActionArguments[_actionParameterName] = ValidateCaptcha(context);
            //    }
            //    else
            //        context.ActionArguments[_actionParameterName] = false;

            //}

            ///// <summary>
            ///// Called after the action executes, before the action result
            ///// </summary>
            ///// <param name="context">A context for action filters</param>
            //public void OnActionExecuted(ActionExecutedContext context)
            //{
            //    //do nothing
            //}

            #endregion
        }

        #endregion
    }
}