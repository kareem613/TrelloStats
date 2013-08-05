using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrelloStats.Configuration;
using TrelloStats.Model.Data;

namespace TrelloStats.Tests
{
    [TestClass]
    public class CardStatsState
    {
        private IListNameConfiguration ListNameConfigStub;

        [TestInitialize]
        public void Initialize()
        {
            ListNameConfigStub = ConfigurationFactory.CreateListNamesConfigurationStub();
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
    }
}
