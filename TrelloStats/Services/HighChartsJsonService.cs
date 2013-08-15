using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TrelloStats.Configuration;
using TrelloStats.Model.Stats;

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

            var burndownPointsData = GetBurndownData(boardStatsAnalysis);
            var burndownSeries = CreateSeries("Historical",burndownPointsData);
            var projectionSeries = CreateProjectionsSeries(boardStatsAnalysis);
            projectionSeries.Add(burndownSeries);
            data.burndown = projectionSeries;

            var burndownHoursData = GetBurndownHoursData(boardStatsAnalysis);
            var burndownHoursSeries = CreateSeries("HistoricalHours", burndownHoursData);
            data.historicalHours = burndownHoursSeries;

            var milestoneSeries = GetMilestonesSeries(boardStatsAnalysis);
            projectionSeries.AddRange(milestoneSeries);

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

        private List<dynamic> GetMilestonesSeries(BoardStatsAnalysis boardStatsAnalysis)
        {
            var totalPoints = boardStatsAnalysis.TotalPoints + boardStatsAnalysis.EstimatedListPoints;
            List<Milestone> milestones = boardStatsAnalysis.Milestones;
            var milestoneSeries = new List<dynamic>();
            foreach (var milestone in milestones)
            {

                var topPoint = GetDateValuePoint(milestone.TargetDate, totalPoints);
                topPoint.milestoneName = milestone.Name;
                topPoint.isTopPoint = true;
                var bottomPoint = GetDateValuePoint(milestone.TargetDate, 0);
                bottomPoint.milestoneName = milestone.Name;
                bottomPoint.isTopPoint = false;
                

                var series = CreateSeries("Milestones", new List<dynamic>() { bottomPoint, topPoint });
                series.color = "#AFAFC1";
                series.marker = new ExpandoObject();
                series.marker.enabled = false;
                milestoneSeries.Add(series);
            }
            return milestoneSeries;
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
                var point = GetDateValuePoint(weekStats.EndDate, totalPointsRemaining);
                burnDownData.Add(point);
            }

            var lastWeekStats = boardStatsAnalysis.WeekStats.Last();
            totalPointsRemaining -= lastWeekStats.PointsCompleted;
            var lastPoint = GetDateValuePoint(DateTime.Now, totalPointsRemaining);
            burnDownData.Add(lastPoint);
            return burnDownData;
        }

        private List<dynamic> GetBurndownHoursData(BoardStatsAnalysis boardStatsAnalysis)
        {
            var burndownHoursData = new List<dynamic>();
            foreach (var weekStats in boardStatsAnalysis.WeekStats.Take(boardStatsAnalysis.WeekStats.Count - 1))
            {
                var point = GetDateValuePoint(weekStats.EndDate, weekStats.TotalHours);
                burndownHoursData.Add(point);
            }

            var lastWeekStats = boardStatsAnalysis.WeekStats.Last();
            var lastPoint = GetDateValuePoint(DateTime.Now, lastWeekStats.TotalHours);
            burndownHoursData.Add(lastPoint);
            return burndownHoursData;
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
            dynamic series = new ExpandoObject();
            series.name = name;
            var startPoint = GetDateValuePoint(DateTime.Now, estimatedPoints);
            startPoint.isStartPoint = true;

            var endPoint = GetDateValuePoint(completionDate, 0);
            endPoint.isStartPoint = false;

            series.data = new object[] { startPoint, endPoint };
            series.isProjection = true;
            return series;
        }

        private dynamic GetDateValuePoint(DateTime date, double points)
        {
            var epochTime = GetEpochTime(date);
            dynamic point = new ExpandoObject() ;
            point.x = epochTime;
            point.y = points;
            return point;
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
