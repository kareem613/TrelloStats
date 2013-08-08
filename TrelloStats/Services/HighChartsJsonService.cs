using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrelloStats.Configuration;

namespace TrelloStats.Services
{
    class HighChartsJsonService
    {
        private readonly ITrelloStatsConfiguration _configuration;
        private readonly HtmlFactory _htmlFactory;

        public HighChartsJsonService(ITrelloStatsConfiguration configuration, HtmlFactory htmlFactory)
        {
            _configuration = configuration;
            _htmlFactory = htmlFactory;
        }

        public void CreateJsonData(BoardStatsAnalysis boardStatsAnalysis)
        {
            dynamic data = new ExpandoObject();
            data.weeklyStats = CreateWeeklyStatsSeriesCollection(boardStatsAnalysis);
            
            var burndownData = GetBurndownData(boardStatsAnalysis);
            var burndownSeries = CreateSeries("Historical",burndownData);
            var projectionSeries = CreateProjectionsSeries(boardStatsAnalysis);
            projectionSeries.Add(burndownSeries);

            data.burndown = projectionSeries;

            var json = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented, new JavaScriptDateTimeConverter());

            var jsonFileInfo = new FileInfo(Path.Combine(_configuration.WebsiteOutputFolder,_configuration.WebsiteJsonFilename));
            File.WriteAllText(jsonFileInfo.FullName, "var data = " + json);

            var htmlFileInfo = new FileInfo(Path.Combine(_configuration.WebsiteOutputFolder, _configuration.WebsiteHtmlFilename));
            var htmlSourceFileInfo = new FileInfo("Html\\index.html");

            var html = File.ReadAllText(htmlSourceFileInfo.FullName);
            html = html.Replace("[[summary]]",_htmlFactory.GetSummaryTextForBoardStat(boardStatsAnalysis));

            File.WriteAllText(htmlFileInfo.FullName, html);

            CopyFileToOutputFolder("style.css");
            CopyFileToOutputFolder("bootstrap.min.css");
        }
  
        private void CopyFileToOutputFolder(string filename)
        {
            var cssFileInfo = new FileInfo(Path.Combine(_configuration.WebsiteOutputFolder, filename));
            File.Copy("Html\\" + filename, cssFileInfo.FullName, true);
        }

        private List<dynamic> GetBurndownData(BoardStatsAnalysis boardStatsAnalysis)
        {
            var totalPoints = boardStatsAnalysis.TotalPoints + boardStatsAnalysis.EstimatedListPoints;
            var totalPointsRemaining = totalPoints;

            var burnDownData = new List<dynamic>();
            foreach (var weekStats in boardStatsAnalysis.WeekStats.Take(boardStatsAnalysis.WeekStats.Count - 1))
            {
                totalPointsRemaining -= weekStats.PointsCompleted;
                var point = GetDateEstimatePoint(weekStats.EndDate, totalPointsRemaining);
                burnDownData.Add(point);
            }

            var lastWeekStats = boardStatsAnalysis.WeekStats.Last();
            totalPointsRemaining -= lastWeekStats.PointsCompleted;
            var lastPoint = GetDateEstimatePoint(DateTime.Now, totalPointsRemaining);
            burnDownData.Add(lastPoint);
            return burnDownData;
        }

        private List<dynamic> CreateProjectionsSeries(BoardStatsAnalysis boardStatsAnalysis)
        {
            dynamic projectPointBestCase = GetCompletionProjectionSeries("Best Case", boardStatsAnalysis.Projections.EstimatePoints, boardStatsAnalysis.Projections.ProjectedMinimumCompletionDate);
            dynamic projectPointWorstCase = GetCompletionProjectionSeries("Worst Case", boardStatsAnalysis.Projections.EstimatePoints, boardStatsAnalysis.Projections.ProjectedMaximumCompletionDate);
            dynamic projectPointIdeal = GetCompletionProjectionSeries("Ideal", boardStatsAnalysis.Projections.EstimatePoints, boardStatsAnalysis.Projections.ProjectionCompletionDate);

            return new List<dynamic>(){ projectPointBestCase, projectPointWorstCase, projectPointIdeal };
        }

        private List<dynamic> CreateWeeklyStatsSeriesCollection(BoardStatsAnalysis boardStatsAnalysis)
        {
            var seriesCollection = new System.Collections.Generic.List<dynamic>();

            dynamic series = CreateSeries("In Progress", "In Progress", boardStatsAnalysis.WeekStats.Select(w => w.NumberOfCardsInProgress).ToArray());
            seriesCollection.Add(series);

            series = CreateSeries("In Test", "In Test", boardStatsAnalysis.WeekStats.Select(w => w.NumberOfCardsInTest).ToArray());
            seriesCollection.Add(series);

            series = CreateSeries("Trinity", "Stories Completed", boardStatsAnalysis.WeekStats.Select(w => w.GetNumberOfCardsWithLabel("Trinity")).ToArray());
            seriesCollection.Add(series);

            series = CreateSeries("Classic", "Stories Completed", boardStatsAnalysis.WeekStats.Select(w => w.GetNumberOfCardsWithLabel("Classic")).ToArray());
            seriesCollection.Add(series);

            series = CreateSeries("Implemented Prior to Estimate", "Implemented Prior to Estimate", boardStatsAnalysis.WeekStats.Select(w => w.GetNumberOfCardsWithLabel("Implemented Prior to Estimate")).ToArray());
            seriesCollection.Add(series);

            series = CreateSeries("Hotfix", "Hotfix", boardStatsAnalysis.WeekStats.Select(w => w.GetNumberOfCardsWithLabel("Hotfix")).ToArray());
            seriesCollection.Add(series);

            return seriesCollection;
        }

        private dynamic GetCompletionProjectionSeries(string name, double estimatedPoints, DateTime completionDate)
        {
            dynamic projectPoint = new ExpandoObject();
            projectPoint.name = name;
            var point1 = GetDateEstimatePoint(DateTime.Now, estimatedPoints);
            var point2 = GetDateEstimatePoint(completionDate, 0);
            projectPoint.data = new object[] { point1, point2 };
            return projectPoint;
        }

        private dynamic GetDateEstimatePoint(DateTime date, double points)
        {
            var epochTime = GetEpochTime(date);
            return new object[] { epochTime, points };
        }

        private static long GetEpochTime(DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0);
            return ((long)(date - epoch).TotalMilliseconds);
        }

        private static dynamic CreateSeries(string seriesName, List<dynamic> data)
        {
            dynamic series = new ExpandoObject();
            series.name = seriesName;
            series.data = data;
            return series;
        }

        private static dynamic CreateSeries(string seriesName, string stackName, int[] data)
        {
            dynamic series = new ExpandoObject();
            series.name = seriesName;
            series.data = data;
            series.stack = stackName;
            return series;
        }
    }
}
