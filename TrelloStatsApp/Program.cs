
using System;
namespace TrelloStatsApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var stats = new TrelloStats.TrelloToGoogleService();
                stats.CalculateStats();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ReadLine();
            }
        }
    }
}
