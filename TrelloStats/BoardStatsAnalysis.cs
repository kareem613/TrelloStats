using System;
using System.Collections.Generic;
using System.Linq;
using TrelloNet;
using TrelloStats.Model;
using TrelloStats.Model.Stats;

namespace TrelloStats
{
    public class BoardStatsAnalysis
    {
        private readonly TimeZoneInfo _timeZone;
        public BoardStats BoardStats { get; set; }
        public BoardStatsAnalysis(BoardStats boardStats, TimeZoneInfo timeZone)
        {
            BoardStats = boardStats;
            _timeZone = timeZone;
        }
        

        public int NumberOfCompletedCards
        {
            get
            {
                return BoardStats.CardStats.Count(c => !c.IsInProgress && !c.IsInTest);
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
                return FirstStartedActivity.EffectiveStartAction.DateInTimeZone(_timeZone);
            }
        }

        public DateTime LastDoneDate
        {
            get
            {
                return LastDoneActivity.GetDoneAction().DateInTimeZone(_timeZone);
            }
        }

        public CardStats FirstStartedActivity
        {
            get
            {
                return BoardStats.CardStats.OrderBy(cs => cs.EffectiveStartAction.Date).First();
            }
            
        }

        public CardStats LastDoneActivity
        {
            get
            {
                return BoardStats.CardStats.Where(c => !c.IsInProgress && !c.IsInTest).OrderByDescending(cs => cs.GetDoneAction().Date).First();

            }
        }
        
        public double TotalPoints
        {
            get
            {
                return BoardStats.CardStats.Sum(c => c.CardData.Points);
            }
        }

        public List<WeekStats> GetWeeklyStats()
        {
            var weekStatsList = new List<WeekStats>();

            for (int week = 1; week <= CompletedWeeksElapsed +1; week++)
            {

                var startDay = BoardStats.ProjectStartDate.AddDays(week * 7);
                
                //TODO: Why does this occasionally go past the current week. Occured on July 31st and august 1st.
                if (startDay > DateTime.Now)
                    continue;
                var endDay = startDay.AddDays(7);
                if(startDay.DayOfWeek != DayOfWeek.Monday || endDay.DayOfWeek != DayOfWeek.Monday)
                {

                }
                var completedCards = BoardStats.CardStats.Where(c => !c.IsInProgress && !c.IsInTest && c.GetDoneAction().DateInTimeZone(_timeZone) >= startDay && c.GetDoneAction().DateInTimeZone(_timeZone) < endDay);
                var inProgressCards = BoardStats.CardStats.Where(c => c.IsInProgress && c.EffectiveStartAction.DateInTimeZone(_timeZone) >= startDay && c.EffectiveStartAction.DateInTimeZone(_timeZone) < endDay);
                var inTestCards = BoardStats.CardStats.Where(c => c.IsInTest && c.EffectiveStartAction.DateInTimeZone(_timeZone) >= startDay && c.EffectiveStartAction.DateInTimeZone(_timeZone) < endDay);
                
                WeekStats weekStats = new WeekStats() { Cards = completedCards, CardsInProgress = inProgressCards,CardsInTest = inTestCards, WeekNumber = week, StartDate = startDay, EndDate = endDay };
                weekStatsList.Add(weekStats);
            }
            return weekStatsList;
        }

        public IEnumerable<CardStats> CompletedCardStats
        {
            get
            {
                return BoardStats.CardStats.Where(c => !c.IsInProgress && !c.IsInTest);
            }
        }

        public double EstimatedListPoints { get; set; }

        internal BoardProjections Projections { get; set; }
    }
  
    public class WeekStats
    {
        public IEnumerable<CardStats> Cards { get; set; }
        public IEnumerable<CardStats> CardsInProgress { get; set; }
        public IEnumerable<CardStats> CardsInTest{ get; set; }

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
