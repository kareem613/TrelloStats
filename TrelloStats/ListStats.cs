using System;
using System.Linq;

namespace TrelloStats
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
