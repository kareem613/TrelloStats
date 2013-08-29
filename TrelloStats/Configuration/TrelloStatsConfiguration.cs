using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace TrelloStats.Configuration
{
    public class TrelloStatsConfiguration : TypeConfigurationManager, ITrelloStatsConfiguration
    {
        
        public ListNameConfiguration ListNames { get; set; }
        public TrelloStatsConfiguration()
        {
            ListNames = new ListNameConfiguration();
        }

        public string[] TimelineJsTags
        {
            get
            {
                return ConfigurationManager.AppSettings["TimelineJS.Tags"].Split(',');
            }
        }

        public string TimelineJsDefaultTag
        {
            get
            {
                return ConfigurationManager.AppSettings["TimelineJS.Tags.Default"];
            }
        }

        public int TimelineJsOffsetMinutesPerCard
        {
            get
            {
                return GetAppConfigInt("TimelineJS.OffsetMinutesPerCard", 0);
            }
        }

        public string[] LabelNames
        {
            get
            {
                return GetAppSettingAsArray("Trello.Labels");
            }
        }

        public string GmailEmailAddress{
            get
            {
                return GetAppConfig("Gmail.EmailAddress");
            }
        }

        public string GmailPassword{
            get
            {
                return GetAppConfig("Gmail.OneTimePassword");
            }
        }

        public string GoogleSpreadsheetName{
            get
            {
                return GetAppConfig("Google.SpreadsheetName");
            }
        }

        public TimeZoneInfo TimeZone
        {
            get
            {
                return TimeZoneInfo.FindSystemTimeZoneById(GetAppConfig("TimeZone"));
            }
        }

        public string TrelloKey
        {
            get
            {
                return GetAppConfig("Trello.Key");
            }
        }

        public string TrelloToken
        {
            get
            {
                return GetAppConfig("Trello.Token");
            }
        }

        public string TrelloBoard
        {
            get
            {
                return GetAppConfig("Trello.Board");
            }
        }

        public double TrelloProjectionsEstimateWindowLowerBoundFactor
        {
            get
            {
                return GetAppConfigDouble("Trello.Projections.EstimateWindowLowerBoundFactor", 1);
            }
        }

        public double TrelloProjectionsEstimateWindowUpperBoundFactor
        {
            get
            {
                return GetAppConfigDouble("Trello.Projections.EstimateWindowUpperBoundFactor", 1);
            }
        }

        public int WeeksToSkipForVelocityCalculation
        {
            get
            {
                return GetAppConfigInt("Trello.Projections.WeeksToSkipForVelocityCalculation", 0);
            }
        }

        public string WebsiteJsonFilename
        {
            get
            {
                return GetAppConfig("WebsiteJsonFilename");
            }
        }
        public string WebsiteHtmlFilename
        {
            get
            {
                return GetAppConfig("WebsiteHtmlFilename");
            }
        }
        public string WebsiteOutputFolder
        {
            get
            {
                return GetAppConfig("WebsiteOutputFolder");
            }
        }

        public string GoogleTimesheetsSpreadsheetName
        {
            get
            {
                return GetAppConfig("Google.Timesheets.SpreadsheetName");
            }
        }

        public string SummaryTextTemplate
        {
            get
            {
               return @"
Work started on <strong>{0}</strong> with the most recent of <strong>{1}</strong> stories completed on <strong>{2}</strong>. Total points completed is <strong>{5}</strong>.<br/>
[[projections_summary]]
<br/> Timeline last updated {3} {4}.";
            }
        }

//        public string SummaryTextTemplate
//        {
//            get
//            {
//                return @"
//Work started on <strong>{0}</strong> with the most recent of <strong>{1}</strong> stories completed on <strong>{2}</strong>. Total points completed is <strong>{5}</strong>.<br/>
//[[projections_summary]]
//<br/> Timeline last updated {3} {4}.
//<div class=""row"">
//    <div class=""col-lg-2"">
//        [[extra_lists_stats_table]]
//    </div>
//    <div class=""col-lg-10"">
//        <table id=""week_stats"" class=""table-condensed table-bordered table-striped"">
//        <tbody>
//            [[weekly_stats_header]]
//            [[weekly_stats_rows]]
//        <tbody>
//        </table>
//    </div>
//</div>
//";
//            }
//        }







        public string GoogleTimesheetsExcludeCategoryFromTotalHours
        {
            get
            {
                return GetAppConfig("Google.Timesheets.ExcludeCategoryFromTotalHours");
            }
        }
    }
}
