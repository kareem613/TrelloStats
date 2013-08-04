using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrelloStats.Model.Stats
{
    public class WeekStats
    {
        public IEnumerable<CardStats> Cards { get; set; }
        public IEnumerable<CardStats> CardsInProgress { get; set; }
        public IEnumerable<CardStats> CardsInTest { get; set; }

        public int WeekNumber { get; set; }
        public int NumberOfCompletedCards
        {
            get
            {
                return Cards.Count();
            }
        }

        public double PointsCompleted
        {
            get
            {
                return Cards.Sum(c => c.CardData.Points);
            }
        }

        public int GetNumberOfCardsWithLabel(string label)
        {
            return Cards.Count(c => c.CardData.Card.Labels.Any(l => l.Name == label));
        }

        public int NumberOfCardsInProgress
        {
            get
            {
                return CardsInProgress.Count();
            }
        }

        public int NumberOfCardsInTest
        {
            get
            {
                return CardsInTest.Count();
            }
        }
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
