using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrelloNet;
using TrelloStats.Configuration;
using TrelloStats.Model.Data;

namespace TrelloStats.Tests
{
    public class CardStatsFactory
    {
        private IListNameConfiguration ListNameConfigStub;
        private ITrelloStatsConfiguration TrelloStatsConfigStub;

        public CardStatsFactory(IListNameConfiguration listNameConfig, ITrelloStatsConfiguration trelloStatsConfig)
        {
            ListNameConfigStub = listNameConfig;
            TrelloStatsConfigStub = trelloStatsConfig;
        }
        public Model.Stats.CardStats GetCardStats(string listName, List<Action> actions)
        {
            var listData = ListDataFactory.GetListData(listName);
            var cardData = new CardData() { Actions = actions, Card = new Card() };
            var cardStats = new TrelloStats.Model.Stats.CardStats() { CardData = cardData, ListData = listData, ListNames = ListNameConfigStub, TimeZone = TrelloStatsConfigStub.TimeZone };
            return cardStats;
        }
    }
}
