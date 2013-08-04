﻿using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using TrelloStats.Configuration;
using TrelloStats.Model;

namespace TrelloStats.Services
{
    public class TrelloToGoogleService
    {
        private readonly GoogleService _googleService;
        private readonly TrelloService _trelloService;
        private readonly BoardStatsService _boardStatsService;
   
        public TrelloToGoogleService()
        {
            
            var config = new TrelloStatsConfiguration();
            var spreadsheetEntryFactory = new SpreadsheetEntryFactory(config);
            var trelloClient = new TrelloClient(config);

            _googleService = new GoogleService(config, spreadsheetEntryFactory);
            _trelloService = new TrelloService(config, trelloClient);
            _boardStatsService = new BoardStatsService(config);
        }

        public void CalculateStats()
        {
            var stopwatch = Stopwatch.StartNew();
            Console.Write("Querying Trello...");
            var trelloData = _trelloService.GetCardsToExamine();
            Console.WriteLine(String.Format("Completed in {0}s.", stopwatch.Elapsed.TotalSeconds));
            
            stopwatch.Restart();
            Console.Write("Calculating stats...");
            BoardStatsAnalysis boardStatsAnalysis = _boardStatsService.BuildBoardStatsAnalysis(trelloData);
            Console.WriteLine(String.Format("Completed in {0}s.", stopwatch.Elapsed.TotalSeconds));
            
            stopwatch.Restart();
            Console.Write("Deleting old records from Google...");
            _googleService.ClearSpreadsheet();
            Console.WriteLine(String.Format("Completed in {0}s.", stopwatch.Elapsed.TotalSeconds));

            stopwatch.Restart();
            Console.Write("Pushing results to Google...");
            _googleService.PushToGoogleSpreadsheet(boardStatsAnalysis);
            Console.WriteLine(String.Format("Completed in {0}s.", stopwatch.Elapsed.TotalSeconds));
        }
  
       
    }

    
}