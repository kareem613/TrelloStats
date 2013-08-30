using System;
using System.Collections.Generic;
using System.Linq;
using TrelloStats.Configuration;
using TrelloStats.Model.Data;
using TrelloStats.Model.Stats;

namespace TrelloStats.Services
{
    public class BoardStatsService
    {
        private readonly TrelloStatsConfiguration _configuration;
        private DateTime ProjectStartDate = new DateTime(2013,6,4,14,0,0);

        public BoardStatsService(TrelloStatsConfiguration configuration)
        {
            _configuration = configuration;
        }

        public BoardStatsAnalysis BuildBoardStatsAnalysis(TrelloData trelloData, List<TimesheetData> timesheetData)
        {
            var boardStats = new BoardStats();
            BuildCardStats(trelloData, boardStats);
            boardStats.ListStats = GetListStats(trelloData.ListsToCount);
            boardStats.TimesheetData = timesheetData;
            boardStats.ProjectStartDate = ProjectStartDate;
            var boardStatsAnalysis = new BoardStatsAnalysis(_configuration, boardStats);

            boardStatsAnalysis.NextMilestoneProjection = BuildProjectionsForNextMilestone(trelloData, boardStatsAnalysis);
            boardStatsAnalysis.Projections = BuildProjections(trelloData, boardStatsAnalysis);

            if (trelloData.MilestoneList != null)
            {
                var milestones = new List<Milestone>();
                foreach (var card in trelloData.MilestoneList.CardDataCollection)
                {
                    
                    if (card.Card.Due.HasValue)
                    {
                        milestones.Add(new Milestone() { Name = card.Card.Name, TargetDate = card.Card.Due.Value });
                    }
                }
                boardStatsAnalysis.Milestones = milestones;
            }

            return boardStatsAnalysis;
        }

        private BoardProjections BuildProjections(TrelloData trelloData, BoardStatsAnalysis boardStatsAnalysis)
        {
            var estimatedPoints = GetEstimatedPointsForList(trelloData, _configuration.ListNames.EstimatedList);
            estimatedPoints += GetEstimatedPointsForList(trelloData, _configuration.ListNames.InProgressListName);
            estimatedPoints += GetEstimatedPointsForList(trelloData, _configuration.ListNames.InTestListName);

            boardStatsAnalysis.EstimatedListPoints = estimatedPoints;

            var projection = BuildBoardProjection(boardStatsAnalysis, estimatedPoints);
            return projection;
        }

        private BoardProjections BuildProjectionsForNextMilestone(TrelloData trelloData, BoardStatsAnalysis boardStatsAnalysis)
        {
            var estimatedPoints = GetEstimatedPointsNextMilestoneForList(trelloData, _configuration.ListNames.EstimatedList);
            estimatedPoints += GetEstimatedPointsNextMilestoneForList(trelloData, _configuration.ListNames.InProgressListName);
            estimatedPoints += GetEstimatedPointsNextMilestoneForList(trelloData, _configuration.ListNames.InTestListName);

            boardStatsAnalysis.EstimatedListPoints = estimatedPoints;

            var projection = BuildBoardProjection(boardStatsAnalysis, estimatedPoints);
            return projection;
        }

        private BoardProjections BuildBoardProjection(BoardStatsAnalysis boardStatsAnalysis, double estimatedPoints)
        {
            var totalDonePoints = boardStatsAnalysis.TotalPoints;
            var elapsedWeeks = boardStatsAnalysis.CompletedWeeksElapsed - _configuration.WeeksToSkipForVelocityCalculation;

            var historicalPointsPerWeek = totalDonePoints / elapsedWeeks;
            var projectedWeeksToComplete = estimatedPoints / historicalPointsPerWeek;
            var projectedWeeksMin = projectedWeeksToComplete * _configuration.TrelloProjectionsEstimateWindowLowerBoundFactor;
            var projectedWeeksMax = projectedWeeksToComplete * _configuration.TrelloProjectionsEstimateWindowUpperBoundFactor;

            var projection = new BoardProjections()
            {
                EstimatePoints = estimatedPoints,
                TotalPointsCompleted = totalDonePoints,
                elapsedWeeks = elapsedWeeks,
                historicalPointsPerWeek = historicalPointsPerWeek,
                ProjectedWeeksToCompletion = projectedWeeksToComplete,
                ProjectionCompletionDate = GetCompletionDate(projectedWeeksToComplete),
                ProjectedMinimumCompletionDate = GetCompletionDate(projectedWeeksMin),
                ProjectedMaximumCompletionDate = GetCompletionDate(projectedWeeksMax)
            };
            return projection;
        }

        private double GetEstimatedPointsForList(TrelloData trelloData, string listName)
        {
            var estimatedListData = trelloData.GetListData(listName);
            var estimatedPoints = estimatedListData.CardDataCollection.Sum(cd => cd.Points);
            return estimatedPoints;
        }

        private double GetEstimatedPointsNextMilestoneForList(TrelloData trelloData, string listName)
        {
            var estimatedListData = trelloData.GetListData(listName);
            var estimatedPoints = estimatedListData.CardDataCollection.Where(c=>c.Card.Labels.Any(l=>l.Name == "Next Milestone")).Sum(cd => cd.Points);
            return estimatedPoints;
        }

        private DateTime GetCompletionDate(double weeks)
        {
            return DateTime.Now.AddDays(weeks * 7);
        }

        private List<ListStats> GetListStats(List<ListData> listDataCollection)
        {
            var listStats = new List<ListStats>();
            foreach (var listData in listDataCollection)
            {
                var listStat = new ListStats(listData);
                listStats.Add(listStat);
            }
            return listStats;
        }

        private void BuildCardStats(TrelloData trelloData, BoardStats boardStats)
        {
            foreach (var listData in trelloData.ListDataCollection)
            {
                foreach (var cardData in listData.CardDataCollection)
                {
                    var stat = new CardStats() { CardData = cardData, ListData = listData, ListNames = _configuration.ListNames, TimeZone = _configuration.TimeZone };
                    
                    if (stat.IsComplete || stat.IsInProgress || stat.IsInTest)
                        boardStats.AddGoodCardStat(stat);
                    else
                        boardStats.AddBadCardStat(stat);
                }
            }
        }
      }
}
