using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrelloStats.Configuration;

namespace TrelloStats.Tests
{
    public static class ConfigurationFactory
    {
        public const string DEFAULT_START_LIST_NAME = "Start";
        public const string DEFAULT_IN_PROGRESS_LIST_NAME = "InProgress";
        public const string DEFAULT_IN_TEST_LIST_NAME = "InTest";
        public const string DEFAULT_DONE_LIST_NAME = "Done";


        public static IListNameConfiguration CreateListNamesConfigurationStub()
        {
            var configStub = Substitute.For<IListNameConfiguration>();
            configStub.StartListNames.Returns(new string[] { DEFAULT_START_LIST_NAME });
            configStub.InProgressListName.Returns( DEFAULT_IN_PROGRESS_LIST_NAME);
            configStub.InTestListName.Returns(DEFAULT_IN_TEST_LIST_NAME);
            configStub.DoneListNames.Returns(new string[] { DEFAULT_DONE_LIST_NAME });
            return configStub;
        }
    }
}
