using Google.GData.Spreadsheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrelloStats.Model;

namespace TrelloStats
{
    public class SpreadsheetEntryFactory
    {
        TrelloStatsConfiguration _configuration;

        public SpreadsheetEntryFactory(TrelloStatsConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ListEntry GetBadCardEntry(BoardStats boardStats)
        {
            var errorRow = new ListEntry();
            errorRow.Elements.Add(new ListEntry.Custom() { LocalName = "startdate", Value = boardStats.FirstStartDate.ToString() });
            errorRow.Elements.Add(new ListEntry.Custom() { LocalName = "enddate", Value = "" });
            errorRow.Elements.Add(new ListEntry.Custom() { LocalName = "headline", Value = "Unproccessed Trello Cards" });
            errorRow.Elements.Add(new ListEntry.Custom() { LocalName = "text", Value = GetSummaryTextForErrorCards(boardStats) });
            return errorRow;
        }

        public ListEntry GetCompletedCardEntry(CardStats cardStat, TimeSpan timeOffset)
        {
            var row = new ListEntry();
            row.Elements.Add(new ListEntry.Custom() { LocalName = "startdate", Value = cardStat.GetDoneAction().DateInTimeZone(_configuration.TimeZone).Add(timeOffset).ToString() });
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

        public ListEntry GetTitleCardEntry(BoardStats boardStats)
        {
            var titleRow = new ListEntry();
            titleRow.Elements.Add(new ListEntry.Custom() { LocalName = "startdate", Value = boardStats.FirstStartDate.ToString() });
            titleRow.Elements.Add(new ListEntry.Custom() { LocalName = "enddate", Value = "" });
            titleRow.Elements.Add(new ListEntry.Custom() { LocalName = "headline", Value = "Development Timeline" });
            titleRow.Elements.Add(new ListEntry.Custom() { LocalName = "text", Value = GetSummaryTextForBoardStat(boardStats) });
            titleRow.Elements.Add(new ListEntry.Custom() { LocalName = "type", Value = "title" });
            return titleRow;
        }

        private string GetSummaryTextForErrorCards(BoardStats boardStats)
        {
            var errorCards = new StringBuilder();
            boardStats.BoardData.BadCardStats.ForEach(c => errorCards.AppendFormat("<div><a href=\"{0}\">{1}</a></div>", c.CardData.Card.Url, c.CardData.Card.Name));
            return errorCards.ToString();
        }

        private string GetHeadlineForCard(CardStats cardStat)
        {
            if (cardStat.CardData.Points > 0)
                return String.Format("<strong>{0}</strong> <span>({1}pts)</span>", cardStat.CardData.Card.Name, cardStat.CardData.Points);

            return String.Format("<strong>{0}</strong> (NE)", cardStat.CardData.Card.Name);
        }

        private string GetSummaryTextForBoardStat(BoardStats boardStats)
        {
            var weekStatsHeader = GetWeekStatsHtmlHeader(boardStats);

            var weekStatsList = boardStats.GetWeeklyStats();
            var weekRows = new StringBuilder();
            weekStatsList.ForEach(w => weekRows.Append(GetWeekStatsHtmlRow(w, boardStats)));

            var summaryText = String.Format(_configuration.SummaryTextTemplate,
                    boardStats.FirstStartDate.ToLongDateString(),
                    boardStats.NumberOfCompletedCards,
                    boardStats.LastDoneDate.ToLongDateString(),
                    boardStats.BoardData.CreatedDate.ToLongDateString(),
                    boardStats.BoardData.CreatedDate.ToLongTimeString(),
                    boardStats.TotalPoints
                );

            summaryText = summaryText.Replace("[[projections_summary]]", GetProjectionsSummaryText(boardStats));
            summaryText = summaryText.Replace("[[extra_lists_stats_table]]", GetExtraListsStatsTableHtml(boardStats));
            summaryText = summaryText.Replace("[[weekly_stats_header]]", weekStatsHeader);
            summaryText = summaryText.Replace("[[weekly_stats_rows]]", weekRows.ToString());


            return summaryText;
        }

        private string GetProjectionsSummaryText(BoardStats boardStats)
        {
            var template = "Team Velocity is <strong>[[velocity]]</strong> points per week. Remaining points are <strong>[[remaining_points]]</strong>. Expected completion window is <strong>[[expected_completion_min]] - [[expected_completion_max]]</strong>.";
            template = template.Replace("[[velocity]]", boardStats.Projections.historicalPointsPerWeek.ToString("##"))
                .Replace("[[remaining_points]]", boardStats.Projections.EstimatePoints.ToString())
                .Replace("[[expected_completion_min]]", boardStats.Projections.ProjectedMinimumCompletionDate.ToLongDateString())
                .Replace("[[expected_completion_max]]", boardStats.Projections.ProjectedMaximumCompletionDate.ToLongDateString());
            return template;
        }



        private string GetExtraListsStatsTableHtml(BoardStats boardStats)
        {
            var row = new StringBuilder(@"<table id=""list_stats"" class=""stats""><tbody>");
            foreach (var listStat in boardStats.BoardData.ListStats)
            {
                row.AppendLine(string.Format("<tr><th>{0}</th><td>{1}</td></tr>", listStat.ListData.List.Name, listStat.CardCount));
            }
            row.AppendLine("</tbody></table>");
            return row.ToString();
        }

        private string GetWeekStatsHtmlHeader(BoardStats boardStats)
        {
            var headerTitles = new List<string>() { "Week #", "Start", "End", "In Progress", "In Test", "Stories Completed", "Points Completed" };
            foreach (var labelName in _configuration.LabelNames)
            {
                headerTitles.Insert(headerTitles.Count - 1, labelName);
            }

            var header = new StringBuilder("<tr>");
            for (int i = 0; i < headerTitles.Count; i++)
            {
                header.AppendFormat("<th>{0}</th>", headerTitles[i]);
            }



            header.AppendLine("</tr>");
            return header.ToString();
        }

        private string GetWeekStatsHtmlRow(WeekStats w, BoardStats boardStats)
        {
            var row = new StringBuilder("<tr>");
            row.AppendLine(GetWeekStatsRow(w.WeekNumber));

            row.AppendLine(GetWeekStatsRow(w.StartDate.ToShortDateString(), "date"));
            row.AppendLine(GetWeekStatsRow(w.EndDate.ToShortDateString(), "date"));
            row.AppendLine(GetWeekStatsRow(GetNumberForTableDisplay(w.NumberOfCardsInProgress)));
            row.AppendLine(GetWeekStatsRow(GetNumberForTableDisplay(w.NumberOfCardsInTest)));

            row.AppendLine(GetWeekStatsRow(GetNumberForTableDisplay(w.NumberOfCompletedCards)));

            foreach (var labelName in _configuration.LabelNames)
            {
                var labelNameSet = labelName.Split('/');
                if (labelNameSet.Length > 1)
                {
                    var value1 = w.GetNumberOfCardsWithLabel(labelNameSet[0]);
                    var value2 = w.GetNumberOfCardsWithLabel(labelNameSet[1]);
                    var valueSet = String.Format("{0}/{1}", value1, value2);
                    row.AppendLine(GetWeekStatsRow(valueSet));
                }
                else
                {
                    row.AppendLine(GetWeekStatsRow(GetNumberForTableDisplay(w.GetNumberOfCardsWithLabel(labelName))));
                }
            }

            row.AppendLine(GetWeekStatsRow(w.PointsCompleted));


            row.Append("</tr>");

            return row.ToString();
        }

        private string GetNumberForTableDisplay(int number)
        {
            return number > 0 ? number.ToString() : "-";
        }

        private string GetWeekStatsRow(object value, string cssClass)
        {
            return string.Format(@"<td class=""{0}"">{1}</td>", cssClass, value.ToString());
        }

        private string GetWeekStatsRow(object value)
        {
            return string.Format("<td>{0}</td>", value.ToString());
        }
    }
}
