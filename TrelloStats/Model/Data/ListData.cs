using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrelloNet;

namespace TrelloStats.Model.Data
{
    public class ListData
    {
        public List List { get; set; }

        public List<CardData> CardDataCollection { get; set; }

        public ListData(List list)
        {
            this.List = list;
            CardDataCollection = new List<CardData>();
        }



        public void AddCardData(CardData cardData)
        {
            CardDataCollection.Add(cardData);
        }
    }
}
