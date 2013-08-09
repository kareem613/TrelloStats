using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrelloStats.Model.Stats;
using TrelloStats.Configuration;
using System.Collections.Generic;

namespace TrelloStats.Tests.BoardStatsTests
{
    [TestClass]
    public class BoardStatsTests
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
        public void GivenOneAddedCardExpectGoodCardCountIsOne()
        {
            var boardStats = new BoardStats();
            boardStats.AddGoodCardStat(CardStatsFactory.GetCardStats(new List<TrelloNet.Action>()));

            Assert.AreEqual(1, boardStats.CardStats.Count);
        }

        [TestMethod]
        public void GivenOneAddedBadCardExpectBadCardCountIsOne()
        {
            var boardStats = new BoardStats();
            boardStats.AddBadCardStat(CardStatsFactory.GetCardStats(new List<TrelloNet.Action>()));

            Assert.AreEqual(1, boardStats.BadCardStats.Count);
        }
    }
}
