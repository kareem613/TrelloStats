using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrelloStats
{
    public class TrelloData
    {
        public TrelloData()
        {
            ListsToStat = new List<TrelloNet.List>();
        }
        public Dictionary<TrelloNet.List, List<TrelloNet.Card>> Cards { get; set; }
        
        internal void AddListToStat(TrelloNet.List list)
        {
            ListsToStat.Add(list);
        }

        public List<TrelloNet.List> ListsToStat { get; set; }
    }
}
