using System.Collections.Generic;
using System.Linq;
using TrelloNet;

namespace TrelloStats
{
    public class TrelloService
    {
        public static readonly string IN_PROGRESS_LIST_NAME = "Doing";
        public static readonly string[] START_LIST_NAMES = new string[] {IN_PROGRESS_LIST_NAME, "HotFix" };
        public static readonly string[] DONE_LIST_NAMES = new string[] {"Value Delivered", "Non-Trinity Value Delivered"};
        private readonly string[] ExtraListNamesToScan = new string[] { "Demoed" };
        
        private readonly Trello _trello;

        public TrelloService(string key, string token)
        {
            _trello = new Trello(key);
            //var url = trello.GetAuthorizationUrl("Trello Stats", Scope.ReadOnly, Expiration.Never);
            _trello.Authorize(token);
        }

        public IEnumerable<T> GetActionsForCard<T>(Card card)
        {
            return _trello.Actions.ForCard(card).OfType<T>();
        }


        public Dictionary<List,List<Card>> GetCardsToExamine()
        {
            var trinityStoriesBoard = _trello.Boards.Search("Trinity Stories").Single();
            var listsInBoard = _trello.Lists.ForBoard(trinityStoriesBoard).ToList();

            var listsToScan = GetListsToScan(listsInBoard);
            var cards = new Dictionary<List,List<Card>>();
            foreach (var list in listsToScan)
            {
                var cardList = new List<Card>();
                cardList.AddRange(_trello.Cards.ForList(list));
                cards.Add(list, cardList);
            }
            return cards;
        }

        private List<List> GetListsToScan(List<List> listsInBoard)
        {
            var doneLists = listsInBoard.Where(l => DONE_LIST_NAMES.Contains(l.Name));

            var listsToScan = new List<List> (doneLists);
            listsToScan.Add(listsInBoard.Single(l=>l.Name == IN_PROGRESS_LIST_NAME));
            foreach (var listName in ExtraListNamesToScan)
            {
                listsToScan.Add(listsInBoard.Single(l => l.Name == listName));
            }
            return listsToScan;
        }
    }
}
