﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.GData.Client;
using Google.GData.Spreadsheets;
using TrelloStats.Model;

namespace TrelloStats
{
    public class GoogleService
    {
        private readonly SpreadsheetsService _service;
        private const string spreadsheetName = "TrinityTimeline";
        private const string SummaryTextTemplate = @"
Beginning <strong>{0}</strong> with the latest of <strong>{1}</strong> stories completed on <strong>{2}</strong> for a total of <strong>{5}</strong> points.
<br/> Timeline last updated {3} {4}.
<style>
#week_stats th {{font-weight:bold;
background:#f4f9fe;
text-align:center;
color:#66a3d3;
}}
#week_stats {{
width:100%;
white-space:nowrap;
border-top:1px solid #e5eff8;
border-right:1px solid #e5eff8;
border-collapse:collapse;
}}
#week_stats td {{
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

</style>
<table id=""week_stats"">
<tbody>
    [[weekly_stats_header]]
    [[weekly_stats_rows]]
<tbody>
</table>
";
        private const string WeekStatsRowTemplate = @"<td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td><td>{6}</td><td>{7}</td><td>{8}</td>";

        private string[] LabelNames { get; set; }
        public GoogleService(string gmailAddress, string password, string[] labelNames)
        {
            _service = new SpreadsheetsService("trelloStats");
            _service.setUserCredentials(gmailAddress, password);

            LabelNames = labelNames;
        }

        public void PushToGoogleSpreadsheet(BoardStats boardStats)
        {
            SpreadsheetQuery query = new SpreadsheetQuery();
            query.Title = spreadsheetName;
            SpreadsheetFeed feed = _service.Query(query);

            if (feed.Entries.Count != 1)
                throw new Exception("Did not find exactly 1 shiftmylist datasource.");

            WorksheetEntry timelineWorksheet = GetWorksheet(feed, "Data");
            var listFeed = GetListFeed(timelineWorksheet);

            foreach (var row in listFeed.Entries)
            {
                row.Delete();
            }

            var titleRow = new ListEntry();
            titleRow.Elements.Add(new ListEntry.Custom() { LocalName = "startdate", Value = boardStats.FirstStartDate.ToString() });
            titleRow.Elements.Add(new ListEntry.Custom() { LocalName = "enddate", Value = "" });
            titleRow.Elements.Add(new ListEntry.Custom() { LocalName = "headline", Value = "Trinity Timeline" });
            titleRow.Elements.Add(new ListEntry.Custom() { LocalName = "text", Value = GetSummaryTextForBoardStat(boardStats) });
            titleRow.Elements.Add(new ListEntry.Custom() { LocalName = "type", Value = "title" });
            _service.Insert(listFeed, titleRow);

            foreach (var cardStat in boardStats.CompletedCardStats)
            {
                var row = new ListEntry();
                row.Elements.Add(new ListEntry.Custom() { LocalName = "startdate", Value = cardStat.EffectiveStartAction.Date.ToString() });
                row.Elements.Add(new ListEntry.Custom() { LocalName = "enddate", Value = "" });
                row.Elements.Add(new ListEntry.Custom() { LocalName = "headline", Value = GetHeadlineForCard(cardStat) });
                row.Elements.Add(new ListEntry.Custom() { LocalName = "text", Value = String.Format("{0} Elapsed Day(s)", cardStat.BusinessDaysElapsed) });
                row.Elements.Add(new ListEntry.Custom() { LocalName = "media", Value = cardStat.Card.Url });
                _service.Insert(listFeed, row);
            }

            if (boardStats.BoardData.BadCardStats.Count > 0)
            {
                var errorRow = new ListEntry();
                errorRow.Elements.Add(new ListEntry.Custom() { LocalName = "startdate", Value = boardStats.FirstStartDate.ToString() });
                errorRow.Elements.Add(new ListEntry.Custom() { LocalName = "enddate", Value = "" });
                errorRow.Elements.Add(new ListEntry.Custom() { LocalName = "headline", Value = "Unproccessed Trello Cards" });
                errorRow.Elements.Add(new ListEntry.Custom() { LocalName = "text", Value = GetSummaryTextForErrorCards(boardStats) });
                _service.Insert(listFeed, errorRow);
            }
        }
  
        private string GetSummaryTextForErrorCards(BoardStats boardStats)
        {
            var errorCards = new StringBuilder();
            boardStats.BoardData.BadCardStats.ForEach(c => errorCards.AppendFormat("<div><a href=\"{0}\">{1}</a></div>", c.Card.Url, c.Card.Name));
            return errorCards.ToString();
        }
  
        private string GetHeadlineForCard(CardStats cardStat)
        {
            if(cardStat.Points > 0)
                return String.Format("<strong>{0}</strong> <span>({1}pts)</span>", cardStat.Card.Name, cardStat.Points);

            return String.Format("<strong>{0}</strong> (NE)", cardStat.Card.Name);
        }
  
        private string GetSummaryTextForBoardStat(BoardStats boardStats)
        {
            var weekStatsHeader = GetWeekStatsHtmlHeader();

            var weekStatsList = boardStats.GetWeeklyStats();
            var weekRows = new StringBuilder();
            weekStatsList.ForEach(w => weekRows.Append(GetWeekStatsHtmlRow(w)));

            var summaryText = String.Format(SummaryTextTemplate, 
                    boardStats.FirstStartDate.ToLongDateString(),
                    boardStats.NumberOfCompletedCards,
                    boardStats.LastDoneDate.ToLongDateString(),
                    boardStats.BoardData.CreatedDate.ToLongDateString(),
                    boardStats.BoardData.CreatedDate.ToLongTimeString(),
                    boardStats.TotalPoints
                );

            summaryText = summaryText.Replace("[[weekly_stats_header]]", weekStatsHeader);

            summaryText = summaryText.Replace("[[weekly_stats_rows]]", weekRows.ToString());
            return summaryText;
        }

        private string GetWeekStatsHtmlHeader()
        {
            var headerTitles = new List<string>(){"Week #","Start","End","Still In Progress","Stories Completed","Points Completed"};
            foreach (var labelName in LabelNames)
            {
                headerTitles.Insert(headerTitles.Count - 1, labelName);
            }
            var header = new StringBuilder("<tr>");
            headerTitles.ForEach(h => header.AppendFormat("<th>{0}</th>", h));
            header.AppendLine("</tr>");
            return header.ToString();
        }
  
        private string GetWeekStatsHtmlRow(WeekStats w)
        {
            var row = new StringBuilder("<tr>");
            row.AppendLine(GetWeekStatsRow(w.WeekNumber));

            row.AppendLine(GetWeekStatsRow(w.StartDate.ToShortDateString()));
            row.AppendLine(GetWeekStatsRow(w.EndDate.ToShortDateString()));
            row.AppendLine(GetWeekStatsRow(w.NumberOfCardsInProgress));
            row.AppendLine(GetWeekStatsRow(w.NumberOfCompletedCards));

            foreach (var labelName in LabelNames)
            {
                row.AppendLine(GetWeekStatsRow(w.GetNumberOfCardsWithLabel(labelName)));
            }
           
            row.AppendLine(GetWeekStatsRow(w.PointsCompleted));
            

            row.Append("</tr>");

            return row.ToString();
        }
  
        private string GetWeekStatsRow(object value)
        {
            return string.Format("<td>{0}</td>", value.ToString());
        }

        private WorksheetEntry GetWorksheet(SpreadsheetFeed feed, string worksheetName)
        {
            var link = feed.Entries[0].Links.FindService(GDataSpreadsheetsNameTable.WorksheetRel, null);
            var worksheetQuery = new WorksheetQuery(link.HRef.ToString());
            worksheetQuery.Title = worksheetName;
            var worksheetFeed = _service.Query(worksheetQuery);

            if (worksheetFeed.Entries.Count != 1) throw new Exception(String.Format("Did not find exactly 1 {0} worksheet.", worksheetName));

            return (WorksheetEntry)worksheetFeed.Entries[0];
        }

        private ListFeed GetListFeed(WorksheetEntry worksheetEntry)
        {
            AtomLink listFeedLink = worksheetEntry.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);

            ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
            ListFeed listFeed = _service.Query(listQuery);
            return listFeed;
        }
    }
}
