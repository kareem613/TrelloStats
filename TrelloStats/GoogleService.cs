using System;
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
width:80%;
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
    <tr><th>Week #</th><th>Start</th><th>End</th><th>Still In Progress</th><th>Stories Completed</th><th>Unestimated Stories Completed</th><th>Fusebill Stories Completed</th><th>Points Completed</th></tr>
    [[weekly_stats_rows]]
<tbody>
</table>
";
        private const string WeekStatsRowTemplate = @"<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td><td>{5}</td><td>{6}</td><td>{7}</td></tr>";
        
        public GoogleService(string gmailAddress, string password)
        {
            _service = new SpreadsheetsService("trelloStats");
            _service.setUserCredentials(gmailAddress, password);
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

            foreach (var cardStat in boardStats.CardStats.Where(c=> !c.IsInProgress))
            {
                var row = new ListEntry();
                row.Elements.Add(new ListEntry.Custom() { LocalName = "startdate", Value = cardStat.EffectiveStartAction.Date.ToString() });
                row.Elements.Add(new ListEntry.Custom() { LocalName = "enddate", Value = "" });
                row.Elements.Add(new ListEntry.Custom() { LocalName = "headline", Value = GetHeadlineForCard(cardStat) });
                row.Elements.Add(new ListEntry.Custom() { LocalName = "text", Value = String.Format("{0} Elapsed Day(s)", cardStat.BusinessDaysElapsed) });
                row.Elements.Add(new ListEntry.Custom() { LocalName = "media", Value = cardStat.Card.Url });
                _service.Insert(listFeed, row);
            }

            if (boardStats.BadCardStats.Count > 0)
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
            boardStats.BadCardStats.ForEach(c => errorCards.AppendFormat("<div><a href=\"{0}\">{1}</a></div>", c.Card.Url, c.Card.Name));
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
            var weekStatsList = boardStats.GetWeeklyStats();
            var weekRows = new StringBuilder();
            weekStatsList.ForEach(w => weekRows.AppendFormat(WeekStatsRowTemplate,w.WeekNumber,w.StartDate.ToShortDateString(), w.EndDate.ToShortDateString(),w.NumberOfCardsInProgress, w.NumberOfCompletedCards, w.NumberOfUnestimatedCards,w.GetNumberOfCardsWithLabel("Fusebill"), w.PointsCompleted));

            var summaryText = String.Format(SummaryTextTemplate, 
                    boardStats.FirstStartDate.ToLongDateString(),
                    boardStats.NumberOfCompletedCards,
                    boardStats.LastDoneDate.ToLongDateString(),
                    boardStats.CreatedDate.ToLongDateString(),
                    boardStats.CreatedDate.ToLongTimeString(),
                    boardStats.TotalPoints
                );
            summaryText = summaryText.Replace("[[weekly_stats_rows]]", weekRows.ToString());
            return summaryText;
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
