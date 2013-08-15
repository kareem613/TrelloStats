using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrelloNet;

namespace TrelloStats.Tests
{
    public class CardActionFactory
    {
        
        public static TrelloNet.UpdateCardMoveAction UpdateCardMoveAction(System.DateTime date, string listBeforeName, string listAfterName)
        {
            return new TrelloNet.UpdateCardMoveAction() { Date = date, Data = new TrelloNet.UpdateCardMoveAction.ActionData() { ListAfter = new TrelloNet.ListName() { Name = listAfterName }, ListBefore = new TrelloNet.ListName() { Name = listBeforeName } } };
        }

        public static TrelloNet.CreateCardAction CardAction(System.DateTime createDate)
        {
            var createCardAction = new TrelloNet.CreateCardAction() { Date = createDate };
            createCardAction.Data = new CreateCardAction.ActionData();
            createCardAction.Data.List = new ListName() { Name = "dummy list name" };
            return createCardAction;
        }

        public static List<TrelloNet.Action> GetActionList(params Action[] actions)
        {
            return new List<TrelloNet.Action>(actions);
        }

        public static List<Action> GetActionsForStartedCard(System.DateTime createDate, System.DateTime startDate)
        {
            var createCardAction = CardActionFactory.CardAction(createDate);
            var startCardAction = CardActionFactory.UpdateCardMoveAction(startDate, "ListBefore", ConfigurationFactory.DEFAULT_START_LIST_NAME);

            return GetActionList(createCardAction, startCardAction);
        }

        public static List<Action> GetActionsForStartedCard()
        {
            var createDate = System.DateTime.Now;
            return GetActionsForStartedCard(createDate, createDate.AddDays(1));
           
        }

        public static List<Action> GetActionsForCompletedCard(System.DateTime createDate, System.DateTime startDate, System.DateTime doneDate)
        {
            var actions = CardActionFactory.GetActionsForStartedCard(createDate, startDate);
            var doneAction = CardActionFactory.UpdateCardMoveAction(doneDate, ConfigurationFactory.DEFAULT_START_LIST_NAME, ConfigurationFactory.DEFAULT_DONE_LIST_NAME);
            actions.Add(doneAction);
            return actions;
        }

        public static List<Action> GetActionsForCompletedCard()
        {
            var createDate = System.DateTime.Now;
            return GetActionsForCompletedCard(createDate, createDate.AddDays(1), createDate.AddDays(2));

        }
    }
}
