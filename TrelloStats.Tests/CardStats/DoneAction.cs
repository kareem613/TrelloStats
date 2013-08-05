using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using TrelloStats.Model.Data;
using TrelloStats.Configuration;
using NSubstitute;

namespace TrelloStats.Tests.CardStats
{
    [TestClass]
    public class DoneAction
    {
        
        private IListNameConfiguration ListNameConfigStub;

        [TestInitialize]
        public void Initialize()
        {
            ListNameConfigStub = ConfigurationFactory.CreateListNamesConfigurationStub();
        }

        [TestMethod]
        public void GivenInDoneListExpectDoneActionIsInDoneList()
        {
            var createDate = DateTime.Now;
            var createCardAction = CardActionFactory.CardAction(createDate);
            var startCardAction = CardActionFactory.UpdateCardMoveAction(createDate.AddDays(1), "ListBefore", ConfigurationFactory.DEFAULT_START_LIST_NAME);
            var doneCardAction = CardActionFactory.UpdateCardMoveAction(createDate.AddDays(2), ConfigurationFactory.DEFAULT_START_LIST_NAME, ConfigurationFactory.DEFAULT_DONE_LIST_NAME);

            var actions = CardActionFactory.GetActionList(createCardAction, startCardAction, doneCardAction);

            var listData = ListDataFactory.GetListData(ConfigurationFactory.DEFAULT_DONE_LIST_NAME);
            var cardData = new CardData() { Actions = actions };
            var cardStats = new TrelloStats.Model.Stats.CardStats() { CardData = cardData,ListData = listData, ListNames = ListNameConfigStub };

            Assert.AreEqual(doneCardAction, cardStats.GetDoneAction());
        }

        [TestMethod]//TODO: Is this funcationality necessary? When not in done list, you're either in test or in progress which returns null.
        public void GivenNotInDoneListExpectDoneActionIsLastAction()
        {
            var createDate = DateTime.Now;
            var createCardAction = CardActionFactory.CardAction(createDate);
            var startCardAction = CardActionFactory.UpdateCardMoveAction(createDate.AddDays(1), "ListBefore", ConfigurationFactory.DEFAULT_START_LIST_NAME);
            

            var actions = CardActionFactory.GetActionList(createCardAction, startCardAction);

            var listData = ListDataFactory.GetListData(ConfigurationFactory.DEFAULT_DONE_LIST_NAME);
            var cardData = new CardData() { Actions = actions };
            var cardStats = new TrelloStats.Model.Stats.CardStats() { CardData = cardData, ListData = listData, ListNames = ListNameConfigStub };

            Assert.AreEqual(startCardAction, cardStats.GetDoneAction());
        }

        [TestMethod]
        public void GivenIsInProgresstExpectDoneActionIsNull()
        {
            
            var listData = ListDataFactory.GetListData(ConfigurationFactory.DEFAULT_IN_PROGRESS_LIST_NAME);
            var cardStats = new TrelloStats.Model.Stats.CardStats() { ListData = listData, ListNames = ListNameConfigStub };

            Assert.IsNull(cardStats.GetDoneAction());
        }

        [TestMethod]
        public void GivenIsInTestExpectDoneActionIsNull()
        {

            var listData = ListDataFactory.GetListData(ConfigurationFactory.DEFAULT_IN_TEST_LIST_NAME);
            var cardStats = new TrelloStats.Model.Stats.CardStats() { ListData = listData, ListNames = ListNameConfigStub };

            Assert.IsNull(cardStats.GetDoneAction());
        }

        

    }
}
