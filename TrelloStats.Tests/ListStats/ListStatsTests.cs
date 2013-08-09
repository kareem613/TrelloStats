using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrelloStats.Model.Stats;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TrelloStats.Tests.ListStats
{
    [TestClass()]
    public class ListStatsTests
    {
        [TestMethod()]
        public void ListStatsTest()
        {
            var expectedCardDataCount = 2;

            var listData = ListDataFactory.GetListData(ConfigurationFactory.DEFAULT_DONE_LIST_NAME);
            listData.AddCardData(new Model.Data.CardData());
            listData.AddCardData(new Model.Data.CardData());

            var listStats = new TrelloStats.Model.Stats.ListStats(listData);

            Assert.AreEqual(expectedCardDataCount, listStats.CardCount);
        }
    }
}
