using System;
using System.Collections.Generic;
using System.Linq;
using TrelloNet;

namespace TrelloStats.Model
{
    public class BoardStats
    {
        private readonly TimeZoneInfo _timeZone;
        public BoardData BoardData { get; set; }
        public BoardStats(BoardData data, TimeZoneInfo timeZone)
        {
            BoardData = data;
            _timeZone = timeZone;
        }
        

        public int NumberOfCompletedCards
        {
            get
            {
                return BoardData.CardStats.Count(c => !c.IsInProgress);
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
                return LastDoneActivity.DoneAction.DateInTimeZone(_timeZone);
            }
        }

        public CardStats FirstStartedActivity
        {
            get
            {
                return BoardData.CardStats.OrderBy(cs => cs.EffectiveStartAction.Date).First();
            }
            
        }

        public CardStats LastDoneActivity
        {
            get
            {
                return BoardData.CardStats.Where(c => !c.IsInProgress).OrderByDescending(cs => cs.DoneAction.Date).First();

            }
        }
        
        public double TotalPoints
        {
            get
            {
                return BoardData.CardStats.Sum(c => c.Points);
            }
        }

        public List<WeekStats> GetWeeklyStats()
        {
            var weekStatsList = new List<WeekStats>();

            for (int week = 1; week <= CompletedWeeksElapsed +1; week++)
            {

                var startDay = BoardData.ProjectStartDate.AddDays(week * 7);
                
                //TODO: Why does this occasionally got past the current week. Occured on July 31st and august 1st.
                if (startDay > DateTime.Now)
                    continue;
                var endDay = startDay.AddDays(7);
                if(startDay.DayOfWeek != DayOfWeek.Monday || endDay.DayOfWeek != DayOfWeek.Monday)
                {

                }
                var completedCards = BoardData.CardStats.Where(c => !c.IsInProgress && c.DoneAction.DateInTimeZone(_timeZone) >= startDay && c.DoneAction.DateInTimeZone(_timeZone) < endDay);
                var inProgressCards = BoardData.CardStats.Where(c => c.IsInProgress && c.EffectiveStartAction.DateInTimeZone(_timeZone) >= startDay && c.EffectiveStartAction.DateInTimeZone(_timeZone) < endDay);
                
                WeekStats weekStats = new WeekStats() { Cards = completedCards, CardsInProgress = inProgressCards, WeekNumber = week, StartDate = startDay, EndDate = endDay };
                weekStatsList.Add(weekStats);
            }
            return weekStatsList;
        }

        public IEnumerable<CardStats> CompletedCardStats
        {
            get
            {
                return BoardData.CardStats.Where(c=> !c.IsInProgress);
            }
        }

        public double EstimatedListPoints { get; set; }

        internal BoardProjections Projections { get; set; }
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
