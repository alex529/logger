using System;

namespace LogUsers
{
    using LogComponent;
    using System.Threading;

    internal static class Program
    {
        private static void Main(string[] args)
        {
            var x = new Random((int)DateTime.Now.Ticks);
            var logger = new Logger(new FileWriter(@"C:\LogTest", $"log{x.Next(100, 999)}"));

            for (int i = 0; i < 15; i++)
            {
                logger.Write("Number with Flush: " + i.ToString());
                Thread.Sleep(50);
            }

            logger.StopWithFlush();

            var logger2 = new Logger(new FileWriter(@"C:\LogTest", $"log{x.Next(100, 999)}"));

            for (int i = 50; i > 0; i--)
            {
                logger2.Write("Number with No flush: " + i.ToString());
                Thread.Sleep(20);
            }

            logger2.StopWithoutFlush();

            Console.ReadLine();
        }
    }
}
