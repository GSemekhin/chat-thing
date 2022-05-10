using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BotTW
{
    class Logs
    {
        StreamWriter outputFile;


        public Logs()
        {
            string logDirectory = Path.Combine(Environment.CurrentDirectory);
            outputFile = new StreamWriter(Path.Combine(logDirectory, "logs" + DateTime.UtcNow.ToString("ddMMyyyy HHmmsffff") + ".txt"), true);
            outputFile.AutoFlush = true;
        }



        public void WriteLog(string data)
        {
            try
            {
                outputFile.WriteLine(data);
            }
            catch
            {
                Console.WriteLine("Не получилось записать в лог " + data);
            }
            //Console.WriteLine(data);

        }


    }
}

