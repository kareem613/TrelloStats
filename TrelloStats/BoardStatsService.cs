using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TrelloNet;
using TrelloStats.Model;

namespace TrelloStats
{
    public class BoardStatsService
    {
        private readonly TrelloStatsConfiguration _configuration;
        private DateTime ProjectStartDate = new DateTime(2013,6,4,14,0,0);

        public double EstimateWindowLowerBoundFactor { get; set; }
        public double EstimateWindowUpperBoundFactor { get; set; }
        public int WeeksToSkipForVelocityCalculation { get; set; }

        public BoardStatsService(TrelloStatsConfiguration configuration)
        {
            _configuration = configuration;
        
            EstimateWindowLowerBoundFactor = 0.5;
            EstimateWindowUpperBoundFactor= 1.5;
        }

        public BoardStats BuildBoardStats(TrelloData trelloData)
        {
            var cardStats = new List<CardStats>();
            var badCards = new List<CardStats>();

            BuildCardStats(trelloData.ListDataCollection, badCards, cardStats);

            var listStats = new List<ListStats>();
            BuildListStats(trelloData.ListsToCount, listStats);
            
            

            var boardData = new BoardData();
            boardData.ProjectStartDate = ProjectStartDate;
            boardData.AddCardStats(cardStats);
            boardData.AddBadCardStats(badCards);
            boardData.ListStats = listStats;

            var estimatedListData = trelloData.GetListData(_configuration.ListNames.EstimatedList);
            var estimatedPoints = estimatedListData.CardDataCollection.Sum(cd => cd.Points);

            var boardStats = new BoardStats(boardData, _configuration.TimeZone);
            boardStats.EstimatedListPoints = estimatedPoints;

            BuildProjections(boardStats);


            return boardStats;
        }

        private void BuildProjections(BoardStats boardStats)
        {
            var estimatedPoints = boardStats.EstimatedListPoints;
            var totalDonePoints = boardStats.TotalPoints;
            var elapsedWeeks = boardStats.CompletedWeeksElapsed - WeeksToSkipForVelocityCalculation;

            var historicalPointsPerWeek = totalDonePoints / elapsedWeeks;
            var projectedWeeksToComplete = estimatedPoints / historicalPointsPerWeek;
            var projectedWeeksMin = projectedWeeksToComplete * EstimateWindowLowerBoundFactor;
            var projectedWeeksMax = projectedWeeksToComplete * EstimateWindowUpperBoundFactor;

            boardStats.Projections = new BoardProjections()
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
        }

        private DateTime GetCompletionDate(double weeks)
        {
            return DateTime.Now.AddDays(weeks * 7);
        }

        private void BuildListStats(List<ListData> listDataCollection, List<ListStats> listStats)
        {
            foreach (var listData in listDataCollection)
            {
                var listStat = new ListStats(listData);
                listStats.Add(listStat);
            }
        }

        private void BuildCardStats(List<ListData> listDataCollection, List<CardStats> badCards, List<CardStats> cardStats)
        {
            foreach (var listData in listDataCollection)
            {
                foreach (var cardData in listData.CardDataCollection)
                {
                    var stat = new CardStats() { CardData = cardData, ListData = listData, ListNames = _configuration.ListNames, TimeZone = _configuration.TimeZone };
                    
                    if (stat.IsComplete || stat.IsInProgress || stat.IsInTest)
                        cardStats.Add(stat);
                    else
                        badCards.Add(stat);
                }
            }
        }
      }
}
