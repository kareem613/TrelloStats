using System;
using System.Linq;
using TrelloStats.Model.Data;

namespace TrelloStats.Model.Stats
{
    public class ListStats
    {
        public ListData ListData;

        public ListStats(ListData listData)
        {
            ListData = listData;
        }
        public int CardCount
        {
            get
            {
                return ListData.CardDataCollection.Count();
            }
        }

    }
}
