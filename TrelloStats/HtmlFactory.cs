using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            
            

            return summaryText;
        }

        private string GetWeekStatsHtmlHeader()
        {
            var headerTitles = new List<string>() { "Week #", "Start", "End", "In Progress", "In Test", "Stories Completed", "Points Completed", "Timesheet Hours" };
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
            row.AppendLine(GetWeekStatsRow(GetNumberForTableDisplay(w.NumberOfCardsInProgress),"text-center"));
            row.AppendLine(GetWeekStatsRow(GetNumberForTableDisplay(w.NumberOfCardsInTest),"text-center"));

            row.AppendLine(GetWeekStatsRow(GetNumberForTableDisplay(w.NumberOfCompletedCards),"text-center"));

            foreach (var labelName in _configuration.LabelNames)
            {
                var labelNameSet = labelName.Split('/');
                if (labelNameSet.Length > 1)
                {
                    var value1 = w.GetNumberOfCardsWithLabel(labelNameSet[0]);
                    var value2 = w.GetNumberOfCardsWithLabel(labelNameSet[1]);
                    var valueSet = String.Format("{0}/{1}", value1, value2);
                    row.AppendLine(GetWeekStatsRow(valueSet,"text-center"));
                }
                else
                {
                    row.AppendLine(GetWeekStatsRow(GetNumberForTableDisplay(w.GetNumberOfCardsWithLabel(labelName)),"text-center"));
                }
            }

            row.AppendLine(GetWeekStatsRow(w.PointsCompleted,"text-center"));
            
            var hoursString = string.Format("{0}/{1}",Math.Ceiling(w.TotalHours),Math.Ceiling(w.TotalExcludedHours));
            row.AppendLine(GetWeekStatsRow(hoursString, "text-center"));


            row.Append("</tr>");

            return row.ToString();
        }

        public string GetProjectionsSummaryText(BoardProjections projections)
        {
            var template = "Team Velocity is <strong>[[velocity]]</strong> points per week. Incomplete estimated points are <strong>[[remaining_points]]</strong>. Expected completion window is <strong>[[expected_completion_min]] - [[expected_completion_max]]</strong>.";
            template = template.Replace("[[velocity]]", projections.historicalPointsPerWeek.ToString("##"))
                .Replace("[[remaining_points]]", projections.EstimatePoints.ToString())
                .Replace("[[expected_completion_min]]", projections.ProjectedMinimumCompletionDate.ToLongDateString())
                .Replace("[[expected_completion_max]]", projections.ProjectedMaximumCompletionDate.ToLongDateString());
            return template;
        }



        private string GetExtraListsStatsTableHtml(BoardStatsAnalysis boardStatsAnalysis)
        {
            var row = new StringBuilder(@"<table id=""list_stats"" class=""table-condensed table-bordered table-striped"">");
            row.AppendLine("<thead><th>List</th><th>Cards</th><th>Points</th></thead>");
            row.AppendLine("<tbody>");
            foreach (var listStat in boardStatsAnalysis.BoardStats.ListStats)
            {
                var pointsForList = listStat.ListData.CardDataCollection.Sum(c => c.Points);
                row.AppendLine(string.Format(@"<tr><th>{0}</th><td class=""text-center"">{1}</td><td class=""text-center"">{2}</td></tr>", listStat.ListData.List.Name, listStat.CardCount, pointsForList == 0 ? "-" : pointsForList.ToString()));
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

        internal string GetExtraListsStatsTable(BoardStatsAnalysis boardStatsAnalysis)
        {
            return GetExtraListsStatsTableHtml(boardStatsAnalysis);
            

        }

        internal string GetWeeklyStatsRows(BoardStatsAnalysis boardStatsAnalysis)
        {
            var weekStatsHeader = GetWeekStatsHtmlHeader();

            var weekStatsList = boardStatsAnalysis.WeekStats;
            var weekRows = new StringBuilder();
            weekStatsList.OrderByDescending(ws=>ws.WeekNumber).ToList().ForEach(w => weekRows.Append(GetWeekStatsHtmlRow(w, boardStatsAnalysis)));

            return weekStatsHeader + weekRows;
        }
    }
}
