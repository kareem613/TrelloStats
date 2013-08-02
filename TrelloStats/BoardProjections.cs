using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrelloStats
{
    class BoardProjections
    {
        public double EstimatePoints { get; set; }

        public double TotalPointsCompleted { get; set; }

        public int elapsedWeeks { get; set; }

        public double historicalPointsPerWeek { get; set; }

        public double ProjectedWeeksToCompletion { get; set; }
    }
}
