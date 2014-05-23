using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMServer
{
    class Logger
    {

        public void Log(Level l, string msg)
        {
            Console.ForegroundColor = l.Color;
            Log(msg);
            Console.ResetColor();
        }

        public void Log(string msg){
            Console.WriteLine(String.Format("{0:d/M/yyyy HH:mm:ss}", DateTime.Now)+": "+msg);
        }

        public class Level
        {
            public static Level Warning = new Level(ConsoleColor.Yellow);
            public static Level MySQLWarning = new Level(ConsoleColor.Red);
            public ConsoleColor Color { get; set; }
            private Level(ConsoleColor color)
            {
                Color = color;
            }
            
        }

    }
}
