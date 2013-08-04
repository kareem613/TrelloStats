using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrelloStats
{
    public class TrelloData
    {
        private List<ListData> ListData;
        private List<ListData> ListDataToCount;

        public TrelloData()
        {
            ListData = new List<ListData>();
            ListDataToCount = new List<ListData>();
        }
        
        internal void AddListData(ListData listData)
        {
            ListData.Add(listData);
        }

        internal void AddListToCount(ListData listData)
        {
            ListDataToCount.Add(listData);
        }

        public List<ListData> ListDataCollection
        {
            get
            {
                return ListData;
            }
        }

        public List<ListData> ListsToCount
        {
            get
            {
                return ListDataToCount;
            }
        }

        internal ListData GetListData(string listName)
        {
            return ListData.SingleOrDefault(ld => ld.List.Name == listName) ?? ListDataToCount.SingleOrDefault(ld => ld.List.Name == listName);
        }
    }
}
