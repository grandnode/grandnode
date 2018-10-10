using Google.Apis.AnalyticsReporting.v4;
using Google.Apis.AnalyticsReporting.v4.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Grand.Core.Domain.Seo;
using Grand.Services.Localization;
using Grand.Services.Logging;
using System;
using System.Collections.Generic;

namespace Grand.Services.Configuration
{
    public partial class GoogleAnalyticsService: IGoogleAnalyticsService
    {
        #region Fields

        private readonly GoogleAnalyticsSettings _googleAnalyticsSettings;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;


        private AnalyticsReportingService _analyticsReportingService
        {
            get
            {
                try
                {
                    var scopes = new[] { AnalyticsReportingService.Scope.Analytics };
                    var temp = new ServiceAccountCredential.Initializer(_googleAnalyticsSettings.gaserviceAccountEmail) { Scopes = scopes };
                    string privateKey = _googleAnalyticsSettings.gaprivateKey.Replace("\\n", "\n");
                    var credential = new ServiceAccountCredential(temp.FromPrivateKey(privateKey));

                    return new AnalyticsReportingService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "GrandNode",
                    });
                }
                catch(Exception ex)
                {
                    _logger.InsertLog(Core.Domain.Logging.LogLevel.Error,"GoogleAnalytics", ex.Message);
                    return null;
                }
            }
        }

        #endregion

        #region Ctor
        public GoogleAnalyticsService(GoogleAnalyticsSettings googleAnalyticsSettings,
            ILocalizationService localizationService, ILogger logger)
        {
            this._googleAnalyticsSettings = googleAnalyticsSettings;
            this._localizationService = localizationService;
            this._logger = logger;
        }
        #endregion

        #region Utilities

        private GoogleAnalyticsResult ParseResponse(GetReportsResponse response, DateTime startDate, DateTime endDate)
        {
            var result = new GoogleAnalyticsResult();

            result.StartDate = startDate.ToString();
            result.EndDate = endDate.ToString();

            foreach (var report in response.Reports)
            {
                ColumnHeader header = report.ColumnHeader;
                IList<string> dimensionHeaders = header.Dimensions;
                IList<MetricHeaderEntry> metricHeaders = header.MetricHeader.MetricHeaderEntries;
                IList<ReportRow> rows = report.Data.Rows;
                if (rows == null)
                    rows = new List<ReportRow>();

                const int MAX_RECORDS = 10;
                int recordCount;
                if (rows.Count > 10) recordCount = MAX_RECORDS; //if needed, this code limits quantity of records up to 10
                else recordCount = rows.Count;

                //recordCount will determine how quantity to save into collection 
                for (int a = 0; a < recordCount; ++a)
                {
                    IList<string> dimensions = rows[a].Dimensions;
                    IList<DateRangeValues> metrics = rows[a].Metrics;

                    var newRecord = new Dictionary<string, string>();
                    for (int i = 0; i < dimensionHeaders.Count && i < dimensions.Count; ++i)
                    {
                        newRecord.Add(dimensionHeaders[i], dimensions[i]);
                    }
                    for (int v = 0; v < metricHeaders.Count; ++v)
                    {
                        newRecord.Add(metricHeaders[v].Name, metrics[0].Values[v]);
                    }
                    result.Records.Add(newRecord);
                }
            }
            return result;
        }

        private GoogleAnalyticsResult ReturnEmpty()
        {
            if (String.IsNullOrEmpty(_googleAnalyticsSettings.gaprivateKey))
                return new GoogleAnalyticsResult() { Message = _localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.GoogleAnalytics.help") };
            else
                return new GoogleAnalyticsResult() { Message = _localizationService.GetResource("Admin.Configuration.Settings.GeneralCommon.GoogleAnalytics.help2") };
        }

        #endregion

        public GoogleAnalyticsResult GetDataByGeneral(DateTime startDate, DateTime endDate)
        {
            if(_analyticsReportingService==null)
            {
                return ReturnEmpty();
            }

            // metrics and dimensions below are tailored to suit current needings
            //in case of needing to use different or more metrics or dimensions, see link below
            //https://developers.google.com/analytics/devguides/reporting/core/dimsmets 
            //it would be nice just to pass metrics and dimensions to this method, but it will surely incerase complexity 
            //and get rid of tidy and neat "fire-n-get" method that requires simple arguments

            
            //DateRange
            IList<DateRange> dateRanges = new List<DateRange>();
            dateRanges.Add(new DateRange() { StartDate = startDate.ToString("yyyy-MM-dd"), EndDate = endDate.ToString("yyyy-MM-dd") });

            //Metric
            IList<Metric> metrics = new List<Metric>();
            metrics.Add(new Metric() { Expression = "ga:users", Alias = "Users" });
            metrics.Add(new Metric() { Expression = "ga:newUsers", Alias = "Unique users" });
            metrics.Add(new Metric() { Expression = "ga:pageviews", Alias = "Views" });

            //Dimension
            IList<Dimension> dimensions = new List<Dimension>();
            dimensions.Add(new Dimension() { Name = "ga:userType" });

            //OrderBy
            IList<OrderBy> orderBys = new List<OrderBy>();
            orderBys.Add(new OrderBy() { FieldName = "ga:pageviews", SortOrder = "DESCENDING", OrderType = "VALUE" });

            //final assembling
            ReportRequest request = new ReportRequest()
            {
                ViewId = _googleAnalyticsSettings.gaviewID,
                DateRanges = dateRanges,
                Metrics = metrics,
                Dimensions = dimensions,
                OrderBys = orderBys
            };

            List<ReportRequest> requests = new List<ReportRequest>();
            requests.Add(request);

            GetReportsRequest reportsRequest = new GetReportsRequest();
            reportsRequest.ReportRequests = requests;

            var response = _analyticsReportingService.Reports.BatchGet(reportsRequest).Execute();

            return ParseResponse(response, startDate, endDate);
        }
        public GoogleAnalyticsResult GetDataByLocalization(DateTime startDate, DateTime endDate)
        {
            if (_analyticsReportingService == null)
            {
                return ReturnEmpty();
            }

            IList<DateRange> dateRanges = new List<DateRange>();
            dateRanges.Add(new DateRange() { StartDate = startDate.ToString("yyyy-MM-dd"), EndDate = endDate.ToString("yyyy-MM-dd") });

            //Metric
            IList<Metric> metrics = new List<Metric>();
            metrics.Add(new Metric() { Expression = "ga:users", Alias = "Users" });
            metrics.Add(new Metric() { Expression = "ga:newUsers", Alias = "Unique users" });
            metrics.Add(new Metric() { Expression = "ga:pageviews", Alias = "Views" });

            //Dimension
            IList<Dimension> dimensions = new List<Dimension>();
            dimensions.Add(new Dimension() { Name = "ga:country" });
            dimensions.Add(new Dimension() { Name = "ga:city" });

            //OrderBy
            IList<OrderBy> orderBys = new List<OrderBy>();
            orderBys.Add(new OrderBy() { FieldName = "ga:pageviews", SortOrder = "DESCENDING", OrderType = "VALUE" });

            //ReportRequest - final assembling
            ReportRequest request = new ReportRequest()
            {
                ViewId = _googleAnalyticsSettings.gaviewID,
                DateRanges = dateRanges,
                Metrics = metrics,
                Dimensions = dimensions,
                OrderBys = orderBys,
            };

            List<ReportRequest> requests = new List<ReportRequest>();
            requests.Add(request);

            GetReportsRequest reportsRequest = new GetReportsRequest();
            reportsRequest.ReportRequests = requests;

            var response = _analyticsReportingService.Reports.BatchGet(reportsRequest).Execute();

            return ParseResponse(response, startDate, endDate);

        }
        public GoogleAnalyticsResult GetDataBySource(DateTime startDate, DateTime endDate)
        {
            if (_analyticsReportingService == null)
            {
                return ReturnEmpty();
            }

            IList<DateRange> dateRanges = new List<DateRange>();
            dateRanges.Add(new DateRange() { StartDate = startDate.ToString("yyyy-MM-dd"), EndDate = endDate.ToString("yyyy-MM-dd") });

            //Metric
            IList<Metric> metrics = new List<Metric>();
            metrics.Add(new Metric() { Expression = "ga:users", Alias = "Users" });
            metrics.Add(new Metric() { Expression = "ga:newUsers", Alias = "Unique users" });
            metrics.Add(new Metric() { Expression = "ga:pageviews", Alias = "Views" });

            //Dimension
            IList<Dimension> dimensions = new List<Dimension>();
            dimensions.Add(new Dimension() { Name = "ga:sourceMedium" });

            //OrderBy
            IList<OrderBy> orderBys = new List<OrderBy>();
            orderBys.Add(new OrderBy() { FieldName = "ga:pageviews", SortOrder = "DESCENDING", OrderType = "VALUE" });

            //final assembling
            ReportRequest request = new ReportRequest()
            {
                ViewId = _googleAnalyticsSettings.gaviewID,
                DateRanges = dateRanges,
                Metrics = metrics,
                Dimensions = dimensions,
                OrderBys = orderBys
            };

            List<ReportRequest> requests = new List<ReportRequest>();
            requests.Add(request);

            GetReportsRequest reportsRequest = new GetReportsRequest();
            reportsRequest.ReportRequests = requests;

            var response = _analyticsReportingService.Reports.BatchGet(reportsRequest).Execute();

            return ParseResponse(response, startDate, endDate);

        }
        public GoogleAnalyticsResult GetDataByExit(DateTime startDate, DateTime endDate)
        {
            if (_analyticsReportingService == null)
            {
                return ReturnEmpty();
            }

            IList<DateRange> dateRanges = new List<DateRange>();
            dateRanges.Add(new DateRange() { StartDate = startDate.ToString("yyyy-MM-dd"), EndDate = endDate.ToString("yyyy-MM-dd") });

            //Metric
            IList<Metric> metrics = new List<Metric>();
            metrics.Add(new Metric() { Expression = "ga:users", Alias = "Users" });
            metrics.Add(new Metric() { Expression = "ga:newUsers", Alias = "Unique users" });
            metrics.Add(new Metric() { Expression = "ga:pageviews", Alias = "Views" });

            //Dimension
            IList<Dimension> dimensions = new List<Dimension>();
            dimensions.Add(new Dimension() { Name = "ga:exitPagePath" });

            //OrderBy
            IList<OrderBy> orderBys = new List<OrderBy>();
            orderBys.Add(new OrderBy() { FieldName = "ga:pageviews", SortOrder = "DESCENDING", OrderType = "VALUE" });

            //final assembling
            ReportRequest request = new ReportRequest()
            {
                ViewId = _googleAnalyticsSettings.gaviewID,
                DateRanges = dateRanges,
                Metrics = metrics,
                Dimensions = dimensions,
                OrderBys = orderBys
            };

            List<ReportRequest> requests = new List<ReportRequest>();
            requests.Add(request);

            GetReportsRequest reportsRequest = new GetReportsRequest();
            reportsRequest.ReportRequests = requests;

            var response = _analyticsReportingService.Reports.BatchGet(reportsRequest).Execute();

            return ParseResponse(response, startDate, endDate);

        }
        public GoogleAnalyticsResult GetDataByDevice(DateTime startDate, DateTime endDate)
        {
            if (_analyticsReportingService == null)
            {
                return ReturnEmpty();
            }
            IList<DateRange> dateRanges = new List<DateRange>();
            dateRanges.Add(new DateRange() { StartDate = startDate.ToString("yyyy-MM-dd"), EndDate = endDate.ToString("yyyy-MM-dd") });

            //Metric
            IList<Metric> metrics = new List<Metric>();
            metrics.Add(new Metric() { Expression = "ga:users", Alias = "Users" });
            metrics.Add(new Metric() { Expression = "ga:newUsers", Alias = "Unique users" });
            metrics.Add(new Metric() { Expression = "ga:pageviews", Alias = "Views" });

            //Dimension
            IList<Dimension> dimensions = new List<Dimension>();
            dimensions.Add(new Dimension() { Name = "ga:browser" });
            dimensions.Add(new Dimension() { Name = "ga:mobileDeviceInfo" });

            //OrderBy
            IList<OrderBy> orderBys = new List<OrderBy>();
            orderBys.Add(new OrderBy() { FieldName = "ga:pageviews", SortOrder = "DESCENDING", OrderType = "VALUE" });

            //final assembling
            ReportRequest request = new ReportRequest()
            {
                ViewId = _googleAnalyticsSettings.gaviewID,
                DateRanges = dateRanges,
                Metrics = metrics,
                Dimensions = dimensions,
                OrderBys = orderBys
            };

            List<ReportRequest> requests = new List<ReportRequest>();
            requests.Add(request);

            GetReportsRequest reportsRequest = new GetReportsRequest();
            reportsRequest.ReportRequests = requests;

            var response = _analyticsReportingService.Reports.BatchGet(reportsRequest).Execute();

            return ParseResponse(response, startDate, endDate);
            

        }
    }
}
