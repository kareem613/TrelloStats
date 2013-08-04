using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace TrelloStats
{
    public class TrelloStatsConfiguration : TypeConfigurationManager
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

        public string SummaryTextTemplate = @"
Work started on <strong>{0}</strong> with the most recent of <strong>{1}</strong> stories completed on <strong>{2}</strong>. Total points completed is <strong>{5}</strong>.<br/>
[[projections_summary]]
<br/> Timeline last updated {3} {4}.
<style>
.stats th {{font-weight:bold;
background:#f4f9fe;
text-align:center;
color:#66a3d3;
padding: 1px 5px;
}}
#week_stats {{
width:100%;
border-top:1px solid #e5eff8;
border-right:1px solid #e5eff8;
border-collapse:collapse;
}}
.stats .date {{
white-space:nowrap;
}}
.stats td {{
color:#678197;
border-bottom:1px solid #e5eff8;
border-left:1px solid #e5eff8;
padding:.3em 1em;
text-align:center;
}}
#week_stats tr:last-child td {{ 
background:#f4f9fe !important
color:#66a3d3 !important;
font-weight: bold !important;
}}
#list_stats {{
margin-bottom:5px;
}}
</style>
<div>
[[extra_lists_stats_table]]
</div>
<table id=""week_stats"" class=""stats"">
<tbody>
    [[weekly_stats_header]]
    [[weekly_stats_rows]]
<tbody>
</table>
";
    }
}
