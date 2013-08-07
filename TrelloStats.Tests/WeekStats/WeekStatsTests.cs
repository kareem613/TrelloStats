using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrelloStats.Model.Stats;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrelloStats.Tests;
using TrelloStats.Configuration;

namespace TrelloStats.Tests.WeekStats
{
    [TestClass()]
    public class WeekStatsTests
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

        private List<TrelloStats.Model.Stats.CardStats> CreateCardStats(string listName, int count)
        {
            var cardStatsLists = new List<TrelloStats.Model.Stats.CardStats>();
            for (int i = 0; i < count; i++)
            {
                var actions = CardActionFactory.GetActionsForStartedCard();
                var cardStats = CardStatsFactory.GetCardStats(listName, actions);
                cardStatsLists.Add(cardStats);
            }
            return cardStatsLists;
        }

        [TestMethod()]
        public void GivenCardsInProgressListExpectMatchingNumberOfCardsInProgress()
        {
            var expectedCardCount = 9;
            var weekStats = new TrelloStats.Model.Stats.WeekStats();
            weekStats.CardsInProgress = CreateCardStats(ConfigurationFactory.DEFAULT_IN_PROGRESS_LIST_NAME, expectedCardCount);

            Assert.AreEqual(expectedCardCount, weekStats.NumberOfCardsInProgress);
        }

        [TestMethod()]
        public void GivenCardsInTestListExpectMatchingNumberOfCardsInProgress()
        {
            var expectedCardCount = 9;
            var weekStats = new TrelloStats.Model.Stats.WeekStats();
            weekStats.CardsInTest = CreateCardStats(ConfigurationFactory.DEFAULT_IN_TEST_LIST_NAME, expectedCardCount);

            Assert.AreEqual(expectedCardCount, weekStats.NumberOfCardsInTest);
        }

        [TestMethod()]
        public void GivenCardsListExpectMatchingNumberOfCards()
        {
            var expectedCardCount = 9;
            var weekStats = new TrelloStats.Model.Stats.WeekStats();
            weekStats.Cards = CreateCardStats(ConfigurationFactory.DEFAULT_IN_TEST_LIST_NAME, expectedCardCount);

            Assert.AreEqual(expectedCardCount, weekStats.NumberOfCompletedCards);
        }

        [TestMethod()]
        public void GivenCardsWithLabelListExpectMatchingNumberOfCardsWithLabel()
        {
            var expectedCardCount = 5;

            var labelName = "label name";
            var weekStats = new TrelloStats.Model.Stats.WeekStats();
            weekStats.Cards = CreateCardStats(ConfigurationFactory.DEFAULT_IN_TEST_LIST_NAME, expectedCardCount);
            weekStats.Cards.ToList().ForEach(c=>c.CardData.Card.Labels = new List<TrelloNet.Card.Label>(){ new TrelloNet.Card.Label(){Name = labelName}});

            Assert.AreEqual(expectedCardCount, weekStats.GetNumberOfCardsWithLabel(labelName));
        }

        [TestMethod()]
        public void GivenCardsWithPointsExpectTotalPointsToBeSumOfCardPoints()
        {
            var pointsPerCard = 2;
            var cardCount = 5;
            var expectedTotalPoints = pointsPerCard * cardCount;

            var weekStats = new TrelloStats.Model.Stats.WeekStats();
            weekStats.Cards = CreateCardStats(ConfigurationFactory.DEFAULT_IN_TEST_LIST_NAME, cardCount);
            weekStats.Cards.ToList().ForEach(c => c.CardData.Points = pointsPerCard);

            Assert.AreEqual(expectedTotalPoints, weekStats.PointsCompleted);
        }

    }
}
