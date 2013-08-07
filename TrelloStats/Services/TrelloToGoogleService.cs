using System;
using System.Configuration;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using TrelloStats.Configuration;
using TrelloStats.Model;

namespace TrelloStats.Services
{
    public class TrelloToGoogleService
    {
        private readonly GoogleService _googleService;
        private readonly TrelloService _trelloService;
        private readonly BoardStatsService _boardStatsService;
   
        public TrelloToGoogleService()
        {
            
            var config = new TrelloStatsConfiguration();
            var spreadsheetEntryFactory = new SpreadsheetEntryFactory(config);
            var trelloClient = new TrelloClient(config);

            _googleService = new GoogleService(config, spreadsheetEntryFactory);
            _trelloService = new TrelloService(config, trelloClient);
            _boardStatsService = new BoardStatsService(config);
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

                dynamic series = new ExpandoObject();
                series.name = "In Progress";
                series.data = boardStatsAnalysis.WeekStats.Select(w => w.NumberOfCardsInProgress).ToArray();
                series.stack = "In Progress";

                
                seriesCollection.Add(series);

                dynamic series2 = new ExpandoObject();
                series2.name = "Trinity";
                series2.data = boardStatsAnalysis.WeekStats.Select(w => w.GetNumberOfCardsWithLabel("Trinity")).ToArray();
                series2.stack = "Stories Completed";
                seriesCollection.Add(series2);

                dynamic series3 = new ExpandoObject();
                series3.name = "Classic";
                series3.data = boardStatsAnalysis.WeekStats.Select(w => w.GetNumberOfCardsWithLabel("Classic")).ToArray();
                series3.stack = "Stories Completed";
                seriesCollection.Add(series3);

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(seriesCollection);
                //string json = Newtonsoft.Json.JsonConvert.SerializeObject(boardStatsAnalysis);


                File.WriteAllText("output.json", json);
            }
        }
  
       
    }

    
}
