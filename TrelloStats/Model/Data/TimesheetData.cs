using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrelloStats.Model.Data
{
    public class TimesheetData
    {
        public int Week { get; set; }

        public string Category { get; set; }

        public string Project { get; set; }

        public double Hours { get; set; }
    }
}
