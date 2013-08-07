using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using TrelloStats.Configuration;
using TrelloStats.Model;
using TrelloStats.Model.Stats;

namespace TrelloStats.Services
{
    public class TrelloToGoogleService
    {
        private readonly GoogleService _googleService;
        private readonly TrelloService _trelloService;
        private readonly BoardStatsService _boardStatsService;
        private readonly TrelloStatsConfiguration _configuration;

        public TrelloToGoogleService()
        {

            _configuration = new TrelloStatsConfiguration();
            var spreadsheetEntryFactory = new SpreadsheetEntryFactory(_configuration);
            var trelloClient = new TrelloClient(_configuration);

            _googleService = new GoogleService(_configuration, spreadsheetEntryFactory);
            _trelloService = new TrelloService(_configuration, trelloClient);
            _boardStatsService = new BoardStatsService(_configuration);
        }

        public void CalculateStats(bool pushToGoogle, bool createJson)
        {
            var stopwatch = Stopwatch.StartNew();
            Console.Write("Querying Trello...");
            var trelloData = _trelloService.GetCardsToExamine();
            Console.WriteLine(String.Format("Completed in {0}s.", stopwatch.Elapsed.TotalSeconds));
            
            stopwatch.Restart();
            Console.Write("Calculating stats...");
            BoardStatsAnalysis boardStatsAnalysis = _boardStatsService.BuildBoardStatsAnalysis(trelloData);
            Console.WriteLine(String.Format("Completed in {0}s.", stopwatch.Elapsed.TotalSeconds));

            if (pushToGoogle)
            {
                stopwatch.Restart();
                Console.Write("Deleting old records from Google...");
                _googleService.ClearSpreadsheet();
                Console.WriteLine(String.Format("Completed in {0}s.", stopwatch.Elapsed.TotalSeconds));

                stopwatch.Restart();
                Console.Write("Pushing results to Google...");
                _googleService.PushToGoogleSpreadsheet(boardStatsAnalysis);
                Console.WriteLine(String.Format("Completed in {0}s.", stopwatch.Elapsed.TotalSeconds));
            }

            if (createJson)
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
                //data.CompletedCards = boardStatsAnalysis.CompletedCardStats.Select(c => new {CardTitle = c.CardData.Card.Name, CompletionDate = c.DoneAction.Date, ElapsedDays = c.BusinessDaysElapsed, Points = c.CardData.Points });

                dynamic projectPointBestCase = GetCompletionProjectionPoint("Best Case", boardStatsAnalysis.Projections.EstimatePoints, boardStatsAnalysis.Projections.ProjectedMinimumCompletionDate);
                dynamic projectPointWorstCase = GetCompletionProjectionPoint("Worst Case", boardStatsAnalysis.Projections.EstimatePoints, boardStatsAnalysis.Projections.ProjectedMaximumCompletionDate);
                dynamic projectPointIdeal= GetCompletionProjectionPoint("Ideal", boardStatsAnalysis.Projections.EstimatePoints, boardStatsAnalysis.Projections.ProjectionCompletionDate);

                dynamic projectionSeries = new object[] { projectPointBestCase, projectPointWorstCase, projectPointIdeal };

                data.projections = projectionSeries;


                string json = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented, new JavaScriptDateTimeConverter());
                
                var fileInfo = new FileInfo(_configuration.JsonOutputFilename);
                File.WriteAllText(fileInfo.FullName, "var data = " + json);
            }
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
