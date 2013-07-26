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
            _googleService = new GoogleService(ConfigurationManager.AppSettings["Gmail.EmailAddress"], ConfigurationManager.AppSettings["Gmail.OneTimePassword"]);
            _trelloService = new TrelloService(ConfigurationManager.AppSettings["Trello.Key"],ConfigurationManager.AppSettings["Trello.Token"]);
            _boardStatsService = new BoardStatsService(_trelloService);
        }

        public void CalculateStats()
        {
            var cards = _trelloService.GetCardsToExamine();

            BoardStats boardStats = _boardStatsService.BuildBoardStats(cards);
            _googleService.PushToGoogleSpreadsheet(boardStats);
        }
  
       
    }

    
}
