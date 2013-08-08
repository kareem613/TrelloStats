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
        private readonly HighChartsJsonService _highChartsJsonService;
        private readonly TrelloStatsConfiguration _configuration;

        public TrelloToGoogleService()
        {

            _configuration = new TrelloStatsConfiguration();
            var htmlFactory = new HtmlFactory(_configuration);
            var spreadsheetEntryFactory = new SpreadsheetEntryFactory(_configuration, htmlFactory);
            var trelloClient = new TrelloClient(_configuration);

            _googleService = new GoogleService(_configuration, spreadsheetEntryFactory);
            _trelloService = new TrelloService(_configuration, trelloClient);
            _highChartsJsonService = new HighChartsJsonService(_configuration, htmlFactory);
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
                stopwatch.Restart();
                Console.Write("Creating json output for highcharts...");
                _highChartsJsonService.CreateJsonData(boardStatsAnalysis);
                
                Console.WriteLine(String.Format("Completed in {0}s.", stopwatch.Elapsed.TotalSeconds));
            }
        }
  
        
  
       
    }

    
}
