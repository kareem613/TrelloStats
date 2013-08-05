using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using TrelloStats.Model.Data;
using TrelloStats.Configuration;
using NSubstitute;
using System.Linq;

namespace TrelloStats.Tests.CardStats
{
    [TestClass]
    public class EffectiveDate
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
        public void GivenCreateAndStartActionExpectEffectiveStartActionDateIsStartAction()
        {
            var actions = CardActionFactory.GetActionsForStartedCard();
            var cardStats = CardStatsFactory.GetCardStats(actions);

            var expectedStartAction = actions.Last();

            Assert.AreEqual(expectedStartAction, cardStats.EffectiveStartAction);
        }

        [TestMethod]
        public void GivenMissingStartExpectEffectiveStartActionDateIsCreateAction()
        {
            var createDate = DateTime.Now;
            var createCardAction = CardActionFactory.CardAction(createDate);
            
            var actions = CardActionFactory.GetActionList(createCardAction);

            var cardStats = CardStatsFactory.GetCardStats(actions);

            Assert.AreEqual(createCardAction, cardStats.EffectiveStartAction);
        }

    }
}
