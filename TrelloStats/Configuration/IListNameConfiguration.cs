using System;
namespace TrelloStats.Configuration
{
    public interface IListNameConfiguration
    {
        string[] DoneListNames { get; }
        string EstimatedList { get; }
        string[] ExtraListsToCount { get; }
        string[] ExtraListsToInclude { get; }
        string InProgressListName { get; }
        string InTestListName { get; }
        string[] StartListNames { get; }
    }
}
