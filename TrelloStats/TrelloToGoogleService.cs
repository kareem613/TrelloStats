using System.Configuration;
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
            _googleService = new GoogleService(GetAppConfig("Gmail.EmailAddress"), GetAppConfig("Gmail.OneTimePassword"), GetAppConfig("Google.SpreadsheetName"), GetAppSettingAsArray("Trello.Labels"));
            _trelloService = new TrelloService(ConfigurationManager.AppSettings["Trello.Key"], ConfigurationManager.AppSettings["Trello.Token"], GetAppConfig("Trello.ListNames.InProgress"), GetAppSettingAsArray("Trello.ListNames.StartNames"), GetAppSettingAsArray("Trello.ListNames.CompletedNames"), GetAppSettingAsArray("Trello.ListNames.ExtraListsToInclude"));
            _boardStatsService = new BoardStatsService(_trelloService);
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
            var cards = _trelloService.GetCardsToExamine();

            BoardStats boardStats = _boardStatsService.BuildBoardStats(cards);
            _googleService.PushToGoogleSpreadsheet(boardStats);
        }
  
       
    }

    
}
