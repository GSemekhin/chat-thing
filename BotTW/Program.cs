using System;
using System.Threading;


namespace BotTW
{

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
          
            Bot bot = new Bot();

            Logs log = new Logs();

            while (true)
            {
                while (true)
                {
                    //Console.WriteLine("+");
                    log.WriteLog(DateTime.UtcNow.ToString() + " Выполняется бесконечный цикл");
                    bot.TwitchLifeSupport();
                    Thread.Sleep(5000);
                }
            }
        }
    }
}
