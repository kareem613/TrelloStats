using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrelloStats.Configuration;
using TrelloStats.Model.Stats;

namespace TrelloStats
{
    public class HtmlFactory
    {
        ITrelloStatsConfiguration _configuration;

        public HtmlFactory(ITrelloStatsConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public string GetSummaryTextForBoardStat(BoardStatsAnalysis boardStatsAnalysis)
        {
            var weekStatsHeader = GetWeekStatsHtmlHeader();

            var weekStatsList = boardStatsAnalysis.WeekStats;
            var weekRows = new StringBuilder();
            weekStatsList.ForEach(w => weekRows.Append(GetWeekStatsHtmlRow(w, boardStatsAnalysis)));

            var summaryText = String.Format(_configuration.SummaryTextTemplate,
                    boardStatsAnalysis.FirstStartDate.ToLongDateString(),
                    boardStatsAnalysis.NumberOfCompletedCards,
                    boardStatsAnalysis.LastDoneDate.ToLongDateString(),
                    boardStatsAnalysis.BoardStats.CreatedDate.ToLongDateString(),
                    boardStatsAnalysis.BoardStats.CreatedDate.ToLongTimeString(),
                    boardStatsAnalysis.TotalPoints
                );

            summaryText = summaryText.Replace("[[projections_summary]]", GetProjectionsSummaryText(boardStatsAnalysis));
            summaryText = summaryText.Replace("[[extra_lists_stats_table]]", GetExtraListsStatsTableHtml(boardStatsAnalysis));
            summaryText = summaryText.Replace("[[weekly_stats_header]]", weekStatsHeader);
            summaryText = summaryText.Replace("[[weekly_stats_rows]]", weekRows.ToString());


            return summaryText;
        }

        private string GetWeekStatsHtmlHeader()
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

        private string GetWeekStatsHtmlRow(WeekStats w, BoardStatsAnalysis boardStatsAnalysis)
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

        private string GetProjectionsSummaryText(BoardStatsAnalysis boardStatsAnalysis)
        {
            var template = "Team Velocity is <strong>[[velocity]]</strong> points per week. Incomplete estimated points are <strong>[[remaining_points]]</strong>. Expected completion window is <strong>[[expected_completion_min]] - [[expected_completion_max]]</strong>.";
            template = template.Replace("[[velocity]]", boardStatsAnalysis.Projections.historicalPointsPerWeek.ToString("##"))
                .Replace("[[remaining_points]]", boardStatsAnalysis.Projections.EstimatePoints.ToString())
                .Replace("[[expected_completion_min]]", boardStatsAnalysis.Projections.ProjectedMinimumCompletionDate.ToLongDateString())
                .Replace("[[expected_completion_max]]", boardStatsAnalysis.Projections.ProjectedMaximumCompletionDate.ToLongDateString());
            return template;
        }



        private string GetExtraListsStatsTableHtml(BoardStatsAnalysis boardStatsAnalysis)
        {
            var row = new StringBuilder(@"<table id=""list_stats"" class=""table-condensed"">");
            row.AppendLine("<thead><th>List</th><th>Cards</th><th>Points</th></thead>");
            row.AppendLine("<tbody>");
            foreach (var listStat in boardStatsAnalysis.BoardStats.ListStats)
            {
                var pointsForList = listStat.ListData.CardDataCollection.Sum(c => c.Points);
                row.AppendLine(string.Format("<tr><th>{0}</th><td>{1}</td><td>{2}</td></tr>", listStat.ListData.List.Name, listStat.CardCount, pointsForList == 0 ? "-" : pointsForList.ToString()));
            }
            row.AppendLine("</tbody></table>");
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
