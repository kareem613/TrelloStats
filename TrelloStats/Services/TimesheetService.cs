using Google.GData.Spreadsheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrelloStats.Clients;
using TrelloStats.Configuration;
using TrelloStats.Model.Data;

namespace TrelloStats.Services
{
    

    public class TimesheetService
    {
        private readonly GoogleClient _googleClient;
        private readonly TrelloStatsConfiguration _configuration;
        
        public TimesheetService(TrelloStatsConfiguration configuration, GoogleClient googleClient)
        {
            _configuration = configuration;
            _googleClient = googleClient;
        }

        public List<TimesheetData> GetTimesheetData()
        {
            ListFeed listFeed = _googleClient.GetListFeedForSpreadsheet(_configuration.GoogleTimesheetsSpreadsheetName);
            List<TimesheetData> data = new List<TimesheetData>();
            foreach (ListEntry entry in listFeed.Entries)
            {
                int week = GetValueInt(entry.Elements[0].Value);
                string category = entry.Elements[1].Value;
                string project = entry.Elements[2].Value;
                double hours = GetValueDouble(entry.Elements[3].Value);

                var timesheetData = new TimesheetData();
                timesheetData.Week = week;
                timesheetData.Category = category;
                timesheetData.Project = project;
                timesheetData.Hours = hours;

                data.Add(timesheetData);
            }
            return data;
        }

        private double GetValueDouble(string valueString)
        {
            //TODO: Make robust / exception handle
            return double.Parse(valueString);
        }

        private int GetValueInt(string valueString)
        {
            //TODO: Make robust / exception handle
            return int.Parse(valueString);
        }
    }
}
