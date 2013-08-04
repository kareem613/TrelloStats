using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.GData.Client;
using Google.GData.Spreadsheets;
using TrelloStats.Model;
using System.Configuration;

namespace TrelloStats
{
    public class GoogleService
    {
        private readonly SpreadsheetsService _service;
        
        TrelloStatsConfiguration _configuration;
        private SpreadsheetEntryFactory _spreadsheetEntryFactory;
        public GoogleService(TrelloStatsConfiguration configuration, SpreadsheetEntryFactory spreadsheetEntryFactory)
        {
            _configuration = configuration;
            _service = new SpreadsheetsService("trelloStats");
            _service.setUserCredentials(_configuration.GmailEmailAddress, _configuration.GmailPassword);
            
            _spreadsheetEntryFactory = spreadsheetEntryFactory;
            
        }

        public void PushToGoogleSpreadsheet(BoardStatsAnalysis boardStatsAnalysis)
        {
            var listFeed = GetListFeedForSpreadsheet();
            AddTitleCard(boardStatsAnalysis, listFeed);
            AddGoodCards(boardStatsAnalysis, listFeed);
            AddBadCards(boardStatsAnalysis, listFeed);
            
        }

        private ListFeed GetListFeedForSpreadsheet()
        {
            SpreadsheetQuery query = new SpreadsheetQuery();
            query.Title = _configuration.GoogleSpreadsheetName;
            SpreadsheetFeed feed = _service.Query(query);

            if (feed.Entries.Count != 1)
                throw new Exception("Did not find exactly 1 shiftmylist datasource.");

            WorksheetEntry timelineWorksheet = GetWorksheet(feed, "Data");
            var listFeed = GetListFeed(timelineWorksheet);
            return listFeed;
        }
  
        private void AddBadCards(BoardStatsAnalysis boardStatsAnalysis, ListFeed listFeed)
        {
            if (boardStatsAnalysis.BoardData.BadCardStats.Count > 0)
            {
                var errorRow = _spreadsheetEntryFactory.GetBadCardEntry(boardStatsAnalysis);
                _service.Insert(listFeed, errorRow);
            }
        }
  
        private void AddGoodCards(BoardStatsAnalysis boardStatsAnalysis, ListFeed listFeed)
        {
            foreach (var dayGroups in boardStatsAnalysis.CompletedCardStats.GroupBy(b => b.GetDoneAction().DateInTimeZone(_configuration.TimeZone).ToShortDateString()))
            {
                var dayGroupList = dayGroups.ToList();
                for (int i = 0; i < dayGroupList.Count(); i++)
                {
                    var cardStat = dayGroupList[i];
                    var minutesConfig = i * _configuration.TimelineJsOffsetMinutesPerCard;
                    var timeOffset = new TimeSpan(0, minutesConfig, 0);

                    var row = _spreadsheetEntryFactory.GetCompletedCardEntry(cardStat, timeOffset);
                    _service.Insert(listFeed, row);
                }
            }
        }
  
        private void AddTitleCard(BoardStatsAnalysis boardStatsAnalysis, ListFeed listFeed)
        {
            var titleRow = _spreadsheetEntryFactory.GetTitleCardEntry(boardStatsAnalysis);
            _service.Insert(listFeed, titleRow);
        }
  
        private void DeleteAllDataInWorksheet(ListFeed listFeed)
        {
            foreach (var row in listFeed.Entries)
            {
                row.Delete();
            }
        }
  
        private WorksheetEntry GetWorksheet(SpreadsheetFeed feed, string worksheetName)
        {
            var link = feed.Entries[0].Links.FindService(GDataSpreadsheetsNameTable.WorksheetRel, null);
            var worksheetQuery = new WorksheetQuery(link.HRef.ToString());
            worksheetQuery.Title = worksheetName;
            var worksheetFeed = _service.Query(worksheetQuery);

            if (worksheetFeed.Entries.Count != 1) throw new Exception(String.Format("Did not find exactly 1 {0} worksheet.", worksheetName));

            return (WorksheetEntry)worksheetFeed.Entries[0];
        }

        private ListFeed GetListFeed(WorksheetEntry worksheetEntry)
        {
            AtomLink listFeedLink = worksheetEntry.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);

            ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
            ListFeed listFeed = _service.Query(listQuery);
            return listFeed;
        }

        internal void ClearSpreadsheet()
        {
            var listFeed = GetListFeedForSpreadsheet();

            DeleteAllDataInWorksheet(listFeed);
        }
    }
}
