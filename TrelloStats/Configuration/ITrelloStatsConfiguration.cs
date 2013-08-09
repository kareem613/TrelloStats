using System;
using System.Collections.Generic;
namespace TrelloStats.Configuration
{
    public interface ITrelloStatsConfiguration
    {
        string GmailEmailAddress { get; }
        string GmailPassword { get; }
        string GoogleSpreadsheetName { get; }
        string[] LabelNames { get; }
        ListNameConfiguration ListNames { get; set; }
        string TimelineJsDefaultTag { get; }
        int TimelineJsOffsetMinutesPerCard { get; }
        string[] TimelineJsTags { get; }
        TimeZoneInfo TimeZone { get; }
        string TrelloBoard { get; }
        string TrelloKey { get; }
        double TrelloProjectionsEstimateWindowLowerBoundFactor { get; }
        double TrelloProjectionsEstimateWindowUpperBoundFactor { get; }
        List<KeyValuePair<string, DateTime>> ProjectionMilestones { get; }
        string TrelloToken { get; }
        int WeeksToSkipForVelocityCalculation { get; }
        string WebsiteJsonFilename { get; }
        string WebsiteHtmlFilename { get; }
        string WebsiteOutputFolder { get; }
        string SummaryTextTemplate { get; }
    }
}
