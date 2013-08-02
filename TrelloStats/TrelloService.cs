using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using TrelloNet;

namespace TrelloStats
{
    public class TrelloService
    {
        public readonly string InProgressListName;
        public readonly string[] StartListNames;
        public readonly string[] DoneListNames;
        private readonly string[] ExtraListNamesToScan;
        private readonly string[] ListNamesToStat;
        public readonly string EstimatedList;
        
        private readonly Trello _trello;

        public TrelloService(string key, string token, string inProgressListName, string[] startListNames, string[] doneListNames, string[] extraListsToScan, string[] listNamesToStat, string estimatedList)
        {
            _trello = new Trello(key);
            //var url = trello.GetAuthorizationUrl("Trello Stats", Scope.ReadOnly, Expiration.Never);
            _trello.Authorize(token);
            

            InProgressListName = inProgressListName;
            StartListNames = startListNames;
            DoneListNames = doneListNames;
            ExtraListNamesToScan = extraListsToScan.Where(s=>!string.IsNullOrWhiteSpace(s)).ToArray();
            ListNamesToStat = listNamesToStat.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            EstimatedList = estimatedList;
        }

        public IEnumerable<T> GetActionsForCard<T>(Card card)
        {
            return _trello.Actions.ForCard(card).OfType<T>();
        }


        public TrelloData GetCardsToExamine()
        {
            var trinityStoriesBoard = _trello.Boards.Search("Trinity Stories").Single();
            var listsInBoard = _trello.Lists.ForBoard(trinityStoriesBoard).ToList();

            var listsToScan = GetListsToScan(listsInBoard);

            var trelloData = new TrelloData();
            trelloData.Cards = new Dictionary<List,List<Card>>();
            foreach (var list in listsToScan)
            {
                var cardList = new List<Card>();
                cardList.AddRange(_trello.Cards.ForList(list));
                trelloData.Cards.Add(list, cardList);
            }

            foreach (var listName in ListNamesToStat)
            {
                var list = listsInBoard.Where(l => listName ==l.Name).Single();
                trelloData.AddListToStat(list);
            }
            
            return trelloData;
        }

        private IEnumerable<List> GetListsToScan(List<List> listsInBoard)
        {
            var doneLists = listsInBoard.Where(l => DoneListNames.Contains(l.Name));

            var listsToScan = new List<List>(doneLists);
            listsToScan.Add(listsInBoard.Single(l => l.Name == InProgressListName));
            foreach (var listName in ExtraListNamesToScan)
            {
                listsToScan.Add(listsInBoard.Single(l => l.Name == listName));
            }
            return listsToScan.Distinct();
        }

        internal List<Card> GetCardsForList(List trelloList)
        {
            return _trello.Cards.ForList(trelloList).ToList();
        }
    }
}
