using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrelloStats.Model;

namespace TrelloStats
{
    public class BoardData
    {
        public List<CardStats> CardStats { get; private set; }
        public List<CardStats> BadCardStats { get; private set; }

        public DateTime CreatedDate { get; private set; }
        public DateTime ProjectStartDate { get; set; }

        public BoardData()
        {
            CardStats = new List<CardStats>();
            BadCardStats = new List<CardStats>();
            CreatedDate = DateTime.Now;
        }

        public void AddBadCardStats(List<CardStats> badCards)
        {
            BadCardStats.AddRange(badCards);
        }

        public void AddCardStats(List<CardStats> cardStats)
        {
            CardStats.AddRange(cardStats);
        }
    }
}
