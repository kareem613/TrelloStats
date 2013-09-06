using System;
using System.Diagnostics;
using System.Linq;
using TrelloStats.Clients;
using TrelloStats.Configuration;

namespace TrelloStats.Services
{
    public class TrelloToGoogleService
    {
        private readonly GoogleService _googleService;
        private readonly TrelloService _trelloService;
        private readonly BoardStatsService _boardStatsService;
        private readonly HighChartsJsonService _highChartsJsonService;
        private readonly TrelloStatsConfiguration _configuration;
        private readonly GoogleClient _googleClient;
        private readonly TimesheetService _timesheetService;

        public TrelloToGoogleService()
        {

            _configuration = new TrelloStatsConfiguration();
            var htmlFactory = new HtmlFactory(_configuration);
            var spreadsheetEntryFactory = new SpreadsheetEntryFactory(_configuration, htmlFactory);
            var trelloClient = new TrelloClient(_configuration);

            _googleClient = new GoogleClient(_configuration);
            _googleService = new GoogleService(_configuration, spreadsheetEntryFactory, _googleClient);
            _trelloService = new TrelloService(_configuration, trelloClient);
            _highChartsJsonService = new HighChartsJsonService(_configuration, htmlFactory);
            _timesheetService = new TimesheetService(_configuration, _googleClient);
            _boardStatsService = new BoardStatsService(_configuration);
        }

        public void CalculateStats(bool pushToGoogle, bool createJson)
        {


           
            CounterCreationDataCollection counters = new CounterCreationDataCollection();
            CounterCreationData totalTimeSheetEntries = new CounterCreationData();
            totalTimeSheetEntries.CounterName = "NumberOfTimesheetEntries";
            totalTimeSheetEntries.CounterHelp = "Total number of timesheet entries from google spreadsheet.";
            totalTimeSheetEntries.CounterType = PerformanceCounterType.NumberOfItems32;
            counters.Add(totalTimeSheetEntries);
            PerformanceCounterCategory.Delete("TrelloStats");
            PerformanceCounterCategory.Create("TrelloStats", "TrelloStats", PerformanceCounterCategoryType.SingleInstance, counters);
           
            var perfCounter = new PerformanceCounter("TrelloStats", "NumberOfTimesheetEntries", "", false);

            var stopwatch = Stopwatch.StartNew();
            Console.Write("Querying Timesheet data...");
            var timesheetData = _timesheetService.GetTimesheetData();
            perfCounter.RawValue = timesheetData.Count;
            Console.WriteLine(String.Format("Completed in {0}s.", stopwatch.Elapsed.TotalSeconds));

            stopwatch.Restart();
            Console.Write("Querying Trello...");
            var trelloData = _trelloService.GetCardsToExamine();
            Console.WriteLine(String.Format("Completed in {0}s.", stopwatch.Elapsed.TotalSeconds));
            
            stopwatch.Restart();
            Console.Write("Calculating stats...");
            BoardStatsAnalysis boardStatsAnalysis = _boardStatsService.BuildBoardStatsAnalysis(trelloData, timesheetData);
            Console.WriteLine(String.Format("Completed in {0}s.", stopwatch.Elapsed.TotalSeconds));

            if (pushToGoogle)
            {
                stopwatch.Restart();
                Console.Write("Deleting old records from Google...");
                _googleClient.ClearSpreadsheet(_configuration.GoogleSpreadsheetName);
                Console.WriteLine(String.Format("Completed in {0}s.", stopwatch.Elapsed.TotalSeconds));

                stopwatch.Restart();
                Console.Write("Pushing results to Google...");
                _googleService.PushToGoogleSpreadsheet(boardStatsAnalysis, _configuration.GoogleSpreadsheetName);
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
