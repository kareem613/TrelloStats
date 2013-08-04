using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using TrelloNet;

namespace TrelloStats
{
    public class TrelloService
    {
        private readonly TrelloStatsConfiguration _configuration;
        private readonly TrelloClient _trelloClient;
        
        public TrelloService(TrelloStatsConfiguration configuration, TrelloClient trelloClient)
        {
            _configuration = configuration;
            _trelloClient = trelloClient;
        }

        public TrelloData GetCardsToExamine()
        {
            var trinityStoriesBoard = _trelloClient.GetBoard(_configuration.TrelloBoard);
            var listsInBoard = _trelloClient.GetListsForBoard(trinityStoriesBoard);
            
            var listsToScan = GetListsToScan(listsInBoard);

            var trelloData = new TrelloData();
            foreach (var list in listsToScan)
            {
                var listData = CreateListData(list);
                trelloData.AddListData(listData);
            }

            foreach (var listName in _configuration.ListNames.ExtraListsToCount)
            {
                var list = listsInBoard.Where(l => listName ==l.Name).Single();
                var listData = CreateListData(list);
                trelloData.AddListToCount(listData);
            }
            
            return trelloData;
        }

        private ListData CreateListData(List list)
        {
            var cardList = _trelloClient.GetCardsForList(list);
            var listData = new ListData(list);

            foreach (var card in cardList)
            {
                listData.AddCardData(CreateCardData(card));
            }
            return listData;
        }

        private CardData CreateCardData(Card card)
        {
            return new CardData()
            {
                Card = card,
                Points = GetPointsForCard(card),
                Name = GetCardNameWithoutPoints(card),
                Actions = _trelloClient.GetActionsForCard(card)
            };
        }

        private IEnumerable<List> GetListsToScan(List<List> listsInBoard)
        {
            var doneLists = listsInBoard.Where(l => _configuration.ListNames.DoneListNames.Contains(l.Name));

            var listsToScan = new List<List>(doneLists);
            listsToScan.Add(listsInBoard.Single(l => l.Name == _configuration.ListNames.InProgressListName));
            listsToScan.Add(listsInBoard.Single(l => l.Name == _configuration.ListNames.InTestListName));
            foreach (var listName in _configuration.ListNames.ExtraListsToInclude)
            {
                listsToScan.Add(listsInBoard.Single(l => l.Name == listName));
            }
            return listsToScan.Distinct();
        }

        

        private double GetPointsForCard(Card card)
        {
            var match = Regex.Match(card.Name, @"^\((.*)\)(.*)");
            if (match.Success)
            {
                var pointsString = match.Groups[1].Value;
                double points;
                if (double.TryParse(pointsString, out points))
                {
                    return points;
                }
            }
            return 0;
        }

        private string GetCardNameWithoutPoints(Card card)
        {
            var match = Regex.Match(card.Name, @"^\((.*)\)(.*)");
            if (match.Success)
            {
                return match.Groups[2].Value; 
            }
            return card.Name;
        }
       
    }
}
