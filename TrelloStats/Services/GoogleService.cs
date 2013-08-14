using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.GData.Client;
using Google.GData.Spreadsheets;
using TrelloStats.Model;
using System.Configuration;
using TrelloStats.Configuration;

namespace TrelloStats.Services
{
    public class GoogleService
    {
        private readonly GoogleClient _googleClient;
        private readonly TrelloStatsConfiguration _configuration;

        private SpreadsheetEntryFactory _spreadsheetEntryFactory;
        
        public GoogleService(TrelloStatsConfiguration configuration, SpreadsheetEntryFactory spreadsheetEntryFactory, GoogleClient googleClient)
        {
            _configuration = configuration;
            _googleClient = googleClient;
            
            _spreadsheetEntryFactory = spreadsheetEntryFactory;
            
        }

        public void PushToGoogleSpreadsheet(BoardStatsAnalysis boardStatsAnalysis)
        {
            var listFeed = _googleClient.GetListFeedForSpreadsheet();
            AddTitleCard(boardStatsAnalysis, listFeed);
            AddGoodCards(boardStatsAnalysis, listFeed);
            AddBadCards(boardStatsAnalysis, listFeed);
            
        }

        private void AddBadCards(BoardStatsAnalysis boardStatsAnalysis, ListFeed listFeed)
        {
            if (boardStatsAnalysis.BoardStats.BadCardStats.Count > 0)
            {
                var errorRow = _spreadsheetEntryFactory.GetBadCardEntry(boardStatsAnalysis);
                _googleClient.Insert(listFeed, errorRow);
            }
        }
  
        private void AddGoodCards(BoardStatsAnalysis boardStatsAnalysis, ListFeed listFeed)
        {
            foreach (var dayGroups in boardStatsAnalysis.CompletedCardStats.GroupBy(b => b.DoneAction.DateInTimeZone(_configuration.TimeZone).ToShortDateString()))
            {
                var dayGroupList = dayGroups.ToList();
                for (int i = 0; i < dayGroupList.Count(); i++)
                {
                    var cardStat = dayGroupList[i];
                    var minutesConfig = i * _configuration.TimelineJsOffsetMinutesPerCard;
                    var timeOffset = new TimeSpan(0, minutesConfig, 0);

                    var row = _spreadsheetEntryFactory.GetCompletedCardEntry(cardStat, timeOffset);
                    _googleClient.Insert(listFeed, row);
                }
            }
        }
  
        private void AddTitleCard(BoardStatsAnalysis boardStatsAnalysis, ListFeed listFeed)
        {
            var titleRow = _spreadsheetEntryFactory.GetTitleCardEntry(boardStatsAnalysis);
            _googleClient.Insert(listFeed, titleRow);
        }
  
       
       
    }
}
