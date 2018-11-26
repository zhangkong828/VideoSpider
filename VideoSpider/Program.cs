using System;
using VideoSpider.Infrastructure;
using VideoSpider.Services;

namespace VideoSpider
{
    class Program
    {
        static void Main(string[] args)
        {
            //捕获Ctrl+C事件
            Console.CancelKeyPress += Console_CancelKeyPress;
            //进程退出事件
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;


            SpiderService.Create().Start();
            Console.WriteLine("执行结束");
            Console.ReadKey();
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Logger.ColorConsole("Services.ProcessExit");
            Stop();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Logger.ColorConsole("Services.CancelKeyPress");
            Stop();
        }

        private static void Stop()
        {
            SpiderService.Create().Stop();
        }
    }
}
