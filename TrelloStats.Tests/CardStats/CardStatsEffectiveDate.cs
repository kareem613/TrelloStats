using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using TrelloStats.Model.Data;
using TrelloStats.Configuration;
using NSubstitute;

namespace TrelloStats.Tests.CardStats
{
    [TestClass]
    public class CardStatsEffectiveDate
    {
        
        private IListNameConfiguration ListNameConfigStub;

        [TestInitialize]
        public void Initialize()
        {
            ListNameConfigStub = ConfigurationFactory.CreateListNamesConfigurationStub();
        }

        [TestMethod]
        public void GivenCreateAndStartActionExpectEffectiveStartActionDateIsStartAction()
        {
            var createDate = DateTime.Now;
            var createCardAction = CardActionFactory.CardAction(createDate);
            var startCardAction = CardActionFactory.UpdateCardMoveAction(createDate.AddDays(1), "ListBefore", ConfigurationFactory.DEFAULT_START_LIST_NAME);

            var actions = CardActionFactory.GetActionList(createCardAction, startCardAction);

            var cardData = new CardData() { Actions = actions };
            var cardStats = new TrelloStats.Model.Stats.CardStats() { CardData = cardData, ListNames = ListNameConfigStub };

            Assert.AreEqual(startCardAction, cardStats.EffectiveStartAction);
        }

        [TestMethod]
        public void GivenMissingStartExpectEffectiveStartActionDateIsCreateAction()
        {
            var createDate = DateTime.Now;
            var createCardAction = CardActionFactory.CardAction(createDate);
            
            var actions = CardActionFactory.GetActionList(createCardAction);

            var cardData = new CardData() { Actions = actions };
            var cardStats = new TrelloStats.Model.Stats.CardStats() { CardData = cardData, ListNames = ListNameConfigStub };

            Assert.AreEqual(createCardAction, cardStats.EffectiveStartAction);
        }

    }
}
