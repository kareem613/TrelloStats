﻿using System;
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
        string TrelloToken { get; }
        int WeeksToSkipForVelocityCalculation { get; }
    }
}