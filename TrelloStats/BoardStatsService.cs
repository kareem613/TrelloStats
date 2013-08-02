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

        private readonly TimeZoneInfo _timeZone;
        private DateTime ProjectStartDate = new DateTime(2013,6,4,14,0,0);

        public BoardStatsService(TrelloService trelloService, TimeZoneInfo timeZone)
        {
            _trelloService = trelloService;
            _timeZone = timeZone;
        }

        public BoardStats BuildBoardStats(TrelloData trelloData)
        {
            var cardStats = new List<CardStats>();
            var badCards = new List<CardStats>();

            BuildCardStats(trelloData.Cards, badCards, cardStats);

            var listStats = new List<ListStats>();
            BuildListStats(trelloData.ListsToStat, listStats);
            
            var boardData = new BoardData();
            boardData.ProjectStartDate = ProjectStartDate;
            boardData.AddCardStats(cardStats);
            boardData.AddBadCardStats(badCards);
            boardData.ListStats = listStats;


            return new BoardStats(boardData, _timeZone);
        }

        private void BuildListStats(List<List> trelloLists, List<ListStats> listStats)
        {
            foreach (var trelloList in trelloLists)
            {
                List<TrelloNet.Card> cardsForList = _trelloService.GetCardsForList(trelloList);
                var listStat = new ListStats() { List = trelloList, CardCount = cardsForList.Count };
                listStats.Add(listStat);
            }
        }

        private void BuildCardStats(Dictionary<List,List<Card>> cards, List<CardStats> badCards, List<CardStats> cardStats)
        {
            foreach (var list in cards.Keys)
            {
                foreach (var card in cards[list])
                {
                    var stat = new CardStats() { Card = card, List = list, InProgressListName = _trelloService.InProgressListName, TimeZone = _timeZone };
                    
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
