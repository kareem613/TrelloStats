using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrelloStats.Model.Data;

namespace TrelloStats.Tests
{
    public static class ListDataFactory
    {

        public static ListData GetListData(string listName)
        {
            var list = new TrelloNet.List() { Name = listName };
            var listData = new ListData(list);
            return listData;
        }

    }
}
