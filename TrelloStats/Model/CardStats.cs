using System;
using System.Collections.Generic;
using TrelloNet;
using System.Linq;

namespace TrelloStats.Model
{
    public class CardStats
    {
        public double Points;

        public List<Card.Label> Labels;

        public TrelloNet.Action FirstAction;

        public IEnumerable<TrelloNet.Action> Actions;

        public Card Card { get; set; }

        public bool IsInProgress
        {
            get
            {
                return List.Name == TrelloService.IN_PROGRESS_LIST_NAME;
            }
            
        }

        public TrelloNet.Action EffectiveStartAction
        {
            get
            {
                return StartAction ?? FirstAction;
            }
        }

        public UpdateCardMoveAction StartAction { get; set; }

        public UpdateCardMoveAction DoneAction { get; set; }

        public TimeSpan Duration
        {
            get
            {
                return DoneAction.Date.Subtract(StartAction.Date);
            }
        }

        public int BusinessDaysElapsed
        {
            get
            {
                return EffectiveStartAction.Date.BusinessDaysUntil(DoneAction.Date);
            }
        }

        public bool IsComplete
        {
            get
            {
                return Card != null && EffectiveStartAction != null && DoneAction != null;
            }
        }



        public List List { get; set; }
    }
}
