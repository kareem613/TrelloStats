using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrelloStats.Configuration;
using TrelloStats.Model.Data;

namespace TrelloStats.Tests
{
    [TestClass]
    public class CardStatsState
    {
        private IListNameConfiguration ListNameConfigStub;
        private ITrelloStatsConfiguration TrelloStatsConfigStub;
        private CardStatsFactory CardStatsFactory;

        [TestInitialize]
        public void Initialize()
        {
            ListNameConfigStub = ConfigurationFactory.CreateListNamesConfigurationStub();
            TrelloStatsConfigStub = ConfigurationFactory.CreateConfigurationStub();
            CardStatsFactory = new CardStatsFactory(ListNameConfigStub, TrelloStatsConfigStub);
        }

        [TestMethod]
        public void GivenInProgressListExpectIsInProgress()
        {
            var listData = ListDataFactory.GetListData(ConfigurationFactory.DEFAULT_IN_PROGRESS_LIST_NAME);
            var cardStats = new TrelloStats.Model.Stats.CardStats() { ListData = listData, ListNames = ListNameConfigStub };

            Assert.IsTrue(cardStats.IsInProgress);
            Assert.IsFalse(cardStats.IsInTest);
        }

        [TestMethod]
        public void GivenInTestListExpectIsInTest()
        {
            var listData = ListDataFactory.GetListData(ConfigurationFactory.DEFAULT_IN_TEST_LIST_NAME);
            var cardStats = new TrelloStats.Model.Stats.CardStats() { ListData = listData, ListNames = ListNameConfigStub };

            Assert.IsFalse(cardStats.IsInProgress);
            Assert.IsTrue(cardStats.IsInTest);
        }

        [TestMethod]
        public void GivenMinimumDataExpectIsComplete()
        {
            var actions = CardActionFactory.GetActionsForCompletedCard();
            var cardStats = CardStatsFactory.GetCardStats(ConfigurationFactory.DEFAULT_DONE_LIST_NAME, actions);

            Assert.IsTrue(cardStats.IsComplete);
        }

        [TestMethod]
        public void GivenMissingCardExpectIsCompleteFalse()
        {
            var actions = CardActionFactory.GetActionsForCompletedCard();
            var cardStats = CardStatsFactory.GetCardStats(ConfigurationFactory.DEFAULT_DONE_LIST_NAME, actions);
            cardStats.CardData.Card = null;
            
            Assert.IsFalse(cardStats.IsComplete);
        }

        [TestMethod]
        public void GivenMissingDoneActionExpectIsCompleteFalse()
        {
            var actions = CardActionFactory.GetActionsForStartedCard();
            var cardStats = CardStatsFactory.GetCardStats(ConfigurationFactory.DEFAULT_IN_PROGRESS_LIST_NAME, actions);

            Assert.IsFalse(cardStats.IsComplete);
        }
    }
}
