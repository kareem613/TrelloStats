﻿using System;
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

        public List<ListStats> ListStats { get; set; }

        internal void AddGoodCardStat(Model.CardStats stat)
        {
            CardStats.Add(stat);
        }

        internal void AddBadCardStat(Model.CardStats stat)
        {
            BadCardStats.Add(stat);
        }
    }
}
