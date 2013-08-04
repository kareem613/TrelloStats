using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrelloNet;

namespace TrelloStats.Model.Data
{
    public class CardData
    {
        public Card Card { get; set; }
        public double Points { get; set; }
        public string Name { get; set; }

        public List<Action> Actions { get; set; }
    }
}
