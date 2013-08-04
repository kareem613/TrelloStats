using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrelloStats
{
    public class ListNames
    {
        public string EstimatedList { get; set; }

        public string InTestListName { get; set; }

        public string InProgressListName { get; set; }

        public string[] DoneListNames { get; set; }

        public string[] StartListNames { get; set; }

        public string[] ExtraListsToInclude { get; set; }

        public string[] ExtraListsToCount { get; set; }
    }
}
