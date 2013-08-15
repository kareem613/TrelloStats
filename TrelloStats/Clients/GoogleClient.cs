using System;
using System.Linq;
using Google.GData.Client;
using Google.GData.Spreadsheets;
using TrelloStats.Configuration;

namespace TrelloStats.Clients
{
    public class GoogleClient
    {
        private readonly SpreadsheetsService _service;
        TrelloStatsConfiguration _configuration;

        public GoogleClient(TrelloStatsConfiguration configuration)
        {
            _configuration = configuration;
            _service = new SpreadsheetsService("trelloStats");
            _service.setUserCredentials(_configuration.GmailEmailAddress, _configuration.GmailPassword);
           
            _configuration = configuration;
        }

        public WorksheetEntry GetWorksheet(SpreadsheetFeed feed, string worksheetName)
        {
           
            var link = feed.Entries[0].Links.FindService(GDataSpreadsheetsNameTable.WorksheetRel, null);
            var worksheetQuery = new WorksheetQuery(link.HRef.ToString());
            worksheetQuery.Title = worksheetName;
            var worksheetFeed = _service.Query(worksheetQuery);

            if (worksheetFeed.Entries.Count != 1) throw new Exception(String.Format("Did not find exactly 1 {0} worksheet.", worksheetName));

            return (WorksheetEntry)worksheetFeed.Entries[0];
        }

        public ListFeed GetListFeed(WorksheetEntry worksheetEntry)
        {
            AtomLink listFeedLink = worksheetEntry.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);

            ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
            ListFeed listFeed = _service.Query(listQuery);
            return listFeed;
        }

        public ListFeed GetListFeedForSpreadsheet(string spreadsheetName)
        {
            SpreadsheetQuery query = new SpreadsheetQuery();
            query.Title = spreadsheetName;
            SpreadsheetFeed feed = _service.Query(query);

            if (feed.Entries.Count != 1)
                throw new Exception("Did not find exactly 1 shiftmylist datasource.");

            WorksheetEntry timelineWorksheet = GetWorksheet(feed, "Data");
            var listFeed = GetListFeed(timelineWorksheet);
            return listFeed;
        }

        public void ClearSpreadsheet(string spreadsheetName)
        {
            var listFeed = GetListFeedForSpreadsheet(spreadsheetName);

            DeleteAllDataInWorksheet(listFeed);
        }

        private void DeleteAllDataInWorksheet(ListFeed listFeed)
        {
            foreach (var row in listFeed.Entries)
            {
                row.Delete();
            }
        }


        public void Insert(ListFeed listFeed, ListEntry row)
        {
            _service.Insert(listFeed, row);
        }
    }
}
