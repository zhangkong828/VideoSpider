using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VideoSpider.Infrastructure;

namespace VideoSpider.Services
{
    public class SpiderService
    {
        private static readonly object _obj = new object();
        private static SpiderService _instance;

        private ConcurrentQueue<string> _queue;

        private SpiderService()
        {
            _queue = new ConcurrentQueue<string>();
        }

        public static SpiderService Create()
        {
            if (_instance == null)
            {
                lock (_obj)
                {
                    if (_instance == null)
                    {
                        _instance = new SpiderService();
                    }
                }
            }
            return _instance;
        }


        public void Start()
        {
            BeginThread(10);
            //Logger.ColorConsole("开始获取Url", ConsoleColor.Blue);
            //var url = "http://caijizy.com/?m=vod-index-pg-{0}.html";
            //for (int i = 1; i < 340; i++)
            //{
            //    try
            //    {
            //        var targetUrl = string.Format(url, i);
            //        Logger.ColorConsole2(targetUrl, ConsoleColor.Blue);

            //        var html = HtmlHelper.Get(targetUrl).Result;
            //        var list = Regex.Matches(html, "<td class=\"l\"><a href=\"(.+?)\" target=\"_blank\">(.+?)<font color=\"red\"> \\[(.*?)\\]</font>");
            //        foreach (Match item in list)
            //        {
            //            var dUrl = item.Groups[1].Value;
            //            var dName = item.Groups[2].Value;
            //            var dRemark = item.Groups[3].Value;
            //            var info = string.Format("{0}[{1}] {2}", dName, dRemark, dUrl);
            //            _queue.Enqueue(dUrl);
            //            Logger.ColorConsole2(info);
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Logger.ColorConsole2(ex.Message, ConsoleColor.Red);
            //    }
            //    finally
            //    {
            //        Thread.Sleep(1000);
            //    }
            //}

        }

        public void Stop()
        {

        }

        private void BeginThread(int threadCount)
        {
            for (int i = 0; i < threadCount; i++)
            {
                Logger.ColorConsole(string.Format("开启线程[{0}]", i + 1), ConsoleColor.Blue);

                var t = new Thread(new ParameterizedThreadStart(index =>
                {
                    Logger.ColorConsole2(string.Format("线程[{0}]已启动,线程ID:{1}", index, Thread.CurrentThread.ManagedThreadId), ConsoleColor.Blue);

                }));
                t.IsBackground = true;
                t.Start(i);

            }
        }
    }
}
