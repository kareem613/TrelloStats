using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrelloStats.Model;

namespace TrelloStats.Model.Stats
{
    public class BoardStats
    {
        public List<CardStats> CardStats { get; private set; }
        public List<CardStats> BadCardStats { get; private set; }

        public DateTime CreatedDate { get; private set; }
        public DateTime ProjectStartDate { get; set; }

        public BoardStats()
        {
            CardStats = new List<CardStats>();
            BadCardStats = new List<CardStats>();
            CreatedDate = DateTime.Now;
        }

        public List<ListStats> ListStats { get; set; }

        public void AddGoodCardStat(CardStats stat)
        {
            CardStats.Add(stat);
        }

        public void AddBadCardStat(CardStats stat)
        {
            BadCardStats.Add(stat);
        }
    }
}
