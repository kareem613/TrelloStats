
using System;
using TrelloStats.Services;
namespace TrelloStatsApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var stats = new TrelloToGoogleService();
                stats.CalculateStats(false, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

            }

            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
        }
    }
}
