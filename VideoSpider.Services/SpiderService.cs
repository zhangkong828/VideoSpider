using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VideoSpider.Infrastructure;
using VideoSpider.Model;

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
            StartThread(10);


        }

        public void Stop()
        {

        }

        private void StartThread(int threadCount)
        {
            for (int i = 0; i < threadCount; i++)
            {
                Logger.ColorConsole(string.Format("开启线程[{0}]", i + 1), ConsoleColor.Blue);

                var task = Task.Factory.StartNew(index =>
                {
                    Logger.ColorConsole2(string.Format("线程[{0}]已启动,线程ID:{1}", index, Thread.CurrentThread.ManagedThreadId), ConsoleColor.Blue);
                    try
                    {
                        var url = string.Empty;
                        if (_queue.TryDequeue(out url))
                        {
                            DoWork(url);
                        }
                        else
                        {
                            Thread.Sleep(1000 * 10);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.ColorConsole2(ex.Message, ConsoleColor.Red);
                    }
                }, i + 1);

            }
        }


        private void CheckUpdate()
        {
            Logger.ColorConsole("开始获取更新", ConsoleColor.Blue);
            var url = "http://caijizy.com/?m=vod-index-pg-{0}.html";
            for (int i = 1; i < 340; i++)
            {
                try
                {
                    var targetUrl = string.Format(url, i);
                    Logger.ColorConsole2(targetUrl, ConsoleColor.Blue);

                    var html = HtmlHelper.Get(targetUrl).Result;
                    var list = Regex.Matches(html, "<td class=\"l\"><a href=\"(.+?)\" target=\"_blank\">(.+?)<font color=\"red\"> \\[(.*?)\\]</font>");
                    foreach (Match item in list)
                    {
                        var dUrl = item.Groups[1].Value;
                        var dName = item.Groups[2].Value;
                        var dRemark = item.Groups[3].Value;
                        var info = string.Format("{0}[{1}] {2}", dName, dRemark, dUrl);
                        dUrl = "http://caijizy.com" + dUrl;
                        _queue.Enqueue(dUrl);
                        Logger.ColorConsole2(info);
                    }
                }
                catch (Exception ex)
                {
                    Logger.ColorConsole2(ex.Message, ConsoleColor.Red);
                }
                finally
                {
                    Thread.Sleep(1000);
                }
            }
        }


        private void DoWork(string url)
        {
            //去重
            var html = HtmlHelper.Get(url).Result;
            if (!string.IsNullOrEmpty(html))
            {
                var video = new Video();
                video.Id = ObjectId.NextId();
                video.CreateTime = DateTime.Now;
                video.UpdateTime = DateTime.Now;

                //图片
                video.Image = Regex.Match(html, "<!--图片开始--><img src=\"(.*?)\"/><!--图片结束-->").Groups[1].Value;
                //名称
                video.Name = Regex.Match(html, "<!--片名开始-->(.*?)<!--片名结束-->").Groups[1].Value;
                //别名
                video.Alias = Regex.Match(html, "<!--别名开始-->(.*?)<!--别名结束-->").Groups[1].Value;
                //备注
                video.Remark = Regex.Match(html, "<!--备注开始-->(.*?)<!--备注结束-->").Groups[1].Value;
                //主演
                video.Starring = Regex.Match(html, "<!--主演开始-->(.*?)<!--主演结束-->").Groups[1].Value;
                //导演
                video.Director = Regex.Match(html, "<!--导演开始-->(.*?)<!--导演结束-->").Groups[1].Value;
                //栏目分类
                video.Classify = Regex.Match(html, "<!--栏目开始-->(.*?)<!--栏目结束-->").Groups[1].Value;
                //影片类型
                video.Type = Regex.Match(html, "<!--类型开始-->(.*?)<!--类型结束-->").Groups[1].Value;
                //语言分类
                video.Language = Regex.Match(html, "<!--语言开始-->(.*?)<!--语言结束-->").Groups[1].Value;
                //影片地区
                video.Region = Regex.Match(html, "<!--地区开始-->(.*?)<!--地区结束-->").Groups[1].Value;
                //连载状态
                video.State = Regex.Match(html, "<!--连载开始-->(.*?)<!--连载结束-->").Groups[1].Value;
                //上映年份
                video.ReleaseDate = Regex.Match(html, "<!--年代开始-->(.*?)<!--年代结束-->").Groups[1].Value;
                //更新时间
                var OriginalUpdateTime = Regex.Match(html, "<!--时间开始-->(.*?)<!--时间结束-->").Groups[1].Value;
                //简介
                video.Description = Regex.Match(html, "<!--简介开始-->(.*?)<!--简介结束-->").Groups[1].Value;

                //jsm3u8
                var jsm3u8 = Regex.Match(html, "<!--前jsm3u8-->(.*?)<!--后jsm3u8-->").Groups[1].Value;
                var list = Regex.Matches(jsm3u8, "<input.+?value=\"(.+?)\\$(.+?)\".+?>");
                foreach (Match item in list)
                {
                    var title = item.Groups[1].Value;
                    var address = item.Groups[2].Value;
                    Logger.ColorConsole2(string.Format("[{0}]{1}", title, address));
                }
            }
        }


    }
}
