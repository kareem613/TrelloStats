using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrelloStats.Services
{
    class HighChartsJsonService
    {
        public string CreateJsonData(BoardStatsAnalysis boardStatsAnalysis)
        {
            var seriesCollection = new System.Collections.Generic.List<dynamic>();

            dynamic data = new ExpandoObject();

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

            data.weeklyStats = seriesCollection;

            dynamic projectPointBestCase = GetCompletionProjectionPoint("Best Case", boardStatsAnalysis.Projections.EstimatePoints, boardStatsAnalysis.Projections.ProjectedMinimumCompletionDate);
            dynamic projectPointWorstCase = GetCompletionProjectionPoint("Worst Case", boardStatsAnalysis.Projections.EstimatePoints, boardStatsAnalysis.Projections.ProjectedMaximumCompletionDate);
            dynamic projectPointIdeal = GetCompletionProjectionPoint("Ideal", boardStatsAnalysis.Projections.EstimatePoints, boardStatsAnalysis.Projections.ProjectionCompletionDate);

            dynamic projectionSeries = new object[] { projectPointBestCase, projectPointWorstCase, projectPointIdeal };

            data.projections = projectionSeries;


            string json = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented, new JavaScriptDateTimeConverter());

            return json;
        }

        private dynamic GetCompletionProjectionPoint(string name, double estimatedPoints, DateTime completionDate)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0);
            var todayEpoch = ((long)(DateTime.Now - epoch).TotalMilliseconds);
            var completionEpoch = ((long)(completionDate - epoch).TotalMilliseconds);

            dynamic projectPoint = new ExpandoObject();
            projectPoint.name = name;
            var point1 = new object[] { todayEpoch, estimatedPoints };
            var point2 = new object[] { completionEpoch, 0 };
            projectPoint.data = new object[] { point1, point2 };
            return projectPoint;
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
