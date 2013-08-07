using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrelloNet;
using TrelloStats.Configuration;

namespace TrelloStats.Services
{
    public class TrelloClient
    {
        private readonly Trello _trello;
        private readonly TrelloStatsConfiguration _configuration;

        private readonly List<ActionType> IncludeActions = new List<ActionType>() { ActionType.CreateCard, ActionType.UpdateCard, ActionType.ConvertToCardFromCheckItem };

        public TrelloClient(TrelloStatsConfiguration configuration)
        {
            _configuration = configuration;

            _trello = new Trello(_configuration.TrelloKey);
            //var url = trello.GetAuthorizationUrl("Trello Stats", Scope.ReadOnly, Expiration.Never);
            _trello.Authorize(_configuration.TrelloToken);

            
        }
        public Board GetBoard(string boardName)
        {
            var trinityStoriesBoard = _trello.Boards.Search(boardName).Single();
            return trinityStoriesBoard;
        }

        public List<List> GetListsForBoard(Board trinityStoriesBoard)
        {
            var listsInBoard = _trello.Lists.ForBoard(trinityStoriesBoard).ToList();
            return listsInBoard;
        }

        public List<Card> GetCardsForList(List trelloList)
        {
            //return _trello.Cards.ForList(trelloList, actionIncludes: IncludeActions).ToList();
            return _trello.Cards.ForList(trelloList).ToList();
        }

        public IEnumerable<T> GetActionsForCard<T>(Card card)
        {
            //return card.Actions.OfType<T>();
            return _trello.Actions.ForCard(card, IncludeActions).OfType<T>();
        }

        public List<Action> GetActionsForCard(Card card)
        {
            //return card.Actions;
            return _trello.Actions.ForCard(card, IncludeActions).ToList();
        }

    }
}
