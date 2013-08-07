using System;
using System.Collections.Generic;
using TrelloNet;
using System.Linq;
using TrelloStats.Model.Data;
using TrelloStats.Configuration;

namespace TrelloStats.Model.Stats
{
    public class CardStats
    {
        public CardData CardData { get; set; }
        public ListData ListData { get; set; }
        public IEnumerable<TrelloNet.Action> Actions
        {
            get { return CardData.Actions; }
        }

        //TODO:// Move this config out of this stats class
        public IListNameConfiguration ListNames { get; set; }

        public TimeZoneInfo TimeZone { get; set; }

        public bool IsInProgress
        {
            get
            {
                return ListData.List.Name == ListNames.InProgressListName;
            }
            
        }

        public bool IsInTest
        {
            get
            {
                return ListData.List.Name == ListNames.InTestListName;
            }

        }

        public TrelloNet.Action CreateAction
        {
            get
            {
                TrelloNet.Action action = Actions.OfType<CreateCardAction>().SingleOrDefault();
                if (action == null)
                    action = Actions.OfType<ConvertToCardFromCheckItemAction>().First();
                return action;
            }
        }

        public TrelloNet.Action EffectiveStartAction
        {
            get
            {
                var action = GetStartAction();
                if(action == null)
                    return CreateAction;
                return action;
            }
        }

        public TrelloNet.Action DoneAction
        {
            get
            {
                return GetDoneAction();
            }
        }

        public TrelloNet.Action GetStartAction()
        {
            TrelloNet.Action action = Actions.OfType<UpdateCardMoveAction>().Where(a => ListNames.StartListNames.Contains(a.Data.ListAfter.Name)).OrderBy(a => a.Date).FirstOrDefault();
            if (action == null)
                action = Actions.OfType<CreateCardAction>().Where(a => ListNames.StartListNames.Contains(a.Data.List.Name)).FirstOrDefault();
            if(action == null)
                action = Actions.OfType<ConvertToCardFromCheckItemAction>().Where(a => ListNames.StartListNames.Contains(a.Data.CardSource.Name)).FirstOrDefault();
            return action;

        }

        private UpdateCardMoveAction GetDoneAction()
        {
            if (IsInProgress || IsInTest)
            {
                return null;
            }
            var action = Actions.OfType<UpdateCardMoveAction>().Where(a => ListNames.DoneListNames.Contains(a.Data.ListAfter.Name)).OrderBy(a => a.Date).FirstOrDefault();

            if (action == null)
            {
                action = Actions.OfType<UpdateCardMoveAction>().Last();
            }

            return action;
        }

        public TimeSpan Duration
        {
            get
            {
                if (!IsInTest && !IsInProgress)
                    return GetDoneAction().DateInTimeZone(TimeZone).Subtract(EffectiveStartAction.DateInTimeZone(TimeZone));
                else
                    return TimeSpan.Zero;
            }
        }

        //TODO: Drop this business days feature or test the hell out of it
        public int BusinessDaysElapsed
        {
            get
            {
                if (!IsInProgress && !IsInTest)
                    return EffectiveStartAction.Date.BusinessDaysUntil(GetDoneAction().Date);
                else return 0;
            }
        }

        public bool IsComplete
        {
            get
            {
                return CardData.Card != null && EffectiveStartAction != null && GetDoneAction() != null;
            }
        }


        
    }
}
