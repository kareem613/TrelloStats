using System;
using System.Linq;

namespace TrelloStats
{
    public class ListStats
    {
        public int CardCount { get; set; }

        public TrelloNet.List List { get; set; }
    }
}
