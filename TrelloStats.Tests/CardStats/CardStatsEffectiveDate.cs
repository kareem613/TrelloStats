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
        private const string START_LIST_NAME = "Start";
        private IListNameConfiguration ListNameConfigStub;

        [TestInitialize]
        public void Initialize()
        {
            ListNameConfigStub = CreateListNamesConfigurationStub();
        }

        [TestMethod]
        public void GivenCreateAndStartActionExpectEffectiveStartActionDateIsStartAction()
        {
            var createDate = DateTime.Now;
            var createCardAction = CardActionFactory.CardAction(createDate);
            var startCardAction = CardActionFactory.UpdateCardMoveAction(createDate.AddDays(1), "ListBefore", START_LIST_NAME);

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

        #region Helpers
        private static IListNameConfiguration CreateListNamesConfigurationStub()
        {
            var configStub = Substitute.For<IListNameConfiguration>();
            configStub.StartListNames.Returns(new string[] { START_LIST_NAME });
            return configStub;
        }

        #endregion


    }
}
