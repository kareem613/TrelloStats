﻿using System;
using System.Collections.Generic;
using TrelloNet;
using System.Linq;

namespace TrelloStats.Model
{
    public class CardStats
    {
        public CardData CardData { get; set; }

        public ListData ListData { get; set; }

        public TrelloNet.Action FirstAction
        {
            get
            {
                return Actions.OrderBy(a => a.Date).First();
            }
        }

        public IEnumerable<TrelloNet.Action> Actions
        {
            get { return CardData.Actions; }
        }

        //TODO:// Move this config out of this stats class
        public ListNameConfiguration ListNames { get; set; }

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

        public TrelloNet.Action EffectiveStartAction
        {
            get
            {
                return GetStartAction() ?? FirstAction;
            }
        }

        public UpdateCardMoveAction GetStartAction()
        {
            return Actions.OfType<UpdateCardMoveAction>().Where(a => ListNames.StartListNames.Contains(a.Data.ListAfter.Name)).OrderBy(a => a.Date).FirstOrDefault();
        }

        public UpdateCardMoveAction GetDoneAction()
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
                return GetDoneAction().DateInTimeZone(TimeZone).Subtract(GetStartAction().DateInTimeZone(TimeZone));
            }
        }

        public int BusinessDaysElapsed
        {
            get
            {
                return EffectiveStartAction.Date.BusinessDaysUntil(GetDoneAction().Date);
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
