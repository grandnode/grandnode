using Grand.Web.Framework.Kendoui;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Grand.Web.Framework
{
    public class ErrorJsonResult: JsonResult
    {
        public DataSourceResult DataResult;
        public ErrorJsonResult(DataSourceResult dataSourceResult)
        {
            this.DataResult = dataSourceResult;
        }
        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            var response = context.HttpContext.Response;
            response.ContentType = !String.IsNullOrEmpty(ContentType) ? ContentType : "application/json";
            if (ContentEncoding != null)
                response.ContentEncoding = ContentEncoding;

            response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
            Data = DataResult;
            var serializedObject = JsonConvert.SerializeObject(Data, Formatting.Indented);
            response.Write(serializedObject);
        }

    }
}
