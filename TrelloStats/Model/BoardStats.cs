using System;
using System.Collections.Generic;
using System.Linq;

namespace TrelloStats.Model
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

        public int NumberOfCompletedCards
        {
            get
            {
                return CardStats.Count(c => !c.IsInProgress);
            }
        }

        public int CompletedWeeksElapsed
        {
            get
            {
                return (int)Math.Floor(LastDoneDate.Subtract(FirstStartDate).TotalDays / 7);
            }
        }
        
        public DateTime FirstStartDate
        {
            get
            {
                return FirstStartedActivity.EffectiveStartAction.Date;
            }
        }

        public DateTime LastDoneDate
        {
            get
            {
                return LastDoneActivity.DoneAction.Date;
            }
        }

        public CardStats FirstStartedActivity
        {
            get
            {
                return CardStats.OrderBy(cs => cs.EffectiveStartAction.Date).First();
            }
            
        }

        public CardStats LastDoneActivity
        {
            get
            {
                return CardStats.Where(c=> !c.IsInProgress).OrderByDescending(cs => cs.DoneAction.Date).First();

            }
        }

        public void AddBadCardStats(List<CardStats> badCards)
        {
            BadCardStats.AddRange(badCards);
        }

        public void AddCardStats(List<CardStats> cardStats)
        {
            CardStats.AddRange(cardStats);
        }

        public double TotalPoints
        {
            get
            {
                return CardStats.Sum(c=>c.Points);
            }
        }

        public List<WeekStats> GetWeeklyStats()
        {
            var weekStatsList = new List<WeekStats>();

            for (int week = 1; week <= CompletedWeeksElapsed; week++)
            {
                
                var startDay = ProjectStartDate.AddDays(week * 7);
                var endDay = startDay.AddDays(7);
                if(startDay.DayOfWeek != DayOfWeek.Monday || endDay.DayOfWeek != DayOfWeek.Monday)
                {

                }
                var completedCards = CardStats.Where(c => !c.IsInProgress && c.DoneAction.Date >= startDay && c.DoneAction.Date < endDay);
                var inProgressCards = CardStats.Where(c => c.IsInProgress && c.EffectiveStartAction.Date >= startDay && c.EffectiveStartAction.Date < endDay);
                
                WeekStats weekStats = new WeekStats() { Cards = completedCards, CardsInProgress = inProgressCards, WeekNumber = week, StartDate = startDay, EndDate = endDay };
                weekStatsList.Add(weekStats);
            }
            return weekStatsList;
        }
    }
  
    public class WeekStats
    {
        public IEnumerable<CardStats> Cards { get; set; }
        public IEnumerable<CardStats> CardsInProgress { get; set; }

        public int WeekNumber { get; set; }
        public int NumberOfCompletedCards
        {
            get
            {
                return Cards.Count();
            }
        }

        public int NumberOfEstimatedCards
        {
            get
            {
                return Cards.Count(c => c.WasEstimatationPriorToImplementation);
            }
        }

        public int NumberOfUnestimatedCards
        {
            get
            {
                return Cards.Count(c => c.WasImplementedPriorToEstimatation );
            }
        }

        public double PointsCompleted
        {
            get
            {
                return Cards.Sum(c => c.Points);
            }
        }

        public int GetNumberOfCardsWithLabel(string label)
        {
            return Cards.Count(c => c.Labels.Any(l => l.Name == label));
        }

        public int NumberOfCardsInProgress
        {
            get
            {
                return CardsInProgress.Count();
            }
        }
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
