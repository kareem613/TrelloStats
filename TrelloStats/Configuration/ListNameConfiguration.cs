using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace TrelloStats.Configuration
{
    public class ListNameConfiguration: TypeConfigurationManager, TrelloStats.Configuration.IListNameConfiguration
    {
        public string InProgressListName
        {
            get
            {
                return GetAppConfig("Trello.ListNames.InProgress");
            }
        }

        public string InTestListName
        {
            get
            {
                return GetAppConfig("Trello.ListNames.InTest");
            }
        }

        public string[] StartListNames
        {
            get
            {
                return GetAppSettingAsArray("Trello.ListNames.StartNames");
            }
        }

        public string[] DoneListNames
        {
            get
            {
                return GetAppSettingAsArray("Trello.ListNames.CompletedNames");
            }
        }

        public string[] ExtraListsToInclude
        {
            get
            {
                return GetAppSettingAsArray("Trello.ListNames.ExtraListsToInclude");
            }
        }

        public string[] ExtraListsToCount
        {
            get
            {
                return GetAppSettingAsArray("Trello.ListNames.ExtraListsToCount");
            }
        }

        public string EstimatedList
        {
            get
            {
                return ConfigurationManager.AppSettings["Trello.Projections.EstimatedList"];
            }
        }

        public string MilestoneList
        {
            get
            {
                return ConfigurationManager.AppSettings["Trello.ListNames.MilestoneList"];
            }
        }
    }
}
