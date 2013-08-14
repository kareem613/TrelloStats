using System;
using System.Linq;
using System.Text;
using Google.GData.Spreadsheets;
using TrelloStats.Configuration;
using TrelloStats.Model.Stats;

namespace TrelloStats
{
    public class SpreadsheetEntryFactory
    {
        TrelloStatsConfiguration _configuration;
        HtmlFactory _htmlFactory;

        public SpreadsheetEntryFactory(TrelloStatsConfiguration configuration, HtmlFactory htmlFactory)
        {
            _configuration = configuration;
            _htmlFactory = htmlFactory;
        }

        public ListEntry GetBadCardEntry(BoardStatsAnalysis boardStatsAnalysis)
        {
            var errorRow = new ListEntry();
            errorRow.Elements.Add(new ListEntry.Custom() { LocalName = "startdate", Value = boardStatsAnalysis.FirstStartDate.ToString() });
            errorRow.Elements.Add(new ListEntry.Custom() { LocalName = "enddate", Value = "" });
            errorRow.Elements.Add(new ListEntry.Custom() { LocalName = "headline", Value = "Unproccessed Trello Cards" });
            errorRow.Elements.Add(new ListEntry.Custom() { LocalName = "text", Value = GetSummaryTextForErrorCards(boardStatsAnalysis) });
            return errorRow;
        }

        public ListEntry GetCompletedCardEntry(CardStats cardStat, TimeSpan timeOffset)
        {
            var row = new ListEntry();
            row.Elements.Add(new ListEntry.Custom() { LocalName = "startdate", Value = cardStat.DoneAction.DateInTimeZone(_configuration.TimeZone).Add(timeOffset).ToString() });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "enddate", Value = "" });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "headline", Value = GetHeadlineForCard(cardStat) });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "text", Value = String.Format("{0} Elapsed Day(s)", cardStat.BusinessDaysElapsed) });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "media", Value = cardStat.CardData.Card.Url });
            row.Elements.Add(new ListEntry.Custom() { LocalName = "tag", Value = GetCategory(cardStat) });

            return row;
        }

        private string GetCategory(CardStats cardStat)
        {
            var tags = _configuration.TimelineJsTags;
            foreach (var tag in tags)
            {
                if (cardStat.CardData.Card.Labels.Any(l => l.Name == tag))
                    return tag;
            }

            return _configuration.TimelineJsDefaultTag;
        }

        public ListEntry GetTitleCardEntry(BoardStatsAnalysis boardStatsAnalysis)
        {
            var titleRow = new ListEntry();
            titleRow.Elements.Add(new ListEntry.Custom() { LocalName = "startdate", Value = boardStatsAnalysis.FirstStartDate.ToString() });
            titleRow.Elements.Add(new ListEntry.Custom() { LocalName = "enddate", Value = "" });
            titleRow.Elements.Add(new ListEntry.Custom() { LocalName = "headline", Value = "Development Timeline" });
            titleRow.Elements.Add(new ListEntry.Custom() { LocalName = "text", Value = _htmlFactory.GetSummaryTextForBoardStat(boardStatsAnalysis) });
            titleRow.Elements.Add(new ListEntry.Custom() { LocalName = "type", Value = "title" });
            return titleRow;
        }

        private string GetSummaryTextForErrorCards(BoardStatsAnalysis boardStatsAnalysis)
        {
            var errorCards = new StringBuilder();
            boardStatsAnalysis.BoardStats.BadCardStats.ForEach(c => errorCards.AppendFormat("<div><a href=\"{0}\">{1}</a></div>", c.CardData.Card.Url, c.CardData.Card.Name));
            return errorCards.ToString();
        }

        private string GetHeadlineForCard(CardStats cardStat)
        {
            if (cardStat.CardData.Points > 0)
                return String.Format("<strong>{0}</strong> <span>({1}pts)</span>", cardStat.CardData.Card.Name, cardStat.CardData.Points);

            return String.Format("<strong>{0}</strong> (NE)", cardStat.CardData.Card.Name);
        }

       

        

       

        
    }
}
