﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TrelloNet;
using TrelloStats.Configuration;
using TrelloStats.Model;
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

        public BoardStatsAnalysis BuildBoardStatsAnalysis(TrelloData trelloData)
        {
            var boardStats = new BoardStats();
            BuildCardStats(trelloData, boardStats);
            boardStats.ListStats = GetListStats(trelloData.ListsToCount);
            
            boardStats.ProjectStartDate = ProjectStartDate;
            var boardStatsAnalysis = new BoardStatsAnalysis(boardStats, _configuration.TimeZone);
            
            BuildProjections(trelloData, boardStatsAnalysis);

            return boardStatsAnalysis;
        }

        private void BuildProjections(TrelloData trelloData, BoardStatsAnalysis boardStatsAnalysis)
        {
            var estimatedListData = trelloData.GetListData(_configuration.ListNames.EstimatedList);
            var estimatedPoints = estimatedListData.CardDataCollection.Sum(cd => cd.Points);

            boardStatsAnalysis.EstimatedListPoints = estimatedPoints;
            
            var totalDonePoints = boardStatsAnalysis.TotalPoints;
            var elapsedWeeks = boardStatsAnalysis.CompletedWeeksElapsed - _configuration.WeeksToSkipForVelocityCalculation;

            var historicalPointsPerWeek = totalDonePoints / elapsedWeeks;
            var projectedWeeksToComplete = estimatedPoints / historicalPointsPerWeek;
            var projectedWeeksMin = projectedWeeksToComplete * _configuration.TrelloProjectionsEstimateWindowLowerBoundFactor;
            var projectedWeeksMax = projectedWeeksToComplete * _configuration.TrelloProjectionsEstimateWindowUpperBoundFactor;

            boardStatsAnalysis.Projections = new BoardProjections()
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