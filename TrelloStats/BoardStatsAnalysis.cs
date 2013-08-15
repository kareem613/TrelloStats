using System;
using System.Collections.Generic;
using System.Linq;
using TrelloNet;
using TrelloStats.Configuration;
using TrelloStats.Model;
using TrelloStats.Model.Stats;

namespace TrelloStats
{
    public class BoardStatsAnalysis
    {
        private readonly TrelloStatsConfiguration _configuration;
        public BoardStats BoardStats { get; set; }
        public BoardStatsAnalysis(TrelloStatsConfiguration configuration, BoardStats boardStats)
        {
            BoardStats = boardStats;
            _configuration = configuration;
            Milestones = new List<Milestone>();
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
                return FirstStartedActivity.EffectiveStartAction.DateInTimeZone(_configuration.TimeZone);
            }
        }

        public DateTime LastDoneDate
        {
            get
            {
                return LastDoneActivity.DoneAction.DateInTimeZone(_configuration.TimeZone);
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
                return BoardStats.CardStats.Where(c => !c.IsInProgress && !c.IsInTest).OrderByDescending(cs => cs.DoneAction.Date).First();

            }
        }
        
        public double TotalPoints
        {
            get
            {
                var doneCards = BoardStats.CardStats.Where(c=>_configuration.ListNames.DoneListNames.Contains(c.ListData.List.Name));
                var totalPoints = doneCards.Sum(cs => cs.CardData.Points);
                return totalPoints;
            }
        }

        public List<WeekStats> WeekStats
        {
            get
            {
                return GetWeeklyStats();
            }
        }

        //public void GetTotalCompletedPointsAsOfDate(DateTime date)
        //{
        //    var points = BoardStats.CardStats.Where(c => c.DoneAction.Date < date).Sum(c => c.CardData.Points);
        //}

        private List<WeekStats> GetWeeklyStats()
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
                var completedCards = BoardStats.CardStats.Where(c => !c.IsInProgress && !c.IsInTest && c.DoneAction.DateInTimeZone(_configuration.TimeZone) >= startDay && c.DoneAction.DateInTimeZone(_configuration.TimeZone) < endDay);
                var inProgressCards = BoardStats.CardStats.Where(c => c.IsInProgress && c.EffectiveStartAction.DateInTimeZone(_configuration.TimeZone) >= startDay && c.EffectiveStartAction.DateInTimeZone(_configuration.TimeZone) < endDay);
                var inTestCards = BoardStats.CardStats.Where(c => c.IsInTest && c.EffectiveStartAction.DateInTimeZone(_configuration.TimeZone) >= startDay && c.EffectiveStartAction.DateInTimeZone(_configuration.TimeZone) < endDay);
                var timesheetEntries = BoardStats.TimesheetData.Where(t => t.Week == week && t.Category != _configuration.GoogleTimesheetsExcludeCategoryFromTotalHours).ToList();
                var excludedTimesheetEntries = BoardStats.TimesheetData.Where(t => t.Week == week && t.Category == _configuration.GoogleTimesheetsExcludeCategoryFromTotalHours).ToList();

                WeekStats weekStats = new WeekStats() { Cards = completedCards, CardsInProgress = inProgressCards, CardsInTest = inTestCards, WeekNumber = week, StartDate = startDay, EndDate = endDay, TimesheetEntries = timesheetEntries, ExcludedTimesheetEntries = excludedTimesheetEntries };
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

        public List<Milestone> Milestones { get; set; }
    }
    
}
