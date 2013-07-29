using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TrelloNet;
using TrelloStats.Model;

namespace TrelloStats
{
    public class BoardStatsService
    {
        private readonly TrelloService _trelloService;

        private DateTime ProjectStartDate = new DateTime(2013,6,4,14,0,0);

        public BoardStatsService(TrelloService trelloService)
        {
            _trelloService = trelloService;
        }

        public BoardStats BuildBoardStats(Dictionary<List,List<Card>> cards)
        {
            var cardStats = new List<CardStats>();
            var badCards = new List<CardStats>();

            BuildCardStats(cards, badCards, cardStats);

            
            var boardData = new BoardData();
            boardData.ProjectStartDate = ProjectStartDate;
            boardData.AddCardStats(cardStats);
            boardData.AddBadCardStats(badCards);


            return new BoardStats(boardData);
        }

        private void BuildCardStats(Dictionary<List,List<Card>> cards, List<CardStats> badCards, List<CardStats> cardStats)
        {
            foreach (var list in cards.Keys)
            {

                foreach (var card in cards[list])
                {
                    var stat = new CardStats() { Card = card, List = list, InProgressListName = _trelloService.InProgressListName };

                    AddStartStats(stat, card);

                    if (!stat.IsInProgress)
                    {
                        AddCompleteStats(stat);
                    }

                    if (stat.IsComplete || stat.IsInProgress)
                        cardStats.Add(stat);
                    else
                        badCards.Add(stat);
                }
            }
        }
  
  
        private void AddStartStats(CardStats stat, Card card)
        {
            stat.Actions = _trelloService.GetActionsForCard<TrelloNet.Action>(card).OrderBy(c => c.Date);
            stat.FirstAction = stat.Actions.First();
            stat.StartAction = stat.Actions.OfType<UpdateCardMoveAction>().Where(a => _trelloService.StartListNames.Contains(a.Data.ListAfter.Name)).OrderBy(a => a.Date).FirstOrDefault();
            stat.Labels = card.Labels;
            var match = Regex.Match(card.Name, @"^\((.*)\)(.*)");
            if (match.Success)
            {
                var pointsString = match.Groups[1].Value;
                double points;
                if (double.TryParse(pointsString, out points))
                {
                    stat.Points = points;
                    stat.Card.Name = match.Groups[2].Value;
                }
            }
        }
  
        private void AddCompleteStats(CardStats stat)
        {
            stat.DoneAction = stat.Actions.OfType<UpdateCardMoveAction>().Where(a => _trelloService.DoneListNames.Contains(a.Data.ListAfter.Name)).OrderBy(a => a.Date).FirstOrDefault();
                  
            if (stat.DoneAction == null)
            {
                stat.DoneAction = stat.Actions.OfType<UpdateCardMoveAction>().Last();
            }
        }
    }
}
