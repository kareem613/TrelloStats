using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using TrelloStats.Model;

namespace TrelloStats
{
    public class TrelloToGoogleService
    {
        private readonly GoogleService _googleService;
        private readonly TrelloService _trelloService;
        private readonly BoardStatsService _boardStatsService;
   
        public TrelloToGoogleService()
        {
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(GetAppConfig("TimeZone"));
            _googleService = new GoogleService(GetAppConfig("Gmail.EmailAddress"), GetAppConfig("Gmail.OneTimePassword"), GetAppConfig("Google.SpreadsheetName"), GetAppSettingAsArray("Trello.Labels"), timeZone);
            _trelloService = new TrelloService(ConfigurationManager.AppSettings["Trello.Key"], ConfigurationManager.AppSettings["Trello.Token"], GetAppConfig("Trello.ListNames.InProgress"), GetAppConfig("Trello.ListNames.InTest"), GetAppSettingAsArray("Trello.ListNames.StartNames"), GetAppSettingAsArray("Trello.ListNames.CompletedNames"), GetAppSettingAsArray("Trello.ListNames.ExtraListsToInclude"), GetAppSettingAsArray("Trello.ListNames.ExtraListsToCount"), ConfigurationManager.AppSettings["Trello.Projections.EstimatedList"]);
            _boardStatsService = new BoardStatsService(_trelloService, timeZone);

            _boardStatsService.EstimateWindowLowerBoundFactor = GetAppConfigDouble("Trello.Projections.EstimateWindowLowerBoundFactor",1);
            _boardStatsService.EstimateWindowUpperBoundFactor = GetAppConfigDouble("Trello.Projections.EstimateWindowUpperBoundFactor", 1);

            _boardStatsService.WeeksToSkipForVelocityCalculation = GetAppConfigInt("Trello.Projections.WeeksToSkipForVelocityCalculation", 0);
        }

        private int GetAppConfigInt(string p, int defaultValue)
        {
            var configString = ConfigurationManager.AppSettings[p];
            int value;
            if (int.TryParse(configString, out value))
                return value;
            else return defaultValue;
        }

        private double GetAppConfigDouble(string p, double defaultValue)
        {
            var configString = ConfigurationManager.AppSettings[p];
            double value;
            if(double.TryParse(configString, out value))
                return value;
            else return defaultValue;
        }
  
        private string[] GetAppSettingAsArray(string key)
        {
            return ConfigurationManager.AppSettings[key].Split(',');
        }
  
        private string GetAppConfig(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        public void CalculateStats()
        {
            var stopwatch = Stopwatch.StartNew();
            Console.Write("Querying Trello...");
            var trelloData = _trelloService.GetCardsToExamine();
            Console.WriteLine(String.Format("Completed in {0}s.", stopwatch.Elapsed.TotalSeconds));
            
            stopwatch.Restart();
            Console.Write("Calculating stats...");
            BoardStats boardStats = _boardStatsService.BuildBoardStats(trelloData);
            Console.WriteLine(String.Format("Completed in {0}s.", stopwatch.Elapsed.TotalSeconds));

            stopwatch.Restart();
            Console.Write("Deleting old records from Google...");
            _googleService.ClearSpreadsheet();
            Console.WriteLine(String.Format("Completed in {0}s.", stopwatch.Elapsed.TotalSeconds));

            stopwatch.Restart();
            Console.Write("Pushing results to Google...");
            _googleService.PushToGoogleSpreadsheet(boardStats);
            Console.WriteLine(String.Format("Completed in {0}s.", stopwatch.Elapsed.TotalSeconds));
        }
  
       
    }

    
}
