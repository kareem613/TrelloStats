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
        private readonly Trello _trello;
        
        public TrelloService(TrelloStatsConfiguration configuration)
        {
            _configuration = configuration;
            _trello = new Trello(configuration.TrelloKey);
            //var url = trello.GetAuthorizationUrl("Trello Stats", Scope.ReadOnly, Expiration.Never);
            _trello.Authorize(configuration.TrelloToken);
            
}

        public IEnumerable<T> GetActionsForCard<T>(Card card)
        {
            return _trello.Actions.ForCard(card, new List<ActionType>() { ActionType.CreateCard, ActionType.UpdateCard }).OfType<T>();
        }

        public List<Action> GetActionsForCard(Card card)
        {
            return _trello.Actions.ForCard(card, new List<ActionType>(){ActionType.CreateCard, ActionType.UpdateCard}).ToList();
        }


        public TrelloData GetCardsToExamine()
        {
            var trinityStoriesBoard = _trello.Boards.Search("Trinity Stories").Single();
            var listsInBoard = _trello.Lists.ForBoard(trinityStoriesBoard).ToList();

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
            var cardList = GetCardsForList(list);
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
                Actions = GetActionsForCard(card)
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

        internal List<Card> GetCardsForList(List trelloList)
        {
            return _trello.Cards.ForList(trelloList).ToList();
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
