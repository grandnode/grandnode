﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;
using System;
using System.Diagnostics;
using System.Linq;

namespace Grand.Framework.Controllers
{
    /// <summary>
    /// Attribute to validate whether a certain form name (or value) was submitted
    /// </summary>
    public class FormValueRequiredAttribute : ActionMethodSelectorAttribute
    {
        private readonly string[] _submitButtonNames;
        private readonly FormValueRequirement _requirement;
        private readonly bool _validateNameOnly;

        public FormValueRequiredAttribute(params string[] submitButtonNames):
            this(FormValueRequirement.Equal, submitButtonNames)
        {
        }
        public FormValueRequiredAttribute(FormValueRequirement requirement, params string[] submitButtonNames):
            this(requirement, true, submitButtonNames)
        {
        }
        public FormValueRequiredAttribute(FormValueRequirement requirement, bool validateNameOnly, params string[] submitButtonNames)
        {
            //at least one submit button should be found
            _submitButtonNames = submitButtonNames;
            _validateNameOnly = validateNameOnly;
            _requirement = requirement;
        }


        public override bool IsValidForRequest(RouteContext routeContext, ActionDescriptor action)
        {
            if (!HttpMethods.IsPost(routeContext.HttpContext.Request.Method))
                return false;

            foreach (string buttonName in _submitButtonNames)
            {
                try
                {
                    switch (this._requirement)
                    {
                        case FormValueRequirement.Equal:
                            {
                                if (_validateNameOnly)
                                {
                                    //"name" only
                                    if (routeContext.HttpContext.Request.Form.Keys.Any(x => x.Equals(buttonName, StringComparison.OrdinalIgnoreCase)))
                                        return true;
                                }
                                else
                                {
                                    //validate "value"
                                    //do not iterate because "Invalid request" exception can be thrown
                                    string value = routeContext.HttpContext.Request.Form[buttonName];
                                    if (!String.IsNullOrEmpty(value))
                                        return true;
                                }
                            }
                            break;
                        case FormValueRequirement.StartsWith:
                            {
                                if (_validateNameOnly)
                                {
                                    //"name" only
                                    if (routeContext.HttpContext.Request.Form.Keys.Any(x => x.StartsWith(buttonName, StringComparison.OrdinalIgnoreCase)))
                                        return true;
                                }
                                else
                                {
                                    //validate "value"
                                    foreach (var formValue in routeContext.HttpContext.Request.Form.Keys)
                                        if (formValue.StartsWith(buttonName, StringComparison.OrdinalIgnoreCase))
                                        { 
                                            var value = routeContext.HttpContext.Request.Form[formValue];
                                            if (!String.IsNullOrEmpty(value))
                                                return true;
                                        }
                                }
                            }
                            break;
                    }
                }
                catch (Exception exc)
                {
                    //try-catch to ensure that no exception is throw
                    Debug.WriteLine(exc.Message);
                }
            }
            return false;
        }
    }

    public enum FormValueRequirement
    {
        Equal,
        StartsWith
    }
}