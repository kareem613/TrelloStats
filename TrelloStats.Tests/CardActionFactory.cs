using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrelloStats.Tests
{
    public class CardActionFactory
    {
        
        public static TrelloNet.UpdateCardMoveAction UpdateCardMoveAction(DateTime date, string listBeforeName, string listAfterName)
        {
            var startCardAction = new TrelloNet.UpdateCardMoveAction() { Date = date, Data = new TrelloNet.UpdateCardMoveAction.ActionData() { ListAfter = new TrelloNet.ListName() { Name = listAfterName }, ListBefore = new TrelloNet.ListName() { Name = listBeforeName } } };
            return startCardAction;
        }

        public static TrelloNet.CreateCardAction CardAction(DateTime createDate)
        {
            var createCardAction = new TrelloNet.CreateCardAction() { Date = createDate };
            return createCardAction;
        }

        public static List<TrelloNet.Action> GetActionList(params TrelloNet.Action[] actions)
        {
            return new List<TrelloNet.Action>(actions);
        }
    }
}
