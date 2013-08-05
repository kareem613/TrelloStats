using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using TrelloStats.Model.Data;
using TrelloStats.Configuration;
using NSubstitute;
using TrelloNet;
using System.Linq;

namespace TrelloStats.Tests.CardStats
{
    [TestClass]
    public class DoneAction
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
        public void GivenInDoneListExpectDoneActionIsInDoneList()
        {
            var actions = CardActionFactory.GetActionsForStartedCard();
            var doneAction = CardActionFactory.UpdateCardMoveAction(actions.Last().Date.AddDays(2), ConfigurationFactory.DEFAULT_START_LIST_NAME, ConfigurationFactory.DEFAULT_DONE_LIST_NAME);
            actions.Add(doneAction);

            var listData = ListDataFactory.GetListData(ConfigurationFactory.DEFAULT_DONE_LIST_NAME);
            var cardData = new CardData() { Actions = actions };
            var cardStats = new TrelloStats.Model.Stats.CardStats() { CardData = cardData,ListData = listData, ListNames = ListNameConfigStub };

            Assert.AreEqual(doneAction, cardStats.GetDoneAction());
        }

        [TestMethod]//TODO: Is this funcationality necessary? When not in done list, you're either in test or in progress which returns null.
        public void GivenNotInDoneListExpectDoneActionIsLastAction()
        {
            var actions = CardActionFactory.GetActionsForStartedCard();
            var startAction = actions.Last();

            var listData = ListDataFactory.GetListData(ConfigurationFactory.DEFAULT_DONE_LIST_NAME);
            var cardData = new CardData() { Actions = actions };
            var cardStats = new TrelloStats.Model.Stats.CardStats() { CardData = cardData, ListData = listData, ListNames = ListNameConfigStub };

            Assert.AreEqual(startAction, cardStats.GetDoneAction());
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

        [TestMethod]
        public void GivenDoneAction3DaysAfterStartActionExpectDurationToBe3()
        {
            var expectedBusinessDaysElapsed = 3;

            var actions = CardActionFactory.GetActionsForStartedCard();
            var doneAction = CardActionFactory.UpdateCardMoveAction(actions.Last().Date.AddDays(2), ConfigurationFactory.DEFAULT_START_LIST_NAME, ConfigurationFactory.DEFAULT_DONE_LIST_NAME);
            actions.Add(doneAction);

            var listData = ListDataFactory.GetListData(ConfigurationFactory.DEFAULT_DONE_LIST_NAME);
            var cardData = new CardData() { Actions = actions };
            var cardStats = new TrelloStats.Model.Stats.CardStats() { CardData = cardData, ListData = listData, ListNames = ListNameConfigStub };

            Assert.AreEqual(expectedBusinessDaysElapsed, cardStats.BusinessDaysElapsed);
        }

        [TestMethod]
        public void GivenStartOnFridayAndDoneOnMondayExpectBusinessDaysElapsedToBe1()
        {
            var expectedBusinessDaysElapsed = 1;

            var actions = CardActionFactory.GetActionsForStartedCard(new System.DateTime(2013, 8, 9), new System.DateTime(2013, 8, 10));
            var doneAction = CardActionFactory.UpdateCardMoveAction(new System.DateTime(2013, 8, 12), ConfigurationFactory.DEFAULT_START_LIST_NAME, ConfigurationFactory.DEFAULT_DONE_LIST_NAME);
            actions.Add(doneAction);

            var listData = ListDataFactory.GetListData(ConfigurationFactory.DEFAULT_DONE_LIST_NAME);
            var cardData = new CardData() { Actions = actions };
            var cardStats = new TrelloStats.Model.Stats.CardStats() { CardData = cardData, ListData = listData, ListNames = ListNameConfigStub };

            Assert.AreEqual(expectedBusinessDaysElapsed, cardStats.BusinessDaysElapsed);
        }

        [TestMethod]
        public void GivenStartOnFridayAndDoneOnMondayExpectDurationToBe3()
        {
            var createDate = new System.DateTime(2013, 8, 8, 12, 0, 0);
            var startDate = new System.DateTime(2013, 8, 9, 12, 0, 0);
            var doneDate = new System.DateTime(2013, 8, 12, 12, 0, 0);
            var expectedTotalDaysDuration = doneDate.Subtract(startDate).TotalDays;

            var actions = CardActionFactory.GetActionsForCompletedCard(createDate, startDate, doneDate);
            var cardStats = CardStatsFactory.GetCardStats(actions);

            Assert.AreEqual(expectedTotalDaysDuration, cardStats.Duration.TotalDays);
        }

        

        

    }
}
